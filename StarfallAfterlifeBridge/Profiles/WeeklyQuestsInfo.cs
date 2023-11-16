using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class WeeklyQuestsInfo
    {
        [JsonPropertyName("seasons")]
        public List<WeeklyQuest> Seasons { get; set; } = new();

        [JsonPropertyName("stages")]
        public List<WeeklyQuestStage> Stages { get; set; } = new();

        [JsonPropertyName("rewards")]
        public List<WeeklyReward> Rewards { get; set; } = new();

        public WeeklyQuestStage[] GetStages(int questId)
        {
            return (Stages ??= new()).Where(s => s?.QuestId == questId).ToArray();
        }

        public WeeklyQuestStage[] GetStages(int questId, int xp)
        {
            return (Stages ??= new()).Where(s => s?.QuestId == questId && s.XpToOpen <= xp).ToArray();
        }
    }
}