using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryAiFleet : DiscoveryFleet
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.AiFleet;

        protected override void OnSystemChanged(StarSystem system)
        {
            base.OnSystemChanged(system);
        }
    }
}
