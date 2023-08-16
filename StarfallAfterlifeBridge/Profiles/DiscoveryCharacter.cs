using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class DiscoveryCharacter
    {
        [JsonPropertyName("charactname")]
        public string Name { get; set; }

        [JsonPropertyName("faction")]
        public Faction Faction { get; set; }

        [JsonPropertyName("xp_factor")]
        public int XpFactor { get; set; }

        [JsonPropertyName("bonus_xp")]
        public int BonusXp { get; set; }

        [JsonPropertyName("access_level")]
        public int AccessLevel { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("ships_list")]
        public List<object> ShipsList { get; set; } = new();

        [JsonPropertyName("ship_groups")]
        public List<object> ShipGroups { get; set; } = new();
    }
}
