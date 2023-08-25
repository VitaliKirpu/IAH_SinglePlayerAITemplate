using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IAH_SinglePlayerAutomation
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://127.0.0.1:6800");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                while (true)
                {
                    var response = await httpClient.GetAsync($"/v1/playerstate");
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine(responseContent);
                    if (response.IsSuccessStatusCode)
                    {
                        TransitionResponse requestResponse = JsonConvert.DeserializeObject<TransitionResponse>(responseContent);
                        Console.WriteLine(requestResponse.state);
                    }
                    
                    await Task.Delay(1000); // run every 1 sec
                    
                }
            }
        }
    }
}