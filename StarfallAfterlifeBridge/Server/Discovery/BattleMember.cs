using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class BattleMember
    {
        public DiscoveryFleet Fleet { get; set; }

        public BattleRole Role { get; set; }

        public Vector2 HexOffset { get; set; }

        public bool IsDestroyed { get; set; }

        public BattleMember(DiscoveryFleet fleet, BattleRole role, Vector2 hexOffset)
        {
            Fleet = fleet;
            Role = role;
            HexOffset = hexOffset;
        }
    }
}
