using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DiscoveryQuestLine : SfaObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public SortedList<int, DiscoveryQuestLineStage> Stages { get; set; } = new();

        public DiscoveryQuestLineStage GetCurrentStage(ServerCharacter character)
        {
            if (character is null ||
                Stages is null)
                return null;

            if (character.Progress?.CompletedQuests is HashSet<int> completedQuests)
            {
                foreach (var stage in Stages.Values)
                {
                    if (stage is null || stage.Entries is null)
                        continue;

                    if (stage.Entries.Any(e => e is not null && completedQuests.Contains(e.QuestId) == false))
                        return stage;
                }
            }

            return null;
        }

        public List<int> GetNewQuests(ServerCharacter character)
        {
            var quests = new List<int>();

            if (character is null)
                return quests;

            if (character.Progress?.CompletedQuests is HashSet<int> completedQuests &&
                character.ActiveQuests is List<QuestListener> activeQuests &&
                GetCurrentStage(character) is DiscoveryQuestLineStage stage)
            {
                foreach (var entry in stage.Entries ?? new())
                {
                    if (entry is null ||
                        entry.QuestId == -1 ||
                        completedQuests.Contains(entry.QuestId) ||
                        activeQuests.Any(q => q?.Id == entry.QuestId) ||
                        character.AccessLevel < entry.AccesLevel)
                        continue;

                    quests.Add(entry.QuestId);
                }
            }

            return quests;
        }


        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is null)
                return;

            Id = (int?)doc["id"] ?? -1;
            Name = (string)doc["name"] ?? "";

            foreach (var item in doc["stages"]?.AsArraySelf() ?? new())
            {
                var stage = item?.DeserializeUnbuffered<DiscoveryQuestLineStage>();

                if (stage is not null)
                    Stages.Add(stage.Position, stage);
            }
        }

        public override JsonNode ToJson()
        {
            var doc = base.ToJson();

            if (doc is not JsonObject)
                doc = new JsonObject();

            doc["id"] = Id;
            doc["name"] = Name;
            doc["stages"] = new JsonArray(Stages.Values.Select(
                s => JsonHelpers.ParseNodeUnbuffered(s)).ToArray());

            return doc;
        }
    }
}
