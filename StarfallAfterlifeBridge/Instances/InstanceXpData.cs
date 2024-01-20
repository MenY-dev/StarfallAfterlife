using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceXpData
    {
        [JsonPropertyName("dungeon_ship")]
        public double DungeonShip { get; set; }

        [JsonPropertyName("dungeon_boss")]
        public double DungeonBoss { get; set; }

        [JsonPropertyName("outpost")]
        public double Outpost { get; set; }

        [JsonPropertyName("station")]
        public double Station { get; set; }
    }
}
