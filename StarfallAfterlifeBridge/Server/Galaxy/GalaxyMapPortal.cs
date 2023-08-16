using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapPortal : GalaxyMapStarSystemObject
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.WarpBeacon;

        [JsonPropertyName("type")]
        public int Type { get; set; } = 0;

        [JsonPropertyName("dest")]
        public int Destination { get; set; } = 0;

        public GalaxyMapPortal()
        {

        }

        public GalaxyMapPortal(int id, int destination, Vector2 location)
        {
            Id = id;
            Destination = destination;
            Hex = SystemHexMap.SystemPointToHex(location);
        }

        public GalaxyMapPortal(int id, int destination, SystemHex location)
        {
            Id = id;
            Destination = destination;
            Hex = location;
        }
    }
}
