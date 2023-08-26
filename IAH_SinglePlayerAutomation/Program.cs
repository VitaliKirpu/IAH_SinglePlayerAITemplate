using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IAH_SinglePlayerAutomation.Class;
using IAH_SinglePlayerAutomation.Class.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IAH_SinglePlayerAutomation
{
    internal static class Program
    {
        private static HttpClient httpClient;

        /*
         * When our currentPlayerState is INGAME we create gameState class to hold tiles and entities.
         */
        private static GameState gameState;

        private static string currenPlayertState = "";

        public static async Task Main(string[] args)
        {
            using (httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://127.0.0.1:6800");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                Console.WriteLine("Starting Singleplayer AI Template...");
                await Task.Delay(3000); // wait 3 seconds before attempting call player state.

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


                        await MainMenuTransition(requestResponse);
                        await ModeSelectionTransition(requestResponse);
                        await HackerSelectionTransition(requestResponse);
                        await HackerSelectTransition(requestResponse);
                        // await GameOverTransition(requestResponse);

                        // these happen when we are in the game - state: INGAME
                        await GetTiles(requestResponse);
                        await GetGameState(requestResponse);

                        Console.Clear();
                        Console.WriteLine("Current Player State: " + requestResponse.state);
                        Console.WriteLine("Money: " + gameState.money + " | Score: " + gameState.score);


                        // boilerplate click few initial menus (start pc, select os, connect to the internet).
                        await InitialMenuSequence();
                    }

                    await Task.Delay(1000); // run every 1 sec
                }
            }
        }

        private static async Task<bool> GetGameState(TransitionResponse response)
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
                if (gameState == null)
                {
                    gameState = new GameState();
                }

                var getReponse = await httpClient.GetAsync($"/v1/gamestate");
                string responseContent = await getReponse.Content.ReadAsStringAsync();
                if (getReponse.IsSuccessStatusCode)
                {
                    GameStateResponse data = JsonConvert.DeserializeObject<GameStateResponse>(responseContent);
                    gameState.modemConnected = data.modemConnected;
                    gameState.pcStarted = data.pcStarted;
                    gameState.osSelected = data.osSelected;
                    gameState.money = data.money;
                    gameState.score = data.score;
                    gameState.money = data.money;
                    gameState.chaosCards = data.chaosCards;
                    gameState.TPCards = data.TPCards;
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> InitialMenuSequence()
        {
            if (gameState != null && gameState.tiles.Count > 0 && gameState.pcStarted == false)
            {
                Tile tile = gameState.GetTileByType("PLAYERPC");
                if (tile != null && tile.IsBusy == false)
                {
                    await UseTile(tile.uniqueID, -1);

                    await Task.Delay(500);

                    bool result = await UseTile(tile.uniqueID, 0);
                }
            }

            else if (gameState != null && gameState.tiles.Count > 0 && string.IsNullOrEmpty(gameState.osSelected))
            {
                Tile tile = gameState.GetTileByType("PLAYERPC");
                if (tile != null && tile.IsBusy == false)
                {
                    await UseTile(tile.uniqueID, -1);

                    await Task.Delay(500);

                    bool result = await UseTile(tile.uniqueID, 0); // 0 windows, 1 linux, 2 love
                }
            }

            else if (gameState != null && gameState.tiles.Count > 0 && gameState.modemConnected == false)
            {
                // you could make function in the gameState class that check that no tile is busy.
                Tile pcTile = gameState.GetTileByType("PLAYERPC");
                Tile tile = gameState.GetTileByType("MODEM");
                if (tile != null && tile.IsBusy == false && pcTile.IsBusy == false)
                {
                    await UseTile(tile.uniqueID, -1);

                    await Task.Delay(500);

                    bool result = await UseTile(tile.uniqueID, 0);
                }
            }

            return true;
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

                gameState = null; // reset internal state

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

        private static async Task<bool> GetTiles(TransitionResponse response)
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
                if (gameState == null)
                {
                    gameState = new GameState();
                }

                var getReponse = await httpClient.GetAsync($"/v1/tiles");
                string responseContent = await getReponse.Content.ReadAsStringAsync();
                if (getReponse.IsSuccessStatusCode)
                {
                    TilesResponse data = JsonConvert.DeserializeObject<TilesResponse>(responseContent);
                    gameState.tiles = data.tiles;
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> UseTile(string tileUniqueID, int action)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"uniqueID", tileUniqueID},
                {
                    "action", action
                } // -1: just open or close, 0-4 Button Index -> which button to press, you can send button index without opening tile, game will open tile for you and click button.
            });

            Console.WriteLine("UseTile...");

            PostResponse postResponse = await SendPostRequestAsync("/v1/usetile", jsonData);
            if (postResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
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