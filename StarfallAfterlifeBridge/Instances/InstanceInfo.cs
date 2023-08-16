using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceInfo
    {
        [JsonPropertyName("type")]
        public InstanceType Type { get; set; } = InstanceType.None;

        [JsonPropertyName("dungeon_type")]
        public DungeonType DungeonType { get; set; } = DungeonType.None;

        [JsonPropertyName("dungeon_faction")]
        public Faction DungeonFaction { get; set; } = Faction.None;

        [JsonPropertyName("mothership_income_override")]
        public int MothershipIncomeOverride { get; set; } = -1;

        [JsonPropertyName("characters")]
        public List<InstanceCharacter> Characters { get; set; } = new();

        [JsonPropertyName("extradata")]
        public string ExtraData { get; set; }

        [JsonPropertyName("map")]
        public string Map { get; set; }

        [JsonIgnore]
        public string Auth { get; set; }

        [JsonIgnore]
        public string Address { get; set; }

        [JsonIgnore]
        public int Port { get; set; }


        public InstanceCharacter GetCharacter(int id, string auth) =>
            Characters?.FirstOrDefault(c => c.Id == id && c.Auth == auth);
    }
}
