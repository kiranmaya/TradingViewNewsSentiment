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
            lastReqTime = DateTime.Now;
            StartTime= DateTime.Now;
            LoadSymbols();
            // Set up the timer to call LoopSymbols every 2 minutes
            symbolLoopTimer = new Timer(LoopSymbolsCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
        }
        public static DateTime lastReqTime;
        public static DateTime StartTime;
        private static async void LoopSymbolsCallback(object state)
        {
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.WriteLine($"Call LoopSymbols function  {DateTime.Now.ToLongTimeString()}");

            if( ( DateTime.Now - lastReqTime ).TotalMinutes > 60 )//minutes
            {
                Console.WriteLine($"No NEWS Request came in last  {( DateTime.Now - lastReqTime ).TotalMinutes}minutes ,So waiting for request ");
            }
            else
            {
                var res = await LoopSymbols();
                Console.WriteLine($"{DateTime.Now} LoopSymbols res => {res}");
            }

            Console.WriteLine($" Runtime {(DateTime.Now-StartTime).TotalDays} days   ");

        }
        static IndexStocksNSE nifty50;
        public static ConcurrentDictionary<string, string> NewsData = new ConcurrentDictionary<string, string>();
        public static async Task<string> LoadSymbols()
        {
            try { 
            if( nifty50 == null) { 
            nifty50 = JsonConvert.DeserializeObject<IndexStocksNSE>(File.ReadAllText(@"C:\wamp64\www\zerodha\NseFututes.json"));
            }
            Console.WriteLine($"Nifty50 {nifty50?.data?.Count}");
            Console.WriteLine();
            }
            catch( Exception e ) { Console.WriteLine(e.Message); }
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
                    while( WebSearch.isBusy )
                    {
                        await Task.Delay(100); // Adjust the delay based on your needs
                    }
                    if( NewsData.ContainsKey(item.symbol) == false )
                    {
                        NewsData.TryAdd(item.symbol, "{}");
                    }

                    var result = await WebSearch.Search(item.symbol);
                    NewsData[item.symbol] = result;
                    await Task.Delay(1000);
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
                NewsData.TryAdd(query, "{}");
            }
        
            return NewsData[query];
        }
    }
}
