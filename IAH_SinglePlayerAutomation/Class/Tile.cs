using System.Collections.Generic;
using System.Numerics;

namespace IAH_SinglePlayerAutomation.Class
{
    public class Tile
    {
        public string equipType;

        public string frameworkType;
        public bool isBusy;
        public bool isOpen;
        public string mainType;
        public string type;
        public string websiteType;
        public Vector3 position;
        public string uniqueID;
        public string isCaptured;
        
        public  List<TileButtonState> buttonEnabled = new List<TileButtonState>();
    }
    
 
    public class TileButtonState
    {
        public int cost;
        public bool enabled;
   
    }
    
}