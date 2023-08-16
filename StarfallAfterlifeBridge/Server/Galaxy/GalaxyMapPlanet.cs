using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapPlanet : GalaxyMapStarSystemObject
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.Planet;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("pt")]
        public PlanetType Type { get; set; } = PlanetType.None;

        [JsonPropertyName("size")]
        public int Size { get; set; } = 600;

        [JsonPropertyName("temp")]
        public float Temperature { get; set; } = 28;

        [JsonPropertyName(name: "atmo")]
        public float Atmosphere { get; set; } = 0;

        [JsonPropertyName("grav")]
        public float Gravitation { get; set; } = 9.8f;

        [JsonPropertyName("noble_gases")]
        public float NoubleGases { get; set; } = 0;

        [JsonPropertyName("radiactive_metals")]
        public float RadiactiveMetals { get; set; } = 0;

        [JsonPropertyName("super_conductors")]
        public float SuperConductors { get; set; } = 0;

        [JsonPropertyName("faction")]
        public Faction Faction { get; set; } = Faction.None;

        [JsonPropertyName("secret_loc")]
        public List<int> SecretLocations { get; set; }
    }
}
