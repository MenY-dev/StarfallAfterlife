using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class BGShopItem
    {
        [JsonPropertyName("id")]
        public int ItemId { get; set; } = -1;

        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;

        [JsonPropertyName("bgc_price")]
        public int BGC { get; set; } = 0;

        [JsonPropertyName("access_level")]
        public int AccesLevel { get; set; } = 0;

        [JsonPropertyName("faction")]
        public Faction Faction { get; set; } = 0;
    }
}
