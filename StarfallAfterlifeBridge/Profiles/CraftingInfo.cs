using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class CraftingInfo
    {
        [JsonPropertyName("id")]
        public int CraftingId { get; set; } = 0;

        [JsonPropertyName("project_entity")]
        public int ProjectEntity { get; set; } = 0;

        [JsonPropertyName("queue_position")]
        public int QueuePosition { get; set; } = 0;

        [JsonPropertyName("production_points_spent")]
        public int ProductionPointsSpent { get; set; } = 0;
    }
}