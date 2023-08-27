using System.Collections.Generic;
using System.Numerics;

namespace IAH_SinglePlayerAutomation.Class.Response
{
    public class BufferResponse
    {
        public List<WebBufferTile> tiles = new List<WebBufferTile>();
    }
    public class WebBufferTile
    {
        public string uniqueID;
        public Vector3 position;
    }

}