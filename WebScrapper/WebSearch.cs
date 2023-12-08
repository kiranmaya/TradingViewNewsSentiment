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
        static ConcurrentDictionary<string, string> cacheSentiment = new ConcurrentDictionary<string, string>();
        public static bool isBusy = false;

        static DateTime retreiveTime;
        public static async Task<string> Search(string query)
        {
            retreiveTime = DateTime.Now;
            Console.WriteLine($"Getting News for  {query}");

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

            var articleElements = driver.FindElements(By.XPath("//article[contains(@class, 'card-exterior')]"));

            Console.WriteLine("articleElements   " + articleElements.Count);

            // Iterate through each article and extract title and date

            List<News> AllNews = new List<News>();

            foreach( var article in articleElements.Take(9) ) // Assuming you want to process the first 5 articles
            {
                try
                {
                    IWebElement element = article.FindElement(By.CssSelector("span[class*='block-'] span"));

                    string providerText = element.Text;
                    // Output the value
                    Console.WriteLine($"Provider Value: {providerText}");

                    var title = article.FindElement(By.CssSelector("[class^='title-']")).Text;
                    var dateElement = article.FindElement(By.CssSelector(".date-TUPxzdRV relative-time"));
                    var ssrTime = dateElement.GetAttribute("event-time");
                    string dateString = ssrTime;
                    DateTime newsTims = DateTime.ParseExact(dateString, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", System.Globalization.CultureInfo.InvariantCulture).ToLocalTime();
                    //<span>Dow Jones Newswires</span>
                    if( newsTims.AddDays(10) > DateTime.Now && (providerText== "Reuters"  || providerText.Contains("Dow Jones Newswires") ) ) // || providerText.Contains("MT Newswires")
                    {
                        News news1 = new News();
                        news1.lasttime = DateTime.Now;
                        news1.provider = providerText;
                        news1.news = title;
                        news1.Symbol = query;
                        news1.newsTime= newsTims;

                        if( cacheSentiment.ContainsKey(news1.news) )
                        {
                            news1.sentiment = cacheSentiment[news1.news];
                        }
                        else
                        {
                            var res = await news1.FillSentiment();
                            cacheSentiment.TryAdd(news1.news, res);
                        }
                      
                        AllNews.Add(news1);

                        Console.WriteLine($"Title: {title}");
                        Console.WriteLine(newsTims.ToString("dd-MM-yyy HH:mm:ss")); // Display in a specific format
                        Console.WriteLine();

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
     

            // Cache the result
            if( AllNews.Count > 0 ) { 
            cache.AddOrUpdate(query, new CachedResult { JsonResult = JsonConvert.SerializeObject(AllNews), Timestamp = DateTime.Now }, (_, existing) => new CachedResult { JsonResult = JsonConvert.SerializeObject(AllNews), Timestamp = DateTime.Now });
            }
            isBusy = false;

            Console.WriteLine("Took "+( DateTime.Now - retreiveTime ).TotalSeconds + $"  Seconds , AllNews {AllNews.Count}");

            return JsonConvert.SerializeObject(AllNews);
        }
    }

    public class News
    {
        public DateTime lasttime;
        public string Symbol;
        public DateTime newsTime;
        public string news;
        public string sentiment;
        public string provider;

        public async Task<string> FillSentiment()
        {
            sentiment = await SentimentApi.CallFlaskApiAsync(news);
            return sentiment;
        }
    }

    public class CachedResult
    {
        public string JsonResult { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
