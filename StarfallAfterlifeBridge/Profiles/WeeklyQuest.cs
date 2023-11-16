using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class WeeklyQuest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("weekly_quests_seasons")]
        public int Seasons { get; set; } = 0;

        [JsonPropertyName("time_start")]
        public DateTime StartTime { get; set; } = new DateTime(2021, 1, 31);

        [JsonPropertyName("minutes_left")]
        public float MinutesLeft { get; set; } = 518400;

        [JsonPropertyName("is_active")]
        public int IsActive { get; set; } = 0;
    }
}
