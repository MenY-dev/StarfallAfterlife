using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public class MobKillInfo
    {
        public int SystemId { get; set; }

        public int MobId { get; set; }
        
        public int FleetId { get; set; }

        public int ObjectId { get; set; }

        public DiscoveryObjectType ObjectType { get; set; }

        public Faction Faction { get; set; } = Faction.None;

        public int FactionGroup { get; set; }

        public int Level { get; set; }

        public int ShipClass { get; set; }

        public int RepEarned { get; set; }

        public int IsInAttackEvent { get; set; }

        public List<string> Tags { get; } = new();
    }
}
