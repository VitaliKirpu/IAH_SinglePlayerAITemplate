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
        public Vector3 position;
        public string type;
        public string uniqueID;
        
        public  List<TileButtonState> buttonEnabled = new List<TileButtonState>();
    }
    
 
    public class TileButtonState
    {
        public int cost;
        public bool enabled;
   
    }
    
}