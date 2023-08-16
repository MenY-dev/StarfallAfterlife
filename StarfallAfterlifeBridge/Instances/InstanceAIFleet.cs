using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceAIFleet
    {
        [JsonPropertyName("ai_id")]
        public int Id { get; set; }

        [JsonPropertyName("ai_type")]
        public int Type { get; set; }

        [JsonPropertyName("ai_faction")]
        public Faction Faction { get; set; }

        [JsonPropertyName("ai_level")]
        public int Level { get; set; }

        [JsonPropertyName("hex_offset_x")]
        public float HexOffsetX { get; set; }

        [JsonPropertyName("hex_offset_y")]
        public float HexOffsetY { get; set; }

        [JsonPropertyName("ai_fleet_xp")]
        public int FleetXp { get; set; }

        [JsonPropertyName("ai_fgroup")]
        public int FactionGroup { get; set; }

        [JsonPropertyName("mob")]
        public InstanceMob Mob { get; set; }

        [JsonPropertyName("fleet_cargo")]
        public List<CargoItem> Cargo { get; set; }

    }
}
