using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class ResearchProjectConditionListener : QuestConditionListener, ICharacterListener
    {
        public int EntityId { get; set; }

        public int MobId { get; set; }

        public bool DropEntity { get; set; }

        public ResearchProjectConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is null)
                return;

            EntityId = (int?)doc["entity_id"] ?? -1;
            MobId = (int?)doc["mob_id"] ?? -1;
            DropEntity = (int?)doc["drop_entity"] == 1;
        }

        public override void Update()
        {
            base.Update();

            Quest?.Character?.CheckResearch(EntityId).ContinueWith(t =>
            {
                if (t.Result == true)
                    Quest?.Client?.Invoke(() => Progress = ProgressRequire);
            });
        }

        public override List<DiscoveryDropRule> CreateDropRules()
        {
            var rules = base.CreateDropRules() ?? new();

            if (DropEntity == true)
            {
                rules.Add(new()
                {
                    Type = DropRuleType.Default,
                    Chance = 1,
                    DropPersonalItems = 1,
                    Mobs = new() { MobId },
                    Items = new() { new()
                    {
                        Count = 2,
                        Id = EntityId,
                        UniqueData = "",
                    }},
                });
            }

            return rules;
        }

        void ICharacterListener.OnCurrencyUpdated(ServerCharacter character) { }

        void ICharacterListener.OnProjectResearch(ServerCharacter character, int projectId)
        {
            if (projectId == EntityId)
                Progress = ProgressRequire;
        }

        void ICharacterListener.OnQuestCompleted(ServerCharacter character, QuestListener quest) { }

        void ICharacterListener.OnNewStatsReceived(ServerCharacter character, string tag, float value) { }
    }
}
