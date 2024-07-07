using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace IAH_SinglePlayerAutomation.Class
{
    public class TransformedEntityVitals
    {
        public short lvl;
        public short hp;
        public short maxHp;
        public short sp;
        public short maxSp;
        public int xp;
        public int xpReq;
        public short armor;
        public short maxArmor;
        public short kills;
    }

    public class TransformedEntityCombat
    {
        public short ammo;
        public short maxAmmo;
        public float atkRange;
        public bool reloading;
        public float atkDelay;
        public string targetId;
    }
    
    public class TransformedEntityVectors
    {
        public Vector3 pos;
        public Vector3 gotoPos;
        public Vector3 fwd;
        public Vector3 right;
    }

    public class TransformedEntityStates
    {
        public bool lookAtEnabled;
        public Vector3 lookAt;
        public bool playerForcedMovement;
        public short splState;
        public bool shootAt;
        public Vector3 shootAtPos;
        public short state;
    }

    public class TransformedEntityInitials
    {
        public List<string> tags = new List<string>();

        public short scopeId;
        public short ammoId;
        public short barrelId;
        public short charmId;
        public short auraId;
        public short scarfId;
        public short hatId;
        public short bracersId;
        public short jointsId;
        public short maskId;

        public float atkArcAngle;
        public float frontArcAngle;
        public float sideArcAngle;
        public float backArcAngle;

        public List<string> equips = new List<string>();
        public List<string> skills = new List<string>();
        public List<GenericDyeData> dyes = new List<GenericDyeData>();

        public string team;
        public string teamCustom;
        public ushort type;
        public string ip;
    }

    public class GenericDyeData
    {
        public ushort dyeType;
        public ushort equipItemType;
    }
    
    public class Entity
    {
        public string id;
        public TransformedEntityInitials initData;
        public TransformedEntityVitals vitals;
        public TransformedEntityStates states;
        public TransformedEntityVectors vectors;
        public TransformedEntityCombat combat;

        public async Task RunAi()
        {
            var entities = Program.GameState.GetEntitiesByFlag("HOSTILE");

            //remove bots that have creep flag or non-combat
            entities = entities.Where(entity => !entity.initData.tags.Contains("CREEP") && !entity.initData.tags.Contains("NON-COMBAT"))
                .ToList();

            // we attack closest enemy.
            entities = entities.OrderBy(entity => Vector3.Distance(vectors.pos, entity.vectors.pos)).ToList();

            if (entities.Count > 0) // battle mode.
            {
                var distance = Vector3.Distance(vectors.pos, entities[0].vectors.pos);

                var blocked = false;// await Requests.RayCast(id, entities[0].id);

                if (distance < combat.atkRange && blocked == false)
                {
                    Requests.BotAction(id, "rotate", entities[0].vectors.pos);
                    Requests.BotAction(id, "stop", "");
                }
                else
                {
                    Requests.BotAction(id, "move", entities[0].vectors.pos);
                    Requests.BotAction(id, "rotate", entities[0].vectors.pos);
                }

                Requests.BotAction(id, "attack", entities[0].id);
            }
            else
            {
                // no enemy bots. reload weapon and spin 360.
                if (combat.ammo != combat.maxAmmo && combat.reloading == false)
                {
                    Requests.BotAction(id, "reload", "");
                    Requests.BotAction(id, "chat", "Reloading!");
                }

                Requests.BotAction(id, "rotate", vectors.pos + vectors.right);
            }

            /*
             * other actions:  cancelAttack, stop, these don't have actionValue.
             * skill: value int from 0 to 3. bot will use skill if has any.
             */
        }

        public static int HostileEntities()
        {
            var entities = Program.GameState.GetEntitiesByFlag("HOSTILE");

            //remove bots that have creep flag or non-combat
            entities = entities.Where(entity => !entity.initData.tags.Contains("CREEP") && !entity.initData.tags.Contains("NON-COMBAT"))
                .ToList();
            return entities.Count;
        }
    }
}