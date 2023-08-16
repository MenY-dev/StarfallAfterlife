using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class DiscoveryDropRule
    {
        [JsonPropertyName("type")]
        public DropRuleType Type { get; set; } = DropRuleType.Default;

        [JsonPropertyName("mobs")]
        public List<int> Mobs { get; set; } = new List<int>();

        [JsonPropertyName("items")]
        public List<DropItem> Items { get; set; } = new List<DropItem>();

        [JsonPropertyName("chance")]
        public float Chance { get; set; } = 1;

        [JsonPropertyName("personal")]
        public int DropPersonalItems { get; set; } = 0;
    }
}
