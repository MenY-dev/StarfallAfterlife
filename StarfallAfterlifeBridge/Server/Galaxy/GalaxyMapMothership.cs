using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System.Text.Json.Serialization;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapMothership : GalaxyMapStarSystemObject
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.Mothership;

        [JsonPropertyName("faction")]
        public Faction Faction { get; set; } = Faction.None;
    }
}