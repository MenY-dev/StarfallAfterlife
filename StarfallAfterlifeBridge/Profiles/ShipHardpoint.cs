using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipHardpoint : ICloneable
    {
        [JsonPropertyName("hp")]
        public string Hardpoint { get; set; } = string.Empty;

        [JsonPropertyName("eqlist")]
        public List<ShipHardpointEquipment> EquipmentList { get; set; } = new List<ShipHardpointEquipment>();

        object ICloneable.Clone() => Clone();

        public ShipHardpoint Clone()
        {
            var clone = MemberwiseClone() as ShipHardpoint;
            clone.EquipmentList = EquipmentList?.Select(i => i?.Clone())?.ToList();
            return clone;
        }
    }
}