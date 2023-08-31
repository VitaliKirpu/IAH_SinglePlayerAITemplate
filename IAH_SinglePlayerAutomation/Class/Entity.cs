using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace IAH_SinglePlayerAutomation.Class
{
    public class Entity
    {
        public List<string> equips = new List<string>();
        public List<string> skills = new List<string>();
        public List<string> tags = new List<string>();

        public float attackDelay;
        public float attackRange;
        public Vector3 forward;

        public string targetUniqueID;

        public string team;
        public string teamCustom;
        public string type;
        public string uniqueID;
        public string ip;

        public int ammo;
        public int maxAmmo;

        public int maxHp;
        public int maxSp;
        public int sp;
        public int hp;
        public int xp;
        public int xpNeeded;

        public Vector3 position;

        public bool reloading;
        public Vector3 right;

        public async Task RunAi()
        {
            var entities = Program.GameState.GetEntitiesByFlag("HOSTILE");

            //remove bots that have creep flag or non-combat
            entities = entities.Where(entity => !entity.tags.Contains("CREEP") && !entity.tags.Contains("NON-COMBAT"))
                .ToList();

            // we attack closest enemy.
            entities = entities.OrderBy(entity => Vector3.Distance(position, entity.position)).ToList();

            if (entities.Count > 0) // battle mode.
            {
                var distance = Vector3.Distance(position, entities[0].position);

                var blocked = await Requests.RayCast(uniqueID, entities[0].uniqueID);

                if (distance < attackRange && blocked == false)
                {
                    Requests.BotAction(uniqueID, "rotate", entities[0].position);
                    Requests.BotAction(uniqueID, "stop", "");
                }
                else
                {
                    Requests.BotAction(uniqueID, "move", entities[0].position);
                    Requests.BotAction(uniqueID, "rotate", entities[0].position);
                }

                Requests.BotAction(uniqueID, "attack", entities[0].uniqueID);
            }
            else
            {
                // no enemy bots. reload weapon and spin 360.
                if (ammo != maxAmmo && reloading == false)
                {
                    Requests.BotAction(uniqueID, "reload", "");
                    Requests.BotAction(uniqueID, "chat", "Reloading!");
                }

                Requests.BotAction(uniqueID, "rotate", position + right);
            }

            /*
             * other actions:  cancelAttack, stop, these don't have actionValue.
             * skill: value int from 0 to 3. bot will use skill if has any.
             */
        }
    }
}