using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct HouseUpgradeInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("levels")]
        public List<HouseUpgradeLevelInfo> Levels { get; set; }

        public HouseUpgradeLevelInfo? GetLevel(int level)
        {
            var levels = Levels;

            if (levels is not null)
            {
                foreach (var item in levels)
                    if (item.Level == level)
                        return item;
            }

            return null;
        }
    }
}
