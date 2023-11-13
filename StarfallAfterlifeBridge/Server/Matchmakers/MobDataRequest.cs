using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public struct MobDataRequest
    {
        public int FleetId { get; set; }

        public int MinLvl { get; set; }

        public int MaxLvl { get; set; }

        public Faction Faction { get; set; }

        public string[] Tags { get; set; }

        public bool IsCustom { get; set; }
    }
}
