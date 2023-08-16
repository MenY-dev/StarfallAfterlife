using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryQuickTravelGate : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.QuickTravelGate;

        public DiscoveryQuickTravelGate(GalaxyMapQuickTravalGate mapGate, StarSystem system)
        {
            System = system;
            Hex = mapGate.Hex;
            Id = mapGate.Id;
        }
    }
}
