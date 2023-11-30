using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleApi;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Search.Common;
using GoogleApi.Entities.Search.Common.Enums;
using GoogleApi.Entities.Search.Video.Common;
using GoogleApi.Entities.Search.Web.Request;
using GoogleApi.Exceptions;
using Newtonsoft.Json;

namespace WebScrapper
{
    public class GoogleSearchApi
    {

        public async Task SearchAsync(string query)
        {
       

           
            using( HttpClient client = new HttpClient() )
            {
                string  apiUrl = $"https://www.googleapis.com/customsearch/v1?key=AIzaSyB8dvt-KEDokb3Hb_aoXmVEvITA2Txx0gs&cx=017576662512468239146:omuauf_lfve&q={Uri.EscapeDataString(query)}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if( response.IsSuccessStatusCode )
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
               
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }

            

        }
    }
}
