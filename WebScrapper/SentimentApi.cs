using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace WebScrapper
{



    public class SentimentApi
    {
        public static async Task<string> CallFlaskApiAsync(string news)
        {
            string apiUrl = "http://localhost:2005/sentiment";
            news = Uri.EscapeDataString(news);

            // Sample JSON payload
            string jsonPayload = $"{{\"text\": \"{news}\"}}";
         

            Console.WriteLine($"CallFlaskApiAsync Sentiment  EscapeDataString   {jsonPayload}  ");

            try
            {


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
                        Console.WriteLine($"CallFlaskApiAsync result   {result}  ");
 
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
            catch( Exception e )
            {
                // Print an error message
                System.Diagnostics.Debug.WriteLine($"Exception Error: {e.Message}  ");
               
                Console.WriteLine($" Exception Error: {e.Message} ");
                return "Error";

            }

        }
    }
}
