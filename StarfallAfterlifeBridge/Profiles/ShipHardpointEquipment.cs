using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipHardpointEquipment : ICloneable
    {
        [JsonPropertyName("eq")]
        public int Equipment { get; set; } = 0;

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("x")]
        public int X { get; set; } = 0;

        [JsonPropertyName("y")]
        public int Y { get; set; } = 0;

        [JsonPropertyName("destroyed")]
        public int IsDestroyed { get; set; } = 0;

        object ICloneable.Clone() => Clone();

        public ShipHardpointEquipment Clone()
        {
            return MemberwiseClone() as ShipHardpointEquipment;
        }
    }
}