using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WebScrapper
{
    public static class NiftySymbols
    {
        private static Timer symbolLoopTimer;

        public static void StartSymbolLoop()
        {
            LoadSymbols();
            // Set up the timer to call LoopSymbols every 2 minutes
            symbolLoopTimer = new Timer(LoopSymbolsCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(300));
        }

        private static async void LoopSymbolsCallback(object state)
        {
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.WriteLine($"Call LoopSymbols function  {DateTime.Now.ToLongTimeString()}");
            await   LoopSymbols();
        }
        static IndexStocksNSE nifty50;
        public static ConcurrentDictionary<string, string> NewsData = new ConcurrentDictionary<string, string>();
        public static async Task<string> LoadSymbols()
        {
            if( nifty50 == null) { 
            nifty50 = JsonConvert.DeserializeObject<IndexStocksNSE>(File.ReadAllText(@"C:\wamp64\www\zerodha\NIFTY50.json"));
            }
            Console.WriteLine($"Nifty50 {nifty50?.data?.Count}");
            Console.WriteLine();
            return "Done";
        }

        public static bool isInLoop = false;
        public static async Task<string> LoopSymbols()
        {
            if( isInLoop ) return "return LoopSymbols isInLoop";
 
            isInLoop = true;
            foreach( var item in nifty50?.data )
            {
                try
                {

                    if( NewsData.ContainsKey(item.symbol) == false )
                    {
                        NewsData.TryAdd(item.symbol, "");
                    }
                    var result = await WebSearch.Search(item.symbol);
                    NewsData[item.symbol] = result;
                }
                catch( Exception e ) { Console.WriteLine(e.Message); }
            }
            isInLoop = false;
            return "";
        }
        public static async Task<string> GetNews(string query)
        {
          
            await Task.Delay(20);
            if( NewsData.ContainsKey(query) == false )
            {
                NewsData.TryAdd(query, "");
            }
            return NewsData[query];
        }
    }
}
