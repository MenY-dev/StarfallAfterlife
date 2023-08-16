using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipProgression : ICloneable
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("points")]
        public int Points { get; set; } = 0;

        object ICloneable.Clone() => Clone();

        public ShipProgression Clone()
        {
            return MemberwiseClone() as ShipProgression;
        }
    }
}