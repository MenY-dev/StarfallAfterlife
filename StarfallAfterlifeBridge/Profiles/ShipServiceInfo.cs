using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipServiceInfo : ICloneable
    {
        [JsonPropertyName("xp")]
        public int Xp { get; set; } = 0;

        [JsonPropertyName("fleet_min")]
        public int FleetMin { get; set; } = -1;

        [JsonPropertyName("fleet_max")]
        public int FleetMax { get; set; } = -1;

        [JsonPropertyName("drop_state")]
        public bool DropState { get; set; } = false;

        [JsonPropertyName("role")]
        public int Role { get; set; } = 0;

        [JsonPropertyName("bt")]
        public string BT { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("drop_tree")]
        public DropTreeNode DropTree { get; set; }

        object ICloneable.Clone() => Clone();

        public ShipServiceInfo Clone()
        {
            var clone = MemberwiseClone() as ShipServiceInfo;
            clone.Tags = Tags?.ToList();
            clone.DropTree = DropTree?.Clone();
            return clone;
        }
    }
}
