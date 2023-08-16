using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public interface ICharacterListener
    {
        public void OnQuestCompleted(ServerCharacter character, QuestListener quest);

        public void OnCurrencyUpdated(ServerCharacter character);

        public void OnProjectResearch(ServerCharacter character, int projectId);

        public void OnNewStatsReceived(ServerCharacter character, string tag, float value);
    }
}
