using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct QuestReward : ICloneable
    {
        [JsonInclude, JsonPropertyName("igc")]
        public int IGC;

        [JsonInclude, JsonPropertyName("xp")]
        public int Xp;

        [JsonInclude, JsonPropertyName("house_currency")]
        public int HouseCurrency;

        [JsonInclude, JsonPropertyName("items")]
        public List<QuestItemInfo> Items;

        public QuestReward(JsonNode doc)
        {
            if (doc is null)
                return;

            IGC = (int?)doc["igc"] ?? 0;
            Xp = (int?)doc["xp"] ?? 0;
            HouseCurrency = (int?)doc["house_currency"] ?? 0;
            Items = new();

            foreach (var item in doc["items"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
            {
                var id = (int?)item["item"] ?? 0;
                var count = (int?)item["count"] ?? 0;

                if (id < 1 || count < 1)
                    continue;

                Items.Add(new() { Id = id, Count = count });
            }
        }

        object ICloneable.Clone() => Clone();

        public QuestReward Clone()
        {
            var c = this;
            c.Items = Items?.ToList() ?? new();
            return c;
        }
    }
}
