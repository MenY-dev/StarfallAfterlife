using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapStarSystem : IGalaxyMapObject
    {
        [JsonIgnore]
        public GalaxyMapObjectType ObjectType => GalaxyMapObjectType.None;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("owning_faction")]
        public Faction Faction { get; set; } = Faction.None;

        [JsonPropertyName("owning_faction_group")]
        public int FactionGroup { get; set; } = -1;

        [JsonPropertyName("x")]
        public float X { get; set; } = 0;

        [JsonPropertyName("y")]
        public float Y { get; set; } = 0;

        [JsonPropertyName("z")]
        public float Z { get; set; } = 0;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nmask")]
        public string NebulaMask { get; set; }

        [JsonPropertyName("amask")]
        public string AsteroidsMask { get; set; }

        [JsonPropertyName("convexhull")]
        public bool Convexhull { get; set; }

        [JsonPropertyName("out")]
        public bool Out { get; set; }

        [JsonPropertyName("star_size")]
        public int Size { get; set; }

        [JsonPropertyName("star_temp")]
        public int Temperature { get; set; }

        [JsonPropertyName("star_type")]
        public int StarType { get; set; } = 0;

        [JsonPropertyName("star_weight")]
        public int Weight { get; set; } = 0;

        [JsonPropertyName("system_type")]
        public int Type { get; set; } = 0;

        [JsonPropertyName("cons")]
        public List<int> ConnectedSystems { get; }

        [JsonPropertyName("ter_edges")]
        public List<TerritoryEdge> TerritoryEdges { get; set; }

        [JsonPropertyName("pls")]
        public List<GalaxyMapPlanet> Planets { get; set; }

        [JsonPropertyName("port")]
        public List<GalaxyMapPortal> Portals { get; set; }

        [JsonPropertyName("motherships")]
        public List<GalaxyMapMothership> Motherships { get; set; }

        [JsonPropertyName("ps")]
        public List<GalaxyMapPiratesStation> PiratesStations { get; set; }

        [JsonPropertyName("po")]
        public List<GalaxyMapPiratesOutpost> PiratesOutposts { get; set; }

        [JsonPropertyName("rep")]
        public List<GalaxyMapRepairStation> RepairStations { get; set; }

        [JsonPropertyName("quick_travel_gates")]
        public List<GalaxyMapQuickTravalGate> QuickTravalGates { get; set; }

        [JsonPropertyName("mms")]
        public List<GalaxyMapMinerMotherships> MinerMotherships { get; set; }

        [JsonPropertyName("scs")]
        public List<GalaxyMapScienceStation> ScienceStations { get; set; }

        [JsonPropertyName("fls")]
        public List<GalaxyMapFuelStation> FuelStations { get; set; }

        [JsonPropertyName("ts")]
        public List<GalaxyMapTradeStation> TradeStations { get; set; }

        [JsonPropertyName("ra")]
        public List<GalaxyMapRichAsteroid> RichAsteroids { get; set; }

        [JsonIgnore]
        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        [JsonIgnore]
        int IGalaxyMapObject.X => (int)X;

        [JsonIgnore]
        int IGalaxyMapObject.Y => (int)Y;

        public GalaxyMapPlanet GetPlanet(int planetId) =>
            Planets?.FirstOrDefault(p => p.Id == planetId);

        public GalaxyMapPortal GetPortal(int portalId) =>
            Portals?.FirstOrDefault(p => p.Id == portalId);

        public GalaxyMapMothership GetMothership(int mothershipId) =>
            Motherships?.FirstOrDefault(m => m.Id == mothershipId);

        public GalaxyMapPiratesStation GetPiratesStation(int stationId) =>
            PiratesStations?.FirstOrDefault(p => p.Id == stationId);

        public IEnumerable<IGalaxyMapObject> GetObjectsWithTaskBoard()
        {
            if (Planets is not null)
                foreach (var item in Planets)
                    yield return item;

            if (RepairStations is not null)
                foreach (var item in RepairStations)
                    yield return item;

            if (MinerMotherships is not null)
                foreach (var item in MinerMotherships)
                    yield return item;

            if (ScienceStations is not null)
                foreach (var item in ScienceStations)
                    yield return item;

            if (TradeStations is not null)
                foreach (var item in TradeStations)
                    yield return item;

        }

        public IGalaxyMapObject GetObjectAt(int x, int y)
        {
            return GetAllObjects()?.FirstOrDefault(o => o?.X == x && o.Y == y);
        }

        public IEnumerable<IGalaxyMapObject> GetObjectsAt(int x, int y)
        {
            return GetAllObjects()?.Where(o => o?.X == x && o.Y == y);
        }

        public IEnumerable<IGalaxyMapObject> GetAllObjects()
        {
            if (Planets is not null)
                foreach (var item in Planets)
                    yield return item;

            if (Portals is not null)
                foreach (var item in Portals)
                    yield return item;

            if (Motherships is not null)
                foreach (var item in Motherships)
                    yield return item;

            if (PiratesStations is not null)
                foreach (var item in PiratesStations)
                    yield return item;

            if (PiratesOutposts is not null)
                foreach (var item in PiratesOutposts)
                    yield return item;

            if (RepairStations is not null)
                foreach (var item in RepairStations)
                    yield return item;

            if (QuickTravalGates is not null)
                foreach (var item in QuickTravalGates)
                    yield return item;

            if (MinerMotherships is not null)
                foreach (var item in MinerMotherships)
                    yield return item;

            if (ScienceStations is not null)
                foreach (var item in ScienceStations)
                    yield return item;

            if (FuelStations is not null)
                foreach (var item in FuelStations)
                    yield return item;

            if (TradeStations is not null)
                foreach (var item in TradeStations)
                    yield return item;

            if (RichAsteroids is not null)
                foreach (var item in RichAsteroids)
                    yield return item;

        }

        public Vector2 GetDefaultSpawnPosition(Faction faction = Database.Faction.None)
        {
            if ((Faction)Faction == faction &&
                 Motherships?.FirstOrDefault()?.Hex is SystemHex mothershipHex)
            {
                var hexes = mothershipHex
                    .GetRingEnumerator(2)
                    .Where(h => h.GetSize() < 17)
                    .ToList();

                return SystemHexMap.HexToSystemPoint(
                    hexes[new Random().Next(0, hexes.Count)]);
            }

            return Vector2.Zero;
        }

        public static GalaxyMapStarSystem Create(int id, Vector2 location)
        {
            return new GalaxyMapStarSystem
            {
                Id = id,
                Location = location,
                Name = "Star System " + id
            };
        }
    }
}
