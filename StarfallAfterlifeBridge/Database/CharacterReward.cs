using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class CharacterReward
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("charact")]
        public int Character { get; set; }

        [JsonPropertyName("reward_id")]
        public int RewardId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
