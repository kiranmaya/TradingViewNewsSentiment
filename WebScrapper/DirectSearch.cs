using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace WebScrapper
{
    public class DirectSearch
    {

        public static async Task SearchAsync(string query)
        {
            using( HttpClient client = new HttpClient() )
            {
                string apiUrl = $"https://www.indiatoday.in/search/{Uri.EscapeDataString(query)}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if( response.IsSuccessStatusCode )
                {
                    string result = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"{result}");
                    dynamic jsonResult = JsonConvert.DeserializeObject(result);

                    Console.WriteLine("Total Results: " + jsonResult.searchInformation.totalResults);

                    foreach( var item in jsonResult.items )
                    {
                        Console.WriteLine(item.title);
                        Console.WriteLine(item.link);
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

    }
}
