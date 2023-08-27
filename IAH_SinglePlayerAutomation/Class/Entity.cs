using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace IAH_SinglePlayerAutomation.Class
{
    public class Entity
    {
        public string uniqueID;
        public Vector3 position;
        public Vector3 forward;
        public Vector3 right;
        public string team;
        public string teamCustom;
        public string ip;
        public string type;

        public int hp;
        public int maxHp;
        public int sp;
        public int maxSp;
        public int xp;
        public int xpNeeded;

        public List<string> tags = new List<string>();
        public List<string> equips = new List<string>();

        public int ammo;
        public int maxAmmo;

        public bool reloading;
        public float attackdelay;

        public float attackRange;

        public string targetUniqueID;

        public  async Task RunAI()
        {
            List<Entity> entities = Program.gameState.GetEntitiesByFlag("HOSTILE");

            //remove bots that have creep flag or non-combat
            entities = entities.Where(entity => !entity.tags.Contains("CREEP") && !entity.tags.Contains("NON-COMBAT")).ToList();

            // we attack closest enemy.
            entities = entities.OrderBy(entity => Vector3.Distance(position, entity.position)).ToList();


            if (entities.Count > 0) // battle mode.
            {
                float distance = Vector3.Distance(position, entities[0].position);
                
                bool blocked = await Program.Raycast(uniqueID, entities[0].uniqueID);
                
                if (distance < attackRange && blocked == false)
                {
                    Program.BotAction(uniqueID, "rotate", entities[0].position);
                    Program.BotAction(uniqueID, "stop", "");
                }
                else
                {
                    Program.BotAction(uniqueID, "move", entities[0].position);
                    Program.BotAction(uniqueID, "rotate", entities[0].position);
                }

                Program.BotAction(uniqueID, "attack", entities[0].uniqueID);
            }
            else
            {
                // no enemy bots. reload weapon and spin 360.
                if (ammo != maxAmmo && reloading == false)
                {
                    Program.BotAction(uniqueID, "reload", "");
                }
                Program.BotAction(uniqueID, "rotate", position + right);
            }


            /*
             * few other actions: reload, cancel_attack , stop these don't have actionValue.
             */
        }
    }
}