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
         * This is rough example, you should create your own AI, you can do it in C#, JS,etc, use some DX or GDI Library to visualize output.
         * Visit HTTPS://IAMHACKER.CC for more information or for support, we have discord server there, also don't forget to wishlist game on Steam.
         * https://store.steampowered.com/app/304770/IAH_INTERNET_WAR/
         */
        public static GameState gameState;
        private static string apiPassword;
        private static string remoteBotIP;

        public static async Task Main(string[] args)
        {
            using (httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("http://127.0.0.1:6800");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                Console.WriteLine("Starting Singleplayer AI Template...");
                await Task.Delay(2000); // wait 2 seconds before attempting call player state.

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
                         * TPSCREEN
                         * GAMEOVER
                         */

                        if (gameState == null)
                        {
                            gameState = new GameState();
                        }

                        await MainMenuTransition(requestResponse);
                        await ModeSelectionTransition(requestResponse);
                        await HackerSelectionTransition(requestResponse);
                        await HackerSelectTransition(requestResponse);
                        await GameOverTransition(requestResponse);

                        // these happen when we are in the game - state: INGAME
                        await GetTiles(requestResponse);
                        await GetGrid(requestResponse);
                        await GetEntities(requestResponse);
                        await GetGameState(requestResponse);
                        await GetBufferTiles(requestResponse);

                        await GetAPIPassword(requestResponse);

                        // these happen when we are in the tpscreen or selecting chaos cards.
                        await TpScreen(requestResponse);

                        RenderConsole(requestResponse);

                        // boilerplate click few initial menus (start pc, select os, connect to the internet).
                        await InitialMenuSequence();

                        // gameplay centric logic.
                        if (gameState.CanPerformAction())
                        {
                            await BrowseInternet();
                            await UseWWWBlock();
                            await ClickLevelUp();
                        }

                        await UseFramework();

                        await RunAILogic();


                        /* few other endpoints
                         * /v1/cpubusy -> true or false -> tells if cpu is busy, this one is good if you want to know if you are performing some actions right now
                         * /v1/time -> returns unity Time.time -> you can use this Entity-attackdelay to check when shoot delay is over, also do note, in competitive Time.time speed will change depending on tactical mode speed.
                         */
                    }

                    await Task.Delay(1000); // run every 1 sec
                }
            }
        }

        private static async Task<bool> GetAPIPassword(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            // API PAssword is used to authenticate your APILicense with the game and provide
            // you with a password that you have to protect during your gameplay session.
            // API Password gives you ownership of the bots that are associated with your RemoteUserBot.
            // When you perform Bot Functions you need always supply API Password.
            // API Password is always regenerated when your RemoteUserBot respawns. if this function is not called every 15-24 seconds game will exit AI Mode.
            // In multiplayer certain bots can crack API Passwords giving you or enemies ability to hjack bots.
            if (response.state == "INGAME")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"ip", "127.0.0.1"}, // in campaign mode this will be always 127.0.0.1 for your RemoteUserBot
                    {"apiKey", "apiKey"} // HTTPS://IAMHACKER.CC -> Get API Key
                });

                PostResponse postResponse = await SendPostRequestAsync("/v1/apipassword", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
                    APIPasswordResponse passwordResponse =
                        JsonConvert.DeserializeObject<APIPasswordResponse>(postResponse.responseString);
                    apiPassword = passwordResponse.apiPassword;
                    remoteBotIP = passwordResponse.ip;

                    Console.WriteLine("Buffer Item Placed on The Map...");
                    await Task.Delay(2000);
                }

                return true;
            }

            return false;
        }

        public static async Task<bool> BotAction(string entityUniqueID, string action, object actionValue)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"ip", remoteBotIP},
                {"apiPassword", apiPassword},
                {"entityUniqueID", entityUniqueID},
                {"actionType", action},
                {"actionValue", actionValue}
            });

            PostResponse postResponse = await SendPostRequestAsync("/v1/botaction", jsonData);
            if (postResponse.IsSuccessStatusCode)
            {
                return true;
            }


            return false;
        }

        private static async Task RunAILogic()
        {
            for (int i = 0; i < gameState.entities.Count; i++)
            {
                if (gameState.entities[i].ip == remoteBotIP)
                {
                    gameState.entities[i].RunAI();
                }
            }

            // 0.2 sec delay..
            await Task.Delay(200);
        }

        private static void RenderConsole(TransitionResponse currentPlayerState)
        {
            Console.Clear();
            Console.WriteLine("Player State:" + currentPlayerState.state);

            if (gameState != null)
            {
                Console.WriteLine("Money: " + gameState.money
                                            + " | Score: " + gameState.score
                                            + " | Relative Dificulty: " + gameState.relativeDificulty
                                            + " | Action Turn: " + gameState.actionTurn
                                            + " | Entities: " + gameState.entities.Count);

                Console.WriteLine("Tiles: " + gameState.tiles.Count
                                            + " | Grid Nodes: " + gameState.gridNodes.Count
                                            + " | Web Buffer Tiles: " + gameState.webBufferTiles.Count);

                Console.WriteLine("API Password: " + apiPassword + " | RemoteBot IP: " + remoteBotIP);
            }
        }

        private static async Task<bool> TpScreen(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == "TPSCREEN")
            {
                var getReponse = await httpClient.GetAsync($"/v1/tpscreen");
                string responseContent = await getReponse.Content.ReadAsStringAsync();
                if (getReponse.IsSuccessStatusCode)
                {
                    TpScreenResponse data = JsonConvert.DeserializeObject<TpScreenResponse>(responseContent);

                    if (data.TpCards.Count > 0)
                    {
                        // we got TP CARDS to choose.... time to choose one.. lets pick first -> index 0.
                        // https://i.gyazo.com/8e375b3f859fd92626b783c364f39b7b.png

                        var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                        {
                            {"action", 0}
                        });


                        PostResponse postResponse = await SendPostRequestAsync("/v1/tpscreen", jsonData);


                        return true;
                    }
                    else if (data.ChaosCards.Count > 0)
                    {
                        // same thing as above but only for chaos cards

                        var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                        {
                            {"action", 0}
                        });


                        PostResponse postResponse = await SendPostRequestAsync("/v1/tpscreen", jsonData);


                        return true;
                    }
                }
            }

            return false;
        }

        private static async Task<bool> GetGameState(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
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
                    gameState.relativeDificulty = data.relativeDificulty;
                    gameState.actionTurn = data.actionTurn;
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> ClickLevelUp()
        {
            // lets spawn first unit from our www block.. in windowsOS  for example first one (zero index) is Good Bot

            if (gameState != null && gameState.TPCards > 0)
            {
                var getReponse = await httpClient.GetAsync($"/v1/levelup");
                if (getReponse.IsSuccessStatusCode)
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task<bool> UseFramework()
        {
            // if we have LESSERHEAL framework item in our inventory we will use it on our remote bot when it is low and heal ourselves.
            if (gameState != null && gameState.webBufferTiles.Count == 0 && gameState.modemConnected)
            {
                List<Tile> tiles = gameState.GetTilesByType("OSTILE", "FRAMEWORK");

                //do note that this will use first remoteuserbot, it could be enemy, but for multiplayer you would want to write own code.
                List<Entity> entities = gameState.GetEntitiesByType("REMOTEUSERBOT");
                if (entities.Count > 0)
                {
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        if (tiles[i].FrameworkType == "LESSERHEAL")
                        {
                            await FrameworkAction(tiles[i].uniqueID, entities[0].uniqueID);
                            return true;
                        }
                    }
                }
            }

            return true;
        }

        private static async Task<bool> UseWWWBlock()
        {
            // lets spawn first unit from our www block.. in windowsOS  for example first one (zero index) is Good Bot
            if (gameState != null && gameState.webBufferTiles.Count == 0 && gameState.modemConnected)
            {
                Tile tile = gameState.GetTileByType("OSTILE", "WWW_BLOCK");

                // we dont have, nwm...
                if (tile == null)
                {
                    return false;
                }

                if (tile.IsBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    gameState.PerformedAction();
                    return await TileAction(tile.uniqueID, 0);
                }
            }

            return true;
        }

        private static async Task<bool> BrowseInternet()
        {
            // we could later expand this by checkeing that there  are no enemies. etc

            if (gameState != null && gameState.webBufferTiles.Count == 0 && gameState.modemConnected)
            {
                Tile tile = gameState.GetTileByType("MAPTILE", "PLAYERPC");
                Tile ocupied = gameState.GetTileByType("MAPTILE", "OCUPIED");

                // for this example. we only browse net once... if we have ocupied tile somewhere dont browse net.
                if (ocupied != null)
                {
                    return false;
                }

                if (tile != null && tile.IsBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);
                    gameState.PerformedAction();
                    return await TileAction(tile.uniqueID, 0);
                }
            }

            return true;
        }


        private static async Task<bool> InitialMenuSequence()
        {
            if (gameState != null && gameState.tiles.Count > 0 && gameState.pcStarted == false)
            {
                Tile tile = gameState.GetTileByType("MAPTILE", "PLAYERPC");
                if (tile != null && tile.IsBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    bool result = await TileAction(tile.uniqueID, 0);
                }
            }

            else if (gameState != null && gameState.tiles.Count > 0 && string.IsNullOrEmpty(gameState.osSelected))
            {
                Tile tile = gameState.GetTileByType("MAPTILE", "PLAYERPC");
                if (tile != null && tile.IsBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    bool result = await TileAction(tile.uniqueID, 0); // 0 windows, 1 linux, 2 love
                }
            }

            else if (gameState != null && gameState.tiles.Count > 0 && gameState.modemConnected == false)
            {
                // you could make function in the gameState class that check that no tile is busy.
                Tile pcTile = gameState.GetTileByType("MAPTILE", "PLAYERPC");
                Tile tile = gameState.GetTileByType("ITEMTILE", "MODEM");
                if (tile != null && tile.IsBusy == false && pcTile.IsBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    bool result = await TileAction(tile.uniqueID, 0);
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


            if (response.state == "GAMEOVER")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "GAMEOVER"}
                });

                Console.WriteLine("Game Over...");

                PostResponse postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.IsSuccessStatusCode)
                {
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


            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
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

        private static async Task<bool> GetGrid(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }


            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
                // very expensive function -> only returns success once for every grid change. store well.
                // you need this if you want to make advanced movement logic for your bots.
                var getReponse = await httpClient.GetAsync($"/v1/grid");
                string responseContent = await getReponse.Content.ReadAsStringAsync();
                if (getReponse.IsSuccessStatusCode)
                {
                    GridResponse data = JsonConvert.DeserializeObject<GridResponse>(responseContent);
                    gameState.gridNodes = data.gridNodes;
                    return true;
                }
            }

            return false;
        }

        private static async Task<bool> GetEntities(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }

            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
                var getReponse = await httpClient.GetAsync($"/v1/entities");
                string responseContent = await getReponse.Content.ReadAsStringAsync();
                if (getReponse.IsSuccessStatusCode)
                {
                    EntitiesResponse data = JsonConvert.DeserializeObject<EntitiesResponse>(responseContent);
                    gameState.entities = data.entities;
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> GetBufferTiles(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state))
            {
                return false;
            }


            if (response.state == "INGAME")
            {
                var getReponse = await httpClient.GetAsync($"/v1/buffer");
                string responseContent = await getReponse.Content.ReadAsStringAsync();
                if (getReponse.IsSuccessStatusCode)
                {
                    BufferResponse data = JsonConvert.DeserializeObject<BufferResponse>(responseContent);
                    gameState.webBufferTiles = data.tiles;

                    // we have websites in a buffer, lets try place website from the buffer on the map.
                    if (gameState.webBufferTiles.Count > 0)
                    {
                        Tile targetTile = gameState.GetTileByType("MAPTILE", "EMPTY");

                        if (targetTile == null)
                        {
                            return false;
                        }

                        gameState.PerformedAction();

                        var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                        {
                            {"bufferUniqueID", gameState.webBufferTiles[0].uniqueID},
                            // we need empty tile, its where we want to place.
                            {"tileUniqueID", targetTile.uniqueID}
                        });

                        Console.WriteLine("Place Buffer Item on the Map...");

                        PostResponse postResponse = await SendPostRequestAsync("/v1/buffer", jsonData);
                        if (postResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Buffer Item Placed on The Map...");
                            await Task.Delay(2000);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private static async Task<bool> TileAction(string tileUniqueID, int action)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"uniqueID", tileUniqueID},
                {
                    "action", action
                } // -1: just open or close, 0-4 Button Index -> which button to press, you can send button index without opening tile, game will open tile for you and click button.
            });

            PostResponse postResponse = await SendPostRequestAsync("/v1/tileaction", jsonData);
            if (postResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static async Task<bool> FrameworkAction(string tileUniqueID, string entityUniqueID)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"uniqueID", tileUniqueID},
                {"entityUniqueID", entityUniqueID}
            });

            PostResponse postResponse = await SendPostRequestAsync("/v1/frameworkaction", jsonData);
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

        public static async Task<bool> Raycast(string uniqueID, string targetUniqueID)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"uniqueID", uniqueID},
                {"targetUniqueID", targetUniqueID}
            });

            PostResponse postResponse = await SendPostRequestAsync("/v1/raycast", jsonData);
            if (postResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}