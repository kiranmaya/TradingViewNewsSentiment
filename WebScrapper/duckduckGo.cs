using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper
{
    public class duckduckGo
    {
        public static async Task Search(string query)
        {
        

            using( HttpClient client = new HttpClient() )
            {
                string apiUrl = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(query)}&format=json";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                Console.WriteLine($"{response.StatusCode} - {response.Content.ReadAsStream().ToString()}");
                if( response.IsSuccessStatusCode )
                {
                    string result = await response.Content.ReadAsStringAsync();
                    DuckDuckGoResponse duckDuckGoResponse = JsonConvert.DeserializeObject<DuckDuckGoResponse>(result);

                    Console.WriteLine("Heading: " + duckDuckGoResponse.Heading);
                    Console.WriteLine("Abstract: " + duckDuckGoResponse.AbstractText);
                    Console.WriteLine("URL: " + duckDuckGoResponse.AbstractURL);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }
    }

    // Define a class to represent the structure of the DuckDuckGo API response
    public class DuckDuckGoResponse
    {
        public string Heading { get; set; }
        public string AbstractText { get; set; }
        public string AbstractURL { get; set; }
    }
}
