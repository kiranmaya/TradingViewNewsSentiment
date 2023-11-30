using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper
{
    public class RestTest
    {
        public static async Task<string> CallFlaskApiAsync(string news)
        {
            string apiUrl = "http://127.0.0.1:2005/sentiment";

            // Sample JSON payload
            string jsonPayload = $"{{\"text\": \"{news}\"}}";

            using( HttpClient client = new HttpClient() )
            {
                // Create an HttpRequestMessage
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Set the Content-Type header
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the request
                HttpResponseMessage response = await client.SendAsync(request);

                // Check if the request was successful (status code 200 OK)
                if( response.IsSuccessStatusCode )
                {
                    // Read and display the response content
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return result;
                }
                else
                {

                    // Print an error message
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return "Error";
                }
            }
        }
    }
}
