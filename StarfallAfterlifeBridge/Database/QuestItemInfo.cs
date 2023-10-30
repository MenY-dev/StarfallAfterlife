using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct QuestItemInfo
    {
        [JsonInclude, JsonPropertyName("item")]
        public int Id;

        [JsonInclude, JsonPropertyName("type")]
        public int Type;

        [JsonInclude, JsonPropertyName("count")]
        public int Count;

        public InventoryItem ToInventoryItem() => new()
        {
            Id = Id,
            Type = (InventoryItemType)Type,
            Count = Count
        };
    }
}
