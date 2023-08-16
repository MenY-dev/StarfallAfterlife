using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class CargoItem
    {
        [JsonPropertyName("entity")]
        public int Entity { get; set; } = 0;

        [JsonPropertyName("type")]
        public InventoryItemType Type { get; set; } = InventoryItemType.Equipment;

        [JsonPropertyName("count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("unique_data")]
        public string UniqueData { get; set; }
    }
}
