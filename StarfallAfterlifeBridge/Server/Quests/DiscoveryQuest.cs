using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DiscoveryQuest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("logic_id")]
        public int LogicId { get; set; }

        [JsonPropertyName("type")]
        public QuestType Type { get; set; } = QuestType.Task;

        [JsonPropertyName("unique_logic_identifier")]
        public string LogicName { get; set; }

        [JsonPropertyName("lvl")]
        public int Level { get; set; } = -1;

        [JsonPropertyName("conditions"), JsonConverter(typeof(JsonNodeStringConverter))]
        public JsonArray Conditions { get; set; }

        [JsonPropertyName("object_system")]
        public int ObjectSystem { get; set; } = -1;

        [JsonPropertyName("object_type")]
        public GalaxyMapObjectType ObjectType { get; set; } = GalaxyMapObjectType.None;

        [JsonPropertyName("object_id")]
        public int ObjectId { get; set; } = -1;

        [JsonPropertyName("object_faction")]
        public Faction ObjectFaction { get; set; } = Faction.None;

        [JsonPropertyName("reward")]
        public QuestReward Reward { get; set; } = new();

        public List<DiscoveryQuestBinding> CreateBindings()
        {
            return Conditions?
                .Select(c => c["bindings"]?.Deserialize<List<DiscoveryQuestBinding>>())
                .Where(b => b is not null)
                .SelectMany(b => b)
                .Where(b => b is not null)
                .ToList() ?? new();
        }
    }
}
