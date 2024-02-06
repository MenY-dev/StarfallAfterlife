using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstancePlayer
    {
        [JsonPropertyName("user_id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("auth")]
        public string Auth { get; set; }

        [JsonPropertyName("fleet_id")]
        public int FleetId { get; set; } = -1;

        [JsonPropertyName("team")]
        public int Team { get; set; } = 0;

        [JsonPropertyName("is_spectator")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int IsSpectator { get; set; } = 0;
    }
}