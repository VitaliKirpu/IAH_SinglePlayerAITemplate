using System;
using System.Collections.Generic;
using System.Linq;
using IAH_SinglePlayerAutomation.Class.Response;

namespace IAH_SinglePlayerAutomation.Class
{
    public class GameState
    {
        public List<GridNode> gridNodes = new List<GridNode>();
        public List<Tile> tiles = new List<Tile>();
        public List<WebBufferTile> webBufferTiles = new List<WebBufferTile>();
        public List<Entity> entities = new List<Entity>();

        public bool pcStarted;
        public string osSelected;
        public bool modemConnected;
        public int score;
        public int money;
        public int chaosCards;
        public int TPCards;
        public int level;
        public int relativeDificulty;
        public int actionTurn;
        public long lastPerformedActionTick;


        public Tile GetTileByType(string mainType, string type)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].type == type && tiles[i].mainType == mainType)
                {
                    return tiles[i];
                }
            }

            return null;
        }

        public List<Tile> GetTilesByType(string mainType, string type)
        {
            List<Tile> filteredTiles = tiles.Where(e => e.type == type && e.mainType == mainType).ToList();

            return filteredTiles;
        }

        public void PerformedAction()
        {
            lastPerformedActionTick = DateTime.Now.Ticks;
        }

        public bool CanPerformAction()
        {
            // actions trigger hostile response, we need to be carefull when we perform actions otherwise many enemies will spawn.
            long now = DateTime.Now.Ticks;
            long cd = TimeSpan.TicksPerSecond * 5;


            if (lastPerformedActionTick + cd > now)
            {
                return false;
            }


            return true;
        }

        public List<Entity> GetEntitiesByFlag(string team)
        {
            List<Entity> filteredEntities = entities.Where(e => e.team == team).ToList();

            return filteredEntities;
        }
        
        public List<Entity> GetEntitiesByType(string type)
        {
            List<Entity> filteredEntities = entities.Where(e => e.type == type).ToList();

            return filteredEntities;
        }
    }
}