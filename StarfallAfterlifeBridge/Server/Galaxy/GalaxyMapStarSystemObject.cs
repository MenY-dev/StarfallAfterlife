using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapStarSystemObject : IGalaxyMapObject
    {
        [JsonIgnore]
        public virtual GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.None;

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("x")]
        public int X { get; set; } = 0;

        [JsonPropertyName("y")]
        public int Y { get; set; } = 0;

        [JsonIgnore]
        public SystemHex Hex
        {
            get => new SystemHex((int)X, (int)Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
    }
}
