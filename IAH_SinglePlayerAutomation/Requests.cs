using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IAH_SinglePlayerAutomation.Class.Response;
using Newtonsoft.Json;

namespace IAH_SinglePlayerAutomation.Class
{
    public static class Requests
    {
        public static string _apiPassword;
        public static string _remoteBotIp;

        public static async Task<bool> GetApiPassword(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            // API Password is used to authenticate your APILicense with the game and provide
            // you with a password that you have to protect during your gameplay session.
            // API Password gives you ownership of the bots that are associated with your RemoteUserBot.
            // When you perform Bot Functions you need always supply API Password.
            // API Password is always regenerated when your RemoteUserBot respawns. if this function is not called every 15-24 seconds game will exit AI Mode.
            // In multiplayer certain bots can crack API Passwords giving you or enemies ability to hjack bots.
            // API Key and API Password are not the same thing, never reveal your API Key.
            if (response.state == "INGAME")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {
                        "ip", "127.0.0.1"
                    }, // in campaign mode this will be always 127.0.0.1 for your RemoteUserBot even if you operate your AI from some other PC.
                    {"apiKey", "<key>"} // HTTPS://IAMHACKER.CC -> Get API Key
                });

                var postResponse = await SendPostRequestAsync("/v1/apipassword", jsonData);
                if (postResponse.isSuccessStatusCode)
                {
                    var passwordResponse =
                        JsonConvert.DeserializeObject<APIPasswordResponse>(postResponse.responseString);
                    _apiPassword = passwordResponse.apiPassword;
                    _remoteBotIp = passwordResponse.ip;

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
                {"ip", _remoteBotIp},
                {"apiPassword", _apiPassword},
                {"entityUniqueID", entityUniqueID},
                {"actionType", action},
                {"actionValue", actionValue}
            });

            var postResponse = await SendPostRequestAsync("/v1/botaction", jsonData);
            if (postResponse.isSuccessStatusCode) return true;


            return false;
        }

        public static async Task RunAiLogic()
        {
            for (var i = 0; i < Program.GameState.entities.Count; i++)
                if (Program.GameState.entities[i].initData.ip == _remoteBotIp)
                    Program.GameState.entities[i].RunAi();

            // 0.2 sec delay..
            await Task.Delay(200);
        }

        public static async Task<bool> TpScreen(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "TPSCREEN")
            {
                var getResponse = await Program._httpClient.GetAsync("/v1/tpscreen");
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                if (getResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<TpScreenResponse>(responseContent);

                    if (data.tpCards.Count > 0)
                    {
                        // we got TP CARDS to choose.... time to choose one.. lets pick first -> index 0.
                        // https://i.gyazo.com/8e375b3f859fd92626b783c364f39b7b.png

                        var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                        {
                            {"action", 0}
                        });


                        var postResponse = await SendPostRequestAsync("/v1/tpscreen", jsonData);


                        return true;
                    }

                    if (data.chaosCards.Count > 0)
                    {
                        // same thing as above but only for chaos cards

                        var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                        {
                            {"action", 0}
                        });


                        var postResponse = await SendPostRequestAsync("/v1/tpscreen", jsonData);


                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> GetGameState(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
                var getResponse = await Program._httpClient.GetAsync("/v1/gameState");
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                if (getResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<GameStateResponse>(responseContent);
                    Program.GameState.modemConnected = data.modemConnected;
                    Program.GameState.pcStarted = data.pcStarted;
                    Program.GameState.osSelected = data.osSelected;
                    Program.GameState.money = data.money;
                    Program.GameState.score = data.score;
                    Program.GameState.money = data.money;
                    Program.GameState.chaosCards = data.chaosCards;
                    Program.GameState.TPCards = data.TPCards;
                    Program.GameState.relativeDificulty = data.relativeDificulty;
                    Program.GameState.actionTurn = data.actionTurn;
                }

                return true;
            }

            return false;
        }

        public static async Task<bool> GetSystemState(TransitionResponse response)
        {
            var getResponse = await Program._httpClient.GetAsync("/v1/system");
            var responseContent = await getResponse.Content.ReadAsStringAsync();
            if (getResponse.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<SystemResponse>(responseContent);
                Program.GameState.fps = data.fps;
                Program.GameState.version = data.version;
                Program.GameState.timeRunning = data.timeRunning;
            }

            return true;
        }

        public static async Task<bool> ClickLevelUp()
        {
            // lets spawn first unit from our www block.. in windowsOS  for example first one (zero index) is Good Bot

            if (Program.GameState != null && Program.GameState.TPCards > 0)
            {
                var getResponse = await Program._httpClient.GetAsync("/v1/levelup");
                if (getResponse.IsSuccessStatusCode) return true;
            }

            return false;
        }

        public static async Task<bool> UseFramework()
        {
            // if we have LESSERHEAL framework item in our inventory we will use it on our remote bot when it is low and heal ourselves.
            if (Program.GameState != null && Program.GameState.webBufferTiles.Count == 0 &&
                Program.GameState.modemConnected)
            {
                var tiles = Program.GameState.GetTilesByType("OSTILE", "FRAMEWORK");

                //do note that this will use first remoteuserbot, it could be enemy, but for multiplayer you would want to write own code.
                var entities = Program.GameState.GetEntitiesByType(30);
                if (entities.Count > 0)
                    for (var i = 0; i < tiles.Count; i++)
                        if (tiles[i].frameworkType == "LESSERHEAL")
                        {
                            await FrameworkAction(tiles[i].uniqueID, entities[0].id);
                            return true;
                        }
            }

            return true;
        }

        public static async Task<bool> UseWWWBlock()
        {
            if (Entity.HostileEntities() > 0)
            {
                return false;
            }

            if (Program.GameState != null && Program.GameState.webBufferTiles.Count == 0 &&
                Program.GameState.modemConnected)
            {
                var tile = Program.GameState.GetTileByType("OSTILE", "WWW_BLOCK");

                if (tile == null)
                {
                    // if we dont have WWW Block / Bot Block we need generate it from the OS Tile.
                    tile = Program.GameState.GetTileByType("OSTILE", "OPERATINGSYSTEM");

                    if (tile != null && tile.isBusy == false)
                    {
                        await TileAction(tile.uniqueID, -1);

                        await Task.Delay(500);

                        Program.GameState.PerformedAction();
                        return await TileAction(tile.uniqueID, 0);
                    }

                    // if we dont have WWW Block / Bot Block we need generate it from the OS Tile.
                    tile = Program.GameState.GetTileByType("OSTILE", "OPERATINGSYSTEMRED");

                    if (tile != null && tile.isBusy == false)
                    {
                        await TileAction(tile.uniqueID, -1);

                        await Task.Delay(500);

                        Program.GameState.PerformedAction();
                        return await TileAction(tile.uniqueID, 0);
                    }
                }
                else if (tile.isBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    Program.GameState.PerformedAction();
                    // lets spawn unit from our www block.. in windowsOS  for example first one (zero index) is Good Bot
                    return await TileAction(tile.uniqueID, 0);
                }
            }

            return true;
        }

        public static async Task<bool> BrowseInternet()
        {
            // we could later expand this by checkeing that there  are no enemies. etc

            if (Entity.HostileEntities() > 0)
            {
                return false;
            }

            if (Program.GameState != null && Program.GameState.webBufferTiles.Count == 0 &&
                Program.GameState.modemConnected)
            {
                var tile = Program.GameState.GetTileByType("MAPTILE", "PLAYERPC");
                var ocupied = Program.GameState.GetTileByType("MAPTILE", "OCUPIED");

                // for this example. we only browse net once... if we have ocupied Tile somewhere dont browse net.
                if (ocupied != null) return false;

                if (tile != null && tile.isBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);
                    Program.GameState.PerformedAction();
                    return await TileAction(tile.uniqueID, 0);
                }
            }

            return true;
        }


        public static async Task<bool> InitialMenuSequence()
        {
            if (Program.GameState != null && Program.GameState.tiles.Count > 0 && Program.GameState.pcStarted == false)
            {
                var tile = Program.GameState.GetTileByType("MAPTILE", "PLAYERPC");
                if (tile != null && tile.isBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    var result = await TileAction(tile.uniqueID, 0);
                }
            }

            else if (Program.GameState != null && Program.GameState.tiles.Count > 0 &&
                     string.IsNullOrEmpty(Program.GameState.osSelected))
            {
                var tile = Program.GameState.GetTileByType("MAPTILE", "PLAYERPC");
                if (tile != null && tile.isBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    var result = await TileAction(tile.uniqueID, 0); // 0 windows, 1 linux, 2 love
                }
            }

            else if (Program.GameState != null && Program.GameState.tiles.Count > 0 &&
                     Program.GameState.modemConnected == false)
            {
                // you could make function in the Program.GameState class that check that no Tile is busy.
                var pcTile = Program.GameState.GetTileByType("MAPTILE", "PLAYERPC");
                var tile = Program.GameState.GetTileByType("ITEMTILE", "MODEM");
                if (tile != null && tile.isBusy == false && pcTile.isBusy == false)
                {
                    await TileAction(tile.uniqueID, -1);

                    await Task.Delay(500);

                    var result = await TileAction(tile.uniqueID, 0);
                }
            }

            return true;
        }

        public static async Task<bool> MainMenuTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;


            if (response.state == "MAIN_MENU_INTRO")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"transition", "MAIN_MENU"}
                });

                Console.WriteLine("Enter Main Menu...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }

        public static async Task<bool> ModeSelectionTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "MAIN_MENU")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"transition", "MODE_SELECTION"}
                });

                Console.WriteLine("Enter Mode Selection Menu...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }

        public static async Task<bool> HackerSelectionTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;


            if (response.state == "MODE_SELECTION")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    {"transition", "HACKER_SELECTION"}
                });

                Console.WriteLine("Enter Hacker Selection Menu...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }

        public static async Task<bool> HackerSelectTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "HACKER_SELECTION")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "HACKER_SELECT"},
                    {"transitionValue", 0}
                });

                Program.GameState = new GameState(); // reset internal state

                Console.WriteLine("Select Hacker...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }

        public static async Task<bool> SelectArcadeCluster(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "CHOOSE_CAMPAIGN")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "SELECT_ARCADE"}
                });

                Program.GameState = new GameState(); // reset internal state

                Console.WriteLine("Select Arcade Cluster...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }


        public static async Task<bool> StartArcade(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "ARCADE_MENU")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "START_ARCADE"},
                });

                Program.GameState = new GameState(); // reset internal state

                Console.WriteLine("Start Arcade...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }

        public static async Task<bool> GameOverTransition(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;


            if (response.state == "GAMEOVER")
            {
                var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                {
                    {"transition", "GAMEOVER"}
                });

                Console.WriteLine("Game Over...");

                var postResponse = await SendPostRequestAsync("/v1/playerstate", jsonData);
                if (postResponse.isSuccessStatusCode) response.state = ""; // consume state.

                return true;
            }

            return false;
        }

        public static async Task<bool> GetTiles(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;


            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
                var getResponse = await Program._httpClient.GetAsync("/v1/tiles");
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                if (getResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<TilesResponse>(responseContent);
                    Program.GameState.tiles = data.tiles;
                }

                return true;
            }

            return false;
        }

        public static async Task<bool> GetGrid(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;


            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
                // very expensive function -> only returns success once for every grid change. store well.
                // you need this if you want to make advanced movement logic for your bots.
                var getResponse = await Program._httpClient.GetAsync("/v1/grid");
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                if (getResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<GridResponse>(responseContent);
                    Program.GameState.gridNodes = data.gridNodes;
                    return true;
                }
            }

            return false;
        }

        public static async Task<bool> GetEntities(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;

            if (response.state == "INGAME" || response.state == "TPSCREEN")
            {
                var getResponse = await Program._httpClient.GetAsync("/v1/entities");
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                if (getResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<EntitiesResponse>(responseContent);
                    Program.GameState.entities = data.entities;
                }

                return true;
            }

            return false;
        }

        public static async Task<bool> GetBufferTiles(TransitionResponse response)
        {
            if (string.IsNullOrEmpty(response.state)) return false;


            if (response.state == "INGAME")
            {
                var getResponse = await Program._httpClient.GetAsync("/v1/buffer");
                var responseContent = await getResponse.Content.ReadAsStringAsync();
                if (getResponse.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<BufferResponse>(responseContent);
                    Program.GameState.webBufferTiles = data.tiles;

                    // we have websites in a buffer, lets try place website from the buffer on the map.
                    if (Program.GameState.webBufferTiles.Count > 0)
                    {
                        var targetTile = Program.GameState.GetTileByType("MAPTILE", "EMPTY");

                        if (targetTile == null) return false;

                        Program.GameState.PerformedAction();

                        var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
                        {
                            {"bufferUniqueID", Program.GameState.webBufferTiles[0].uniqueID},
                            // we need empty Tile, its where we want to place.
                            {"tileUniqueID", targetTile.uniqueID}
                        });

                        Console.WriteLine("Place Buffer Item on the Map...");

                        var postResponse = await SendPostRequestAsync("/v1/buffer", jsonData);
                        if (postResponse.isSuccessStatusCode)
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
                } // -1: just open or close, 0-4 Button Index -> which button to press, you can send button index without opening Tile, game will open Tile for you and click button.
            });

            var postResponse = await SendPostRequestAsync("/v1/tileaction", jsonData);
            if (postResponse.isSuccessStatusCode)
                return true;
            return false;
        }

        private static async Task<bool> FrameworkAction(string tileUniqueID, string entityUniqueID)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"uniqueID", tileUniqueID},
                {"entityUniqueID", entityUniqueID}
            });

            var postResponse = await SendPostRequestAsync("/v1/frameworkaction", jsonData);
            if (postResponse.isSuccessStatusCode)
                return true;
            return false;
        }

        private static async Task<PostResponse> SendPostRequestAsync(string endpoint, string jsonData)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await Program._httpClient.SendAsync(request).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return new PostResponse
                {responseString = responseContent, isSuccessStatusCode = response.IsSuccessStatusCode};
        }

        public static async Task<bool> RayCast(string uniqueID, string targetUniqueID)
        {
            var jsonData = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"uniqueID", uniqueID},
                {"targetUniqueID", targetUniqueID}
            });

            var postResponse = await SendPostRequestAsync("/v1/raycast", jsonData);
            if (postResponse.isSuccessStatusCode)
                return true;
            return false;
        }
    }
}