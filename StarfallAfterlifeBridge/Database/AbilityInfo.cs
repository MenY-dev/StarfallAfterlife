using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct AbilityInfo
    {
        public int Id { get; set; }

        public AbilityLogic Logic { get; set; }

        public AbilityTargetType TargetType { get; set; }

        public float Cooldown { get; set; }
        
        public float AgroVision { get; set; }

        public float NebulaVision { get; set; }

        public float SensorRadius { get; set; }

        public IReadOnlyList<FleetEffectInfo> Effects { get; set; }
    }
}
