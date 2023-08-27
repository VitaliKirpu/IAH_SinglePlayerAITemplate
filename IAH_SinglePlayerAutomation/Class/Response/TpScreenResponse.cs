using System.Collections.Generic;

namespace IAH_SinglePlayerAutomation.Class.Response
{

    public class TpCard
    {
        public string type;
    }
    
    public class TpScreenResponse
    {
        public List<TpCard> TpCards = new List<TpCard>();
        public List<TpCard> ChaosCards = new List<TpCard>();
    }
}