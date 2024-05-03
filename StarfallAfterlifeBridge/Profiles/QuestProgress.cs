using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class QuestProgress
    {
        [JsonPropertyName("conditions")]
        public Dictionary<string, ConditionProgress> Conditions { get; set; } = new();

        [JsonPropertyName("quest_data"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DiscoveryQuest QuestData { get; set; }

        public bool IsDynamic => QuestData is DiscoveryQuest quest &&
                                 QuestIdInfo.Create(quest.Id).IsDynamicQuest;

        public ConditionProgress GetProgress(string conditionName) =>
            Conditions.GetValueOrDefault(conditionName);

        public void SetProgress(string condition, int progress)
        {
            if (GetProgress(condition) is ConditionProgress item)
                item.Progress = progress;
            else
            {
                Conditions.Add(condition, new() { Progress = progress });
            }
        }

        public void SetProgress(string condition, ConditionProgress progress) =>
            Conditions[condition ?? string.Empty] = progress;

        public int GetOption(string condition, string option) =>
            GetProgress(condition)?.GetOption(option) ?? default;

        public void SetOption(string condition, string option, int value)
        {
            var progress = GetProgress(condition);

            if (progress is null)
            {
                progress = new();
                SetProgress(condition, progress);
            }

            progress.SetOption(option, value);
        }
    }
}