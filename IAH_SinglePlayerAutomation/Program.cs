using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IAH_SinglePlayerAutomation.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IAH_SinglePlayerAutomation
{
    internal class Program
    {
        private static HttpClient httpClient;

        public static async Task Main(string[] args)
        {
            using (httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://127.0.0.1:6800");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                
                Console.WriteLine("Starting Singleplayer AI Template...");
                await Task.Delay(3000); // run every 3 sec
                
                while (true)
                {
                    string currentState = "";
                    var response = await httpClient.GetAsync($"/v1/playerstate");
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        TransitionResponse requestResponse =
                            JsonConvert.DeserializeObject<TransitionResponse>(responseContent);


                        /* STARTING
                         * LOADING
                         * MAIN_MENU_INTRO
                         * MODE_SELECTION
                         * HACKER_SELECTION
                         * INGAME
                         * GAMEOVER
                         */

                        string newState = requestResponse.state;

                        if (newState != currentState && newState == "MAIN_MENU_INTRO")
                        {
                            var data = new Dictionary<string, string>
                            {
                                {"transition", "MAIN_MENU"}
                            };

                            var jsonData = JsonConvert.SerializeObject(data);
                            
                            Console.WriteLine("Enter Main Menu...");
                            
                            PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                            if (postResponse.IsSuccessStatusCode)
                            {
                                currentState = newState;
                            }
                        }
                    }

                    await Task.Delay(1000); // run every 1 sec
                }
            }
        }

        static async Task<PostResponse> SendPostRequestAsync(string endpoint, string jsonData)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return new PostResponse()
                {responseString = responseContent, IsSuccessStatusCode = response.IsSuccessStatusCode};
        }
    }
}