using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using Trady.Analysis;
using Trady.Analysis.Extension;
using Trady.Analysis.Infrastructure;
using Trady.Core;
using Trady.Core.Infrastructure;
using Trady.Importer.Yahoo;

namespace WebScraper
{
    public class HLC3
    {
        public static double CalculateHLCC(double high, double low, double close)
        {
            return (high + low + close) / 3;
        }

        public static double CalculateHLCL(double high, double low, double close)
        {
            return (high + 2 * low + 2 * close) / 5;
        }

        public static double CalculateHLCClose(double high, double low, double close)
        {
            return (high + low + 2 * close) / 4;
        }
    }

    public class FeaturesX
    {
        public string Name { get; set; }
        public List<double> Values { get; set; }

        public FeaturesX(string name, List<double> values)
        {
            Name = name;
            Values = values;
        }
    }

    public class Diction
    {
        public enum Direction
        {
            LONG = 1,
            SHORT = -1,
            NEUTRAL = 0
        }

        public static Direction GetDirection(double score)
        {
            if(score < 0)
                return Direction.SHORT;
            else if(score > 0)
                return Direction.LONG;
            else
                return Direction.NEUTRAL;
        }
    }

    public class KNode
    {
        public List<KNode> NeighborNodes { get; set; }
        public double Prediction { get; set; }
        public Diction.Direction Direction { get; set; }
        public List<double> Features { get; set; }
        public object Info { get; set; }

        public KNode(List<double> features, object info = null, double prediction = 0)
        {
            Features = features;
            Info = info;
            Prediction = prediction;
            NeighborNodes = new List<KNode>();
            Direction = Diction.GetDirection(prediction);
        }

        public void Score(List<KNode> neighborNodes)
        {
            NeighborNodes = neighborNodes;
            Prediction = neighborNodes.Sum(neighbor => neighbor.Prediction);
            Direction = Diction.GetDirection(Prediction);
        }
    }

    public class NearestNeighbor
    {
        public KNode Node { get; set; }
        public double Distance { get; set; }

        public NearestNeighbor(KNode node, double distance)
        {
            Node = node;
            Distance = distance;
        }
    }

    public class FeaturesDefinition
    {
        private List<FeaturesX> Features { get; }

        public FeaturesDefinition()
        {
            Features = new List<FeaturesX>();
        }

        public void AddFeature(string name, List<double> values)
        {
            Features.Add(new FeaturesX(name, values));
        }

        public List<FeaturesX> GetAllFeatures()
        {
            return Features;
        }

        public List<string> GetFeatureNames()
        {
            return Features.Select(f => f.Name).ToList();
        }

        public int GetNumFeatures()
        {
            return Features.Count;
        }

        public static FeaturesDefinition Default(List<double> high = null, List<double> low = null, List<double> close = null, List<double> open = null, List<double> vol = null)
        {
            FeaturesDefinition featuresDefinition = new FeaturesDefinition();
            // Add default features
            return featuresDefinition;
        }

        public List<double> Calculate(List<List<double>> df)
        {
            List<double> result = new List<double>();

            var ohlcvList = df.Select(row => (decimal)row[4]).ToList();

            foreach(var feature in GetAllFeatures())
            {
                switch(feature.Name)
                {
                    case "SMA":
                        for(int i = 0; i < ohlcvList.Count; i++)
                        {
                            var sma = ohlcvList.Sma(14, i);
                            result.AddRange(sma.Select(d => (double)d).ToList());
                        }
                        break;
                    case "EMA":
                        for(int i = 0; i < ohlcvList.Count; i++)
                        {
                            var ema = ohlcvList.Ema(14, i);
                            result.AddRange(ema.Select(d => (double)d).ToList());
                        }
                        break;
                    // Add more cases for other indicators
                    default:
                        // Handle unknown indicators
                        break;
                }
            }

            return result;
        }
    }

    public class MlClassification
    {
        public static double LorentzianDistance(List<double> x, List<double> y, int size)
        {
            if(x.Count != y.Count)
                throw new ArgumentException("x and y must have the same length");

            return x.Select((t, i) => Math.Log(1 + Math.Abs(t - y[i]))).Sum();
        }

        public static List<NearestNeighbor> LorentzianKNearestNeighbor(KNode x, List<KNode> yArr, int size, int k = 8)
        {
            List<NearestNeighbor> minDistKNodes = new List<NearestNeighbor>();
            foreach(var y in yArr)
            {
                double distance = LorentzianDistance(x.Features, y.Features, size);
                if(minDistKNodes.Count < k)
                {
                    minDistKNodes.Add(new NearestNeighbor(y, distance));
                    minDistKNodes.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                }
                else if(distance < minDistKNodes.Last().Distance)
                {
                    minDistKNodes.Add(new NearestNeighbor(y, distance));
                    minDistKNodes.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                    minDistKNodes.RemoveAt(minDistKNodes.Count - 1);
                }
            }

            return minDistKNodes;
        }

        public static List<KNode> TrainDataByDf(List<List<double>> df, FeaturesDefinition featuresDefinition, FeaturesX source, int futureCount = 4)
        {
            List<KNode> trainData = new List<KNode>();
            int position = 0;
            foreach(var row in df)
            {
                int nextPosition = position + futureCount;
                position++;
                if(nextPosition < df.Count)
                {
                    List<double> features = featuresDefinition.GetAllFeatures().Select(name => row[featuresDefinition.GetAllFeatures().IndexOf(name)]).ToList();
                    List<double> targetRow = df[nextPosition];
                    KNode kNode = new KNode(features, prediction: (double)Diction.GetDirection(targetRow[featuresDefinition.GetAllFeatures().IndexOf(source)] - row[featuresDefinition.GetAllFeatures().IndexOf(source)]));
                    trainData.Add(kNode);
                }
            }
            return trainData;
        }

        public static List<KNode> TestDataByDf(List<List<double>> df, FeaturesDefinition featuresDefinition, FeaturesX source, FeaturesX low, FeaturesX high, int futureCount = 4)
        {
            List<KNode> testData = new List<KNode>();
            int position = 0;
            foreach(var row in df)
            {
                int nextPosition = position + futureCount;
                position++;
                if(nextPosition < df.Count)
                {
                    List<double> features = featuresDefinition.GetAllFeatures().Select(name => row[featuresDefinition.GetAllFeatures().IndexOf(name)]).ToList();
                    List<double> targetRow = df[nextPosition];

                    Diction.Direction theDirection = Diction.Direction.NEUTRAL;
                    if(targetRow[featuresDefinition.GetAllFeatures().IndexOf(source)] > row[featuresDefinition.GetAllFeatures().IndexOf(source)] && targetRow[featuresDefinition.GetAllFeatures().IndexOf(low)] > row[featuresDefinition.GetAllFeatures().IndexOf(low)])
                    {
                        theDirection = Diction.Direction.LONG;
                    }
                    else if(targetRow[featuresDefinition.GetAllFeatures().IndexOf(source)] < row[featuresDefinition.GetAllFeatures().IndexOf(source)] && targetRow[featuresDefinition.GetAllFeatures().IndexOf(high)] < row[featuresDefinition.GetAllFeatures().IndexOf(high)])
                    {
                        theDirection = Diction.Direction.SHORT;
                    }
                    KNode kNode = new KNode(features, prediction: (double)Diction.GetDirection(targetRow[featuresDefinition.GetAllFeatures().IndexOf(source)] - row[featuresDefinition.GetAllFeatures().IndexOf(source)]), info: row);
                    testData.Add(kNode);
                }
            }
            return testData;
        }

        public static KNode Prediction2Node(List<double> row, List<KNode> trainData, FeaturesDefinition featuresDefinition, int k = 8)
        {
            List<double> features = featuresDefinition.GetAllFeatures().Select(name => row[featuresDefinition.GetAllFeatures().IndexOf(name)]).ToList();
            KNode currentNode = new KNode(features);
            List<NearestNeighbor> neighborNode = LorentzianKNearestNeighbor(currentNode, trainData, featuresDefinition.GetAllFeatures().Count, k);
            currentNode.Score(neighborNode.Select(n => n.Node).ToList());
            return currentNode;
        }

        static void Main()
        {
            string symbol = "AAPL"; // Stock symbol
            DateTimeOffset from = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero); // Start date
            DateTimeOffset to = DateTimeOffset.UtcNow; // End date

            var importer = new YahooFinanceImporter();
            IEnumerable<IOhlcv> data = importer.ImportAsync(symbol, from.DateTime, to.DateTime).Result;

            // Create a data frame from the imported data
            var df = data.Select(d => new List<double> { (double)(double)d.Open, (double)(double)d.High, (double)d.Low, (double)d.Close, (double)d.Volume }).ToList();

            // Create a FeaturesDefinition object with Trady indicators
            var featuresDefinition = FeaturesDefinition.Default(df.Select(d => d[1]).ToList(), df.Select(d => d[2]).ToList(), df.Select(d => d[3]).ToList(), df.Select(d => d[0]).ToList(), df.Select(d => d[4]).ToList());

            // Calculate the indicators
            var calculatedFeatures = featuresDefinition.Calculate(df);

            // Access the calculated features
            foreach(var feature in calculatedFeatures)
            {
                Console.WriteLine($"{feature}: {string.Join(", ", feature)}");
            }
        }
    }
}
