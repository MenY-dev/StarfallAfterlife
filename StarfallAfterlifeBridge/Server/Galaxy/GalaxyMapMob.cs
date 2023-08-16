using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapMob
    {
        [JsonPropertyName("fleet_id")]
        public int FleetId { get; set; }

        [JsonPropertyName("mob_id")]
        public int MobId { get; set; }

        [JsonPropertyName("system_id")]
        public int SystemId { get; set; }

        [JsonPropertyName("faction_group")]
        public int FactionGroup { get; set; }

        [JsonPropertyName("spawn_hex")]
        public SystemHex SpawnHex { get; set; }

        [JsonPropertyName("object_type")]
        public GalaxyMapObjectType ObjectType { get; set; } = GalaxyMapObjectType.None;

        [JsonPropertyName("object_id")]
        public int ObjectId { get; set; } = -1;
    }
}
