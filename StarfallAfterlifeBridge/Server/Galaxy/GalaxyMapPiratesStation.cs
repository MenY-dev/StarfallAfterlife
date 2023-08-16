using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System.Text.Json.Serialization;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapPiratesStation : GalaxyMapStarSystemObject
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.PiratesStation;

        [JsonPropertyName("faction")]
        public Faction Faction { get; set; } = 0;

        [JsonPropertyName("faction_group")]
        public int FactionGroup { get; set; } = 0;

        [JsonPropertyName("level")]
        public int Level { get; set; } = 0;
    }
}