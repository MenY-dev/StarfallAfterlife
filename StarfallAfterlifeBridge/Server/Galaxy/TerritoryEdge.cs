using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class TerritoryEdge
    {
        [JsonPropertyName("neighbor")]
        public int Neighbor { get; set; }

        [JsonPropertyName("x")]
        public float X { get; set; } = 0;

        [JsonPropertyName("y")]
        public float Y { get; set; } = 0;


        [JsonIgnore]
        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
    }
}
