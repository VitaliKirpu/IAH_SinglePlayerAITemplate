using System;
using System.Collections.Generic;
using System.Linq;
using IAH_SinglePlayerAutomation.Class.Response;

namespace IAH_SinglePlayerAutomation.Class
{
    public class GameState
    {
        public List<GridNode> gridNodes = new List<GridNode>();
        public List<WebBufferTile> webBufferTiles = new List<WebBufferTile>();
        public List<Tile> tiles = new List<Tile>();

        public int actionTurn;
        public int chaosCards;
        public List<Entity> entities = new List<Entity>();
        public long lastPerformedActionTick;
        public int level;
        public bool modemConnected;
        public int money;
        public string osSelected;
        public bool pcStarted;
        public int relativeDificulty;
        public int score;

        public int TPCards;

        public float timeRunning;
        public int fps;
        public string version;


        public Tile GetTileByType(string mainType, string type)
        {
            for (var i = 0; i < tiles.Count; i++)
                if (tiles[i].type == type && tiles[i].mainType == mainType)
                    return tiles[i];

            return null;
        }

        public List<Tile> GetTilesByType(string mainType, string type)
        {
            var filteredTiles = tiles.Where(e => e.type == type && e.mainType == mainType).ToList();

            return filteredTiles;
        }

        public void PerformedAction()
        {
            lastPerformedActionTick = DateTime.Now.Ticks;
        }

        public bool CanPerformAction()
        {
            // actions trigger hostile response, we need to be carefull when we perform actions otherwise many enemies will spawn.
            var now = DateTime.Now.Ticks;
            var cd = TimeSpan.TicksPerSecond * 5;


            if (lastPerformedActionTick + cd > now) return false;


            return true;
        }

        public List<Entity> GetEntitiesByFlag(string team)
        {
            var filteredEntities = entities.Where(e => e.team == team).ToList();

            return filteredEntities;
        }

        public List<Entity> GetEntitiesByType(string type)
        {
            var filteredEntities = entities.Where(e => e.type == type).ToList();

            return filteredEntities;
        }
    }
}