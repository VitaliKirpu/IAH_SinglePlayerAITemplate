using System;
using System.Collections.Generic;

namespace IAH_SinglePlayerAutomation.Class
{
    public class GameState
    {
        public bool pcStarted = false;
        public string osSelected = "";
        public bool modemConnected = false;
        public int score = 0;
        public int money = 0;
        public int chaosCards;
        public int TPCards;
        public int level;
        
        public List<Tile> tiles = new List<Tile>();

        public Tile GetTileByType(string type)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].type == type)
                {
                    return tiles[i];
                }
            }

            return null;
        }
    }
}