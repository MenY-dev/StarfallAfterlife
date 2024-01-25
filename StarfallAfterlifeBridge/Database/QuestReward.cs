using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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
        public List<QuestRevardItemInfo> Items;

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

        public QuestReward Combine(QuestReward reward)
        {
            var newItems = reward.Items?.ToList() ?? new();

            if (Items is List<QuestRevardItemInfo> items)
            {
                foreach (var item in items)
                {
                    var index = newItems.FindIndex(m => m.Id == item.Id);

                    if (index < 0)
                    {
                        newItems.Add(item);
                    }
                    else
                    {
                        var newItem = newItems[index];
                        newItem.Count += item.Count;
                        newItems[index] = newItem;
                    }
                }
            }

            return new QuestReward
            {
                IGC = IGC + reward.IGC,
                Xp = Xp + reward.Xp,
                HouseCurrency = HouseCurrency + reward.HouseCurrency,
                Items = newItems,
            };
        }
    }
}
