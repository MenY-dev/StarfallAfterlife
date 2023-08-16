using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class QuestLogicInfo : ICloneable
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("type")]
        public QuestType Type { get; set; } = QuestType.Task;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("unique_logic_identifier")]
        public string UniqueLogicIdentifier { get; set; }

        [JsonPropertyName("conditions")]
        public List<QuestConditionInfo> Conditions { get; set; } = new();

        [JsonPropertyName("rewards")]
        public List<QuestReward> Rewards { get; set; } = new();

        public QuestLogicInfo() { }

        public QuestLogicInfo(JsonNode doc)
        {
            if (doc is null)
                return;

            Id = (int?)doc["id"] ?? -1;
            Type = (QuestType?)(int?)doc["type"] ?? QuestType.Task;
            Name = (string)doc["name"] ?? string.Empty;
            UniqueLogicIdentifier = (string)doc["unique_logic_identifier"] ?? string.Empty;

            foreach (var item in doc["conditions"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
                Conditions.Add(new(item));

            foreach (var item in doc["rewards"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
                Rewards.Add(new(item));
        }

        object ICloneable.Clone() => Clone();

        public QuestLogicInfo Clone()
        {
            var c = new QuestLogicInfo();

            c.Id = Id;
            c.Type = Type;
            c.Name = Name;
            c.UniqueLogicIdentifier = UniqueLogicIdentifier;
            c.Conditions = Conditions?.Select(i => i.Clone()).ToList() ?? new();
            c.Rewards = Rewards?.Select(i => i.Clone()).ToList() ?? new();

            return c;
        }
    }
}
