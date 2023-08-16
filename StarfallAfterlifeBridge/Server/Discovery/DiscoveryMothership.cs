using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryMothership : DockableObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.Mothership;

        public int Circle { get; set; }

        public List<int> SecretLocations { get; set; } = new List<int>() { 0 };

        [JsonIgnore]
        public override Vector2 Location { get; set; }

        public DiscoveryMothership(GalaxyMapMothership info, StarSystem system)
        {
            Database = system?.Galaxy.Database;
            System = system;

            Id = info.Id;
            Faction = info.Faction;
            Hex = info.Hex;
            Circle = system?.Info.Level ?? -1;
        }
    }
}
