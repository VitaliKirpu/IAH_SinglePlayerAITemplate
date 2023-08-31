using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IAH_SinglePlayerAutomation.Class;
using IAH_SinglePlayerAutomation.Class.Response;
using Newtonsoft.Json;

namespace IAH_SinglePlayerAutomation
{
    internal static class Program
    {
        public static HttpClient _httpClient;

        /*
         * When our currentPlayerState is INGAME we create gameState class to hold tiles and entities.
         * This is rough example designed to be refactored or remade, you should create your own AI, you can do it in C#, JS,etc, use some DX or GDI Library to visualize output.
         * Visit HTTPS://IAMHACKER.CC for more information or for support, we have discord server there,
         * also don't forget to wishlist game on Steam: https://store.steampowered.com/app/304770/IAH_INTERNET_WAR/
         */

        public static GameState GameState;

        public static async Task Main(string[] args)
        {
            using (_httpClient = new HttpClient())
            {
                // you can specify different port in the game launch parameters (default is 6800) -> for example: -apiPort 6900
                _httpClient.BaseAddress = new Uri("http://127.0.0.1:6800");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine("Starting Singleplayer AI Template...");
                await Task.Delay(2000); // wait 2 seconds before attempting call player state.

                while (true)
                {
                    var response = await _httpClient.GetAsync("/v1/playerstate");
                    var responseContent = await response.Content.ReadAsStringAsync();

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

                        if (GameState == null) GameState = new GameState();

                        // STEP 1: Do some Transitions in the Main Menu
                        await Requests.MainMenuTransition(requestResponse);
                        await Requests.ModeSelectionTransition(requestResponse);
                        await Requests.HackerSelectionTransition(requestResponse);
                        await Requests.HackerSelectTransition(requestResponse);

                        // STEP2: These happen INGAME, we get some data. 
                        await Requests.GetTiles(requestResponse);
                        await Requests.GetGrid(requestResponse);
                        await Requests.GetEntities(requestResponse);
                        await Requests.GetGameState(requestResponse);
                        await Requests.GetSystemState(requestResponse);
                        await Requests.GetBufferTiles(requestResponse);

                        // STEP3: Get API Password that we need in order to perform bot AI actions.
                        await Requests.GetApiPassword(requestResponse);

                        // STEP4: When we Get Level Ups, select TP Perks or Chaos Cards.
                        await Requests.TpScreen(requestResponse);

                        // Lets Render Console for the time being....
                        RenderConsole(requestResponse);

                        // STEP5: Now when we are in the game we need to click game menus, like starting PC, selecting OS and connecting to the Internet.
                        await Requests.InitialMenuSequence();

                        // STEP6: Browse Internet, Create your Bots, and trigger Level Up Screen (TpScreen).
                        if (GameState.CanPerformAction())
                        {
                            await Requests.BrowseInternet();
                            await Requests.UseWWWBlock();
                            await Requests.ClickLevelUp();
                        }

                        //STEP7: Use Framework to improve your bots.
                        await Requests.UseFramework();

                        //STEP8: Run AI Logic.
                        await Requests.RunAiLogic();

                        //STEP9: Check if we lost, and then go back to the main menu.
                        await Requests.GameOverTransition(requestResponse);

                        /* few other endpoints
                         * /v1/cpubusy -> true or false -> tells if cpu is busy, this one is good if you want to know if you are performing some actions right now
                         * /v1/time -> returns unity Time.time -> you can use this Entity-attackdelay to check when shoot delay is over, also do note, in competitive Time.time speed will change depending on tactical mode speed.
                         */
                    }

                    await Task.Delay(1000); // run every 1 sec
                }
            }
        }

        private static void RenderConsole(TransitionResponse currentPlayerState)
        {
            Console.Clear();
            Console.WriteLine("Player State:" + currentPlayerState.state);

            if (Program.GameState != null)
            {
                Console.WriteLine("Money: " + Program.GameState.money
                                            + " | Score: " + Program.GameState.score
                                            + " | Relative Dificulty: " + Program.GameState.relativeDificulty
                                            + " | Action Turn: " + Program.GameState.actionTurn
                                            + " | Entities: " + Program.GameState.entities.Count);

                Console.WriteLine("Tiles: " + Program.GameState.tiles.Count
                                            + " | Grid Nodes: " + Program.GameState.gridNodes.Count
                                            + " | Web Buffer Tiles: " + Program.GameState.webBufferTiles.Count);

                Console.WriteLine(
                    "API Password: " + Requests._apiPassword + " | RemoteBot IP: " + Requests._remoteBotIp);

                Console.WriteLine("System FPS: " + Program.GameState.fps + " | Version: " + Program.GameState.version);
            }
        }
    }
}