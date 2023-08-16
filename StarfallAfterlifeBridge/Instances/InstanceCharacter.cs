using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceCharacter
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("auth")]
        public string Auth { get; set; }

        [JsonPropertyName("team")]
        public int Team { get; set; } = 100;

        [JsonPropertyName("role")]
        public int Role { get; set; }

        [JsonPropertyName("faction")]
        public Faction Faction { get; set; }

        [JsonPropertyName("hex_offset_x")]
        public float HexOffsetX { get; set; }

        [JsonPropertyName("hex_offset_y")]
        public float HexOffsetY { get; set; }

        [JsonPropertyName("party_id")]
        public int PartyId { get; set; }

        [JsonPropertyName("discovery_data")]
        public string DiscoveryData { get; set; }

        [JsonPropertyName("features")]
        public InstanceCharacterFeatures Features { get; set; }
    }
}
