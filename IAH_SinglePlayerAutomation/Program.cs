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
    internal static class Program
    {
        private static HttpClient httpClient;

        private static string currenPlayertState = "";

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
                    var response = await httpClient.GetAsync($"/v1/playerstate");
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        TransitionResponse requestResponse =
                            JsonConvert.DeserializeObject<TransitionResponse>(responseContent);


                        /* STARTING
                         * LOADING
                         * MAIN_MENU_INTRO
                         * MAIN_MENU
                         * MODE_SELECTION
                         * HACKER_SELECTION
                         * HACKER_SELECT
                         * SANDBOX
                         * INGAME
                         * GAMEOVER
                         */

                        Console.WriteLine("Current Player State: " + requestResponse.state);

                        await MainMenuTransition(requestResponse);
                        await ModeSelectionTransition(requestResponse);
                        await HackerSelectionTransition(requestResponse);
                        await HackerSelectTransition(requestResponse);
                        await GameOverTransition(requestResponse);
                    }

                    await Task.Delay(1000); // run every 1 sec
                }
            }
        }

        private static async Task<bool> MainMenuTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == currenPlayertState)
            {
                return false;
            }

            if (response.state == "MAIN_MENU_INTRO")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"transition", "MAIN_MENU"}
                });

                Console.WriteLine("Enter Main Menu...");

                PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
                    currenPlayertState = response.state;
                    response.state = ""; // consume state.
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> ModeSelectionTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == currenPlayertState)
            {
                return false;
            }

            if (response.state == "MAIN_MENU")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"transition", "MODE_SELECTION"}
                });

                Console.WriteLine("Enter Mode Selection Menu...");

                PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
                    currenPlayertState = response.state;
                    response.state = ""; // consume state.
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> HackerSelectionTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == currenPlayertState)
            {
                return false;
            }

            if (response.state == "MODE_SELECTION")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"transition", "HACKER_SELECTION"}
                });

                Console.WriteLine("Enter Hacker Selection Menu...");

                PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
                    currenPlayertState = response.state;
                    response.state = ""; // consume state.
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> HackerSelectTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == currenPlayertState)
            {
                return false;
            }

            if (response.state == "HACKER_SELECTION")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "HACKER_SELECT"},
                    {"transitionValue", 0}
                });

                Console.WriteLine("Select Hacker...");

                PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
                    currenPlayertState = response.state;
                    response.state = ""; // consume state.
                }

                return true;
            }

            return false;
        }
        
        private static async Task<bool> GameOverTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == currenPlayertState)
            {
                return false;
            }

            if (response.state == "INGAME")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "GAMEOVER"}
                });

                Console.WriteLine("Game Over...");

                PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
                    currenPlayertState = response.state;
                    response.state = ""; // consume state.
                }

                return true;
            }

            return false;
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