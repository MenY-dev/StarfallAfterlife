using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public class HouseDoctrine
    {
        [JsonPropertyName("info")]
        public HouseDoctrineInfo Info { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("last_change")]
        public DateTime LastChange { get; set; }

        [JsonPropertyName("target")]
        public int Target { get; set; }

        [JsonPropertyName("progress")]
        public int Progress { get; set; }

        [JsonIgnore]
        public bool IsCompleted => Progress >= Target;

        [JsonIgnore]
        public bool IsEndOfTime => DateTime.UtcNow >= EndTime;

        public void AddToProgress(int count)
        {
            if (Progress >= Target ||
                IsEndOfTime == true)
                return;

            LastChange = DateTime.UtcNow;
            Progress += count;
        }
    }
}
