using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class WeeklyQuestStage
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("weekly_quest")]
        public int QuestId { get; set; } = -1;

        [JsonPropertyName("open_xp")]
        public int XpToOpen { get; set; } = 0;

        [JsonPropertyName("skip_sfc_price")]
        public int SkipSfcPrice { get; set; } = 0;

    }
}
