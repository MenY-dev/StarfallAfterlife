using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct FleetEffectInfo
    {
        public GameplayEffectType Logic { get; set; }
        public float Duration { get; set; }
        public float EngineBoost { get; set; }
        public int Vision { get; set; }
        public int NebulaVision { get; set; }
        public int FleetId { get; set; }
    }
}
