using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace WebScrapper
{
    public class WebSearch
    {
        static ChromeDriver driver;
        static readonly ConcurrentDictionary<string, CachedResult> cache = new ConcurrentDictionary<string, CachedResult>();

        static bool isBusy = false;
        public static async Task<string> Search(string query)
        {



            // Check if result is cached and not expired
            if( cache.TryGetValue(query, out var cachedResult) && DateTime.Now.Subtract(cachedResult.Timestamp).TotalMinutes <= 3 )
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

     

            var articleElements = driver.FindElements(By.XPath("//article[contains(@class, 'card-exterior')]"));

            Console.WriteLine("articleElements   " + articleElements.Count);

            // Iterate through each article and extract title and date
            Dictionary<DateTime, string> news = new Dictionary<DateTime, string>();

            AllNews all = new AllNews();
            all.lasttime = DateTime.Now;
            all.Symbol = query;

            foreach( var article in articleElements.Take(5) ) // Assuming you want to process the first 5 articles
            {
                try
                {
                    var title = article.FindElement(By.CssSelector("[class^='title-']")).Text;
                    var dateElement = article.FindElement(By.CssSelector(".date-TUPxzdRV relative-time"));
                    var ssrTime = dateElement.GetAttribute("event-time");
                    string dateString = ssrTime;
                    DateTime dateTime = DateTime.ParseExact(dateString, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", System.Globalization.CultureInfo.InvariantCulture);

                    if( dateTime.AddDays(10) > DateTime.Now )
                    {
                        Console.WriteLine($"Title: {title}");
                        Console.WriteLine(dateTime.ToString("dd-MM-yyy HH:mm:ss")); // Display in a specific format
                        Console.WriteLine();
                        news.Add(dateTime.ToLocalTime(), title);
                    }
                }
                catch( Exception e )
                {
                    isBusy = false;

                    // Handle exceptions during article processing
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Done ");
            all.cacheval = news;

            // Cache the result
            cache.AddOrUpdate(query, new CachedResult { JsonResult = JsonConvert.SerializeObject(all), Timestamp = DateTime.Now }, (_, existing) => new CachedResult { JsonResult = JsonConvert.SerializeObject(all), Timestamp = DateTime.Now });
            isBusy = false;

            return JsonConvert.SerializeObject(all);
        }
    }

    public class AllNews
    {
        public DateTime lasttime;
        public string Symbol;
        public Dictionary<DateTime, string> cacheval;
    }

    public class CachedResult
    {
        public string JsonResult { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
