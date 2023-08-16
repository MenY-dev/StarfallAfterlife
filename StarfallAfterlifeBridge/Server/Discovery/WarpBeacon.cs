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
    public class WarpBeacon : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.WarpBeacon;

        public int Destination { get; set; } = 0;

        [JsonIgnore]
        public override Vector2 Location { get; set; }

        public WarpBeacon(GalaxyMapPortal mapPortal, StarSystem system)
        {
            System = system;
            Destination = mapPortal.Destination;
            Hex = mapPortal.Hex;
            Id = mapPortal.Id;
        }
    }
}
