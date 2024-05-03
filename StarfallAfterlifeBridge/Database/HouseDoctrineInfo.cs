using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct HouseDoctrineInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("quest_ident")]
        public string QuestIdent { get; set; }

        [JsonPropertyName("quest_duration")]
        public int QuestDuration { get; set; }

        [JsonPropertyName("effect_duration")]
        public int EffectDuration { get; set; }

        [JsonPropertyName("effect")]
        public int Effect { get; set; }
    }
}
