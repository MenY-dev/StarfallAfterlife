using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class ReachCharacterLevelConditionListener : QuestConditionListener, ICharacterListener
    {
        public int LevelToReach { get; set; }

        public ReachCharacterLevelConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            LevelToReach = (int?)doc?["level_to_reach"] ?? -1;
        }

        public override void Update()
        {
            base.Update();

            if (Quest?.Character is ServerCharacter character &&
                character.Level >= LevelToReach)
                Progress = ProgressRequire;
        }

        void ICharacterListener.OnCurrencyUpdated(ServerCharacter character)
        {
            Update();
        }

        void ICharacterListener.OnProjectResearch(ServerCharacter character, int projectId) { }

        void ICharacterListener.OnQuestCompleted(ServerCharacter character, QuestListener quest) { }

        void ICharacterListener.OnNewStatsReceived(ServerCharacter character, string tag, float value) { }
    }
}
