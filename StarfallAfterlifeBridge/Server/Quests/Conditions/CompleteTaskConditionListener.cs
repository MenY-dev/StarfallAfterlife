using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class CompleteTaskConditionListener : QuestConditionListener, ICharacterListener
    {
        public int TaskStarSystemLevel { get; set; }

        public CompleteTaskConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            TaskStarSystemLevel = (int?)doc?["TaskStarSystemLevel"] ?? -1;
        }

        void ICharacterListener.OnCurrencyUpdated(ServerCharacter character) { }

        void ICharacterListener.OnProjectResearch(ServerCharacter character, int projectId) { }

        void ICharacterListener.OnQuestCompleted(ServerCharacter character, QuestListener quest)
        {
            if (quest is null ||
                quest.Info.Type != QuestType.Task ||
                (TaskStarSystemLevel != -1 && quest.Info.Level < TaskStarSystemLevel))
                return;

            Progress = Math.Min(ProgressRequire, Progress + 1);
        }

        void ICharacterListener.OnNewStatsReceived(ServerCharacter character, string tag, float value) { }
    }
}
