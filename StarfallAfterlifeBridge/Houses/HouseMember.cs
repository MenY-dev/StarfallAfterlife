using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public class HouseMember
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("player_id")]
        public Guid PlayerId { get; set; }

        [JsonPropertyName("char_id")]
        public Guid CharacterId { get; set; }

        [JsonPropertyName("currency")]
        public int Currency { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("rank_id")]
        public int Rank { get; set; }

        [JsonPropertyName("effects")]
        public List<HouseEffect> Effects { get; set; }
    }
}
