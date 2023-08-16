using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    internal class StatTrackingConditionListener : QuestConditionListener, ICharacterListener
    {
        public float Factor { get; protected set; } = 1;

        public string StatTag { get; protected set; } = null;

        public StatTrackingConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }


        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            Factor = (float?)doc?["quest_value_factor"] ?? 1;
            StatTag = (string)doc?["stat_tag"];
        }

        void ICharacterListener.OnQuestCompleted(ServerCharacter character, QuestListener quest) { }

        void ICharacterListener.OnCurrencyUpdated(ServerCharacter character) { }

        void ICharacterListener.OnProjectResearch(ServerCharacter character, int projectId) { }

        void ICharacterListener.OnNewStatsReceived(ServerCharacter character, string tag, float value)
        {
            if (string.IsNullOrWhiteSpace(tag) || tag.Equals(StatTag, StringComparison.InvariantCultureIgnoreCase) == false)
                return;

            Progress = Math.Min(ProgressRequire, Math.Max(Progress, (int)(Progress + value * Factor)));
        }
    }
}
