 
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

public class MCRss
{
    public static async Task Main()
    {
        string rssFeedUrl = "https://www.moneycontrol.com/rss/MCtopnews.xml";
        await ReadRssFeed(rssFeedUrl);
    }

    static async Task ReadRssFeed(string url)
    {
        try
        {
            using( HttpClient client = new HttpClient() )
            {
                string rssData = await client.GetStringAsync(url);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(rssData);

                XmlNodeList items = xmlDoc.SelectNodes("//item");

                if( items != null )
                {
                    Console.WriteLine($"Latest News from {url}:\n");

                    foreach( XmlNode item in items )
                    {
                        XmlNode titleNode = item.SelectSingleNode("title");
                        XmlNode linkNode = item.SelectSingleNode("link");
                        XmlNode dataNode = item.SelectSingleNode("pubDate");



           

                        if( titleNode != null && linkNode != null )
                        {
                            string title = titleNode.InnerText;
                            string link = linkNode.InnerText;

                            Console.WriteLine($"Title: {title}");
                            Console.WriteLine($"Link: {link}\n");
                          
                            string dateString = dataNode.InnerText;

                            if( DateTimeOffset.TryParseExact(dateString, "ddd, dd MMM yyyy HH:mm:ss zzz",
                                                            System.Globalization.CultureInfo.InvariantCulture,
                                                            System.Globalization.DateTimeStyles.None,
                                                            out DateTimeOffset result) )
                            {
                                 
                                Console.WriteLine($"Converted DateTime: {result}");
                                Console.WriteLine($"\n\n\n\n\n");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to parse the date string: {dateString}");
                            }

                        }
                    }
                }
                else
                {
                    Console.WriteLine("No items found in the RSS feed.");
                }
            }
        }
        catch( Exception ex )
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
