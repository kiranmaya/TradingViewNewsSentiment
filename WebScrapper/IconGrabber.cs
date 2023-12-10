using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using GoogleApi.Entities.Search.Common;

namespace WebScrapper
{
    public class SymbolData
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string iconURL { get; set; }
    }
    public class IconGrabber
    {


        static ChromeDriver driver;
        static readonly ConcurrentDictionary<string, CachedResult> cache = new ConcurrentDictionary<string, CachedResult>();
        static ConcurrentDictionary<string, string> cacheSentiment = new ConcurrentDictionary<string, string>();
        public static bool isBusy = false;

        static DateTime retreiveTime;

        public static async Task<string> saveCons( )
        {
            var SymbolDataDict = JsonConvert.DeserializeObject<Dictionary<string, SymbolData>>(File.ReadAllText(@"C:\wamp64\www\zerodha\symbolsInfo.txt"));

            using( HttpClient client = new HttpClient() )
            {
                foreach( var item in SymbolDataDict.Values )
                {
                    try { 
                    string fileName = Path.Combine(@"D:\Projs\BlazorTrading\LykeeServer\wwwroot\Icons\", item.Symbol + ".svg");
                        Console.WriteLine(fileName);
                        // Download the file
                        if( item.iconURL != null )
                        {

                  
                        byte[] fileBytes = await client.GetByteArrayAsync(item.iconURL);

                    // Save the file to the specified folder path
                    await File.WriteAllBytesAsync(fileName, fileBytes);
                        }
                    }
                    catch( Exception ex ) { Console.WriteLine(ex.Message); }
                }
            }

            return "done";
        }


        static Dictionary<string, SymbolData> symbolDict = new Dictionary<string, SymbolData>();
        public static async Task<string> LoopSymbols()
        {
            var nifty50 = JsonConvert.DeserializeObject<IndexStocksNSE>(File.ReadAllText(@"C:\wamp64\www\zerodha\NseFututes.json"));
            foreach( var item in nifty50?.data )
            {
                SymbolData symbolData = new SymbolData();
                symbolData.Symbol = item.symbol;
                symbolData.Name = item.meta.companyName;

                await Search(item.symbol, symbolData);
                symbolDict.Add(item.symbol, symbolData);
                await Task.Delay(2000);
            }


            var result = JsonConvert.SerializeObject(symbolDict);

            File.WriteAllText(@"C:\wamp64\www\zerodha\symbolsInfo.txt", result);
            Console.WriteLine(result);

            return "";
        }

        public static async Task<string> Search(string query, SymbolData symbolData)
        {
            retreiveTime = DateTime.Now;
            Console.WriteLine($"Getting IconGrabber for  {query}");

            // Check if result is cached and not expired
            if( cache.TryGetValue(query, out var cachedResult) && DateTime.Now.Subtract(cachedResult.Timestamp).TotalMinutes <= 2 )
            {
                Console.WriteLine($"Returning cached result for {query}");
                isBusy = false;
                return cachedResult.JsonResult;
            }

            if( isBusy ) return "";

            isBusy = true;
            string chromeExePath = @"C:\wamp64\www\zerodha\Dependencies\Chrome-bin\chrome.exe";

            // Set up ChromeOptions with the custom executable path
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            chromeOptions.AddArgument("--window-size=800,600");

            chromeOptions.AddArgument("--headless");
            chromeOptions.BinaryLocation = chromeExePath;
            string chromeDriverPath = @"C:\wamp64\www\zerodha\chromedriver.exe";

            if( driver == null ) driver = new ChromeDriver(chromeDriverPath, chromeOptions);

            query = query.Replace(" ", "-");
            string url = $"https://www.tradingview.com/symbols/NSE-{query}/news/";
            driver.Url = url;

            Console.WriteLine(driver.Title);

            try
            {
                await Task.Delay(300);
                IWebElement closeButton = driver.FindElement(By.CssSelector("div.tv-dialog__close"));
                closeButton.Click();
            }
            catch( Exception e )
            {
                Console.WriteLine("waiting done " + e.Message);
            }

            Console.WriteLine("waiting 1 ");
            await Task.Delay(200);
            Console.WriteLine("waiting done ");

            Console.WriteLine("Done ");

            try
            {
                // Find the <img> element by its class name
                IWebElement imgElement = driver.FindElement(By.CssSelector("img.tv-circle-logo.tv-circle-logo--xxxlarge.large-xoKMfU7r"));

                // Get the source (src) attribute value
                string srcAttributeValue = imgElement.GetAttribute("src");

                // Get the alt attribute value
                string altAttributeValue = imgElement.GetAttribute("alt");

                symbolData.iconURL= srcAttributeValue; 
                // Output the values
                Console.WriteLine("Source (src): " + srcAttributeValue);
                Console.WriteLine("Alt: " + altAttributeValue);
            }
            catch( NoSuchElementException ex )
            {
                Console.WriteLine("Element not found: " + ex.Message);
            }

            isBusy = false;


            return "";

        }
    }
}
