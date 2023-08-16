using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class InventoryItem : ICloneable
    {
        [JsonPropertyName("itemtype")]
        public InventoryItemType Type { get; set; } = InventoryItemType.Equipment;

        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("count")]
        public int Count { get; set; } = 0;

        [JsonIgnore]
        public int IGCPrice { get; set; } = -1;

        [JsonIgnore]
        public int BGCPrice { get; set; } = -1;

        public static InventoryItem Create(SfaItem item, int count = 1) => new()
        {
            Id = item.Id,
            Type = item.ItemType,
            Count = count,
            IGCPrice = item.IGC,
            BGCPrice = item.BGC,
        };

        object ICloneable.Clone() => Clone();

        public InventoryItem Clone()
        {
            return MemberwiseClone() as InventoryItem;
        }
    }
}