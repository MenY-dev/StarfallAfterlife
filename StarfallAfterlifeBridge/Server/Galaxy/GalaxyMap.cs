using Microsoft.VisualBasic;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Server.Discovery.NavigationMap;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMap : SfaObject
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("numFuelStations")]
        public int FuelStationsCount { get; set; }

        [JsonPropertyName("numPlanets")]
        public int PlanetsCount { get; set; }

        [JsonPropertyName("numPortals")]
        public int PortalsCount { get; set; }

        [JsonPropertyName("numRepairStations")]
        public int RepairStationsCount { get; set; }

        [JsonPropertyName("numStarSystems")]
        public int StarSystemsCount { get; set; }

        [JsonPropertyName("numTradeStations")]
        public int TradeStationsCount { get; set; }

        [JsonPropertyName("starlist")]
        public List<GalaxyMapStarSystem> Systems { get; set; } = new();

        [JsonIgnore]
        public GalaxyMapStatistics Statistics { get; } = new();

        [JsonIgnore]
        public Dictionary<Faction, int> StartSystems { get; } = new();

        public void UpdateStatistics()
        {
            Statistics.Build(this);
        }

        public GalaxyMapStarSystem GetSystem(int systemId)
        {
            if (systemId > -1 && systemId < Systems.Count)
                return Systems[systemId];

            return null;
        }

        public IEnumerable<GalaxyMapStarSystem> GetSystemsAtDistance(int systemId, int jumpsCount, bool limitLevel = false)
        {
            foreach (var system in GetSystemsArround(systemId, jumpsCount, limitLevel))
                if (system.Value == jumpsCount)
                    yield return system.Key;
        }

        public IEnumerable<KeyValuePair<GalaxyMapStarSystem, int>> GetSystemsArround(int systemId, int jumpsCount, bool limitLevel = false)
        {
            var startSystem = GetSystem(systemId);
            var initialLevel = startSystem.Level;

            if (startSystem is null || jumpsCount < 0)
                yield break;

            HashSet<GalaxyMapStarSystem> lastWave = new(16);
            HashSet<GalaxyMapStarSystem> currentWave = new(16);
            HashSet<GalaxyMapStarSystem> nextWave = new(16) { startSystem };

            void ShiftWaves()
            {
                var tmp = lastWave;
                lastWave = currentWave;
                currentWave = nextWave;
                nextWave = tmp;
                nextWave.Clear();
            }

            for (int currentJump = 0; currentJump <= jumpsCount; currentJump++)
            {
                ShiftWaves();

                foreach (var system in currentWave)
                {
                    yield return new(system, currentJump);

                    if (currentJump < jumpsCount && system.Portals is not null)
                    {
                        foreach (var nextSystem in system.Portals
                            .Where(p => p is not null)
                            .Select(p => GetSystem(p.Destination)))
                        {
                            if (nextSystem is not null &&
                                (limitLevel == false || nextSystem.Level == initialLevel) &&
                                currentWave.Contains(nextSystem) == false &&
                                lastWave.Contains(nextSystem) == false)
                                nextWave.Add(nextSystem);
                        }
                    }
                }
            }
        }

        public override void Init()
        {
            base.Init();

            int systemsCount = 0;
            int fuelStationsCount = 0;
            int tradeStationsCount = 0;
            int repairStationsCount = 0;
            int portalsCount = 0;
            int planetsCount = 0;

            foreach (var system in Systems)
            {
                systemsCount++;
                fuelStationsCount += system.FuelStations?.Count ?? 0;
                tradeStationsCount += system.TradeStations?.Count ?? 0;
                repairStationsCount += system.RepairStations?.Count ?? 0;
                portalsCount += system.Portals?.Count ?? 0;
                planetsCount += system.Planets?.Count ?? 0;

                if ((system.Motherships?.Count ?? 0) > 0)
                    StartSystems[(Faction)system.Faction] = system.Id;
            }

            StarSystemsCount = systemsCount;
            FuelStationsCount = fuelStationsCount;
            TradeStationsCount = tradeStationsCount;
            RepairStationsCount = repairStationsCount;
            PortalsCount = portalsCount;
            PlanetsCount = planetsCount;

            UpdateStatistics();
        }

        public JsonNode ToJsonNode(bool onlyVariableMap)
        {
            JsonArray exploredSystems = new JsonArray();
            JsonArray exploredPortals = new JsonArray();
            JsonArray exploredPlanets = new JsonArray();
            JsonArray exploredQuickTravelGates = new JsonArray();
            JsonArray exploredMotherships = new JsonArray();

            foreach (var system in Systems)
            {
                exploredSystems.Add(new JsonObject
                {
                    ["id"] = new SValue(system.Id),
                    ["mask"] = new SValue("/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////w==")
                });

                foreach (var item in system.Planets ?? new())
                {
                    exploredPlanets.Add(new SValue(item.Id));
                }

                foreach (var item in system.Portals ?? new())
                {
                    exploredPortals.Add(new SValue(item.Id));
                }

                foreach (var item in system.QuickTravalGates ?? new())
                {
                    exploredQuickTravelGates.Add(new SValue(item.Id));
                }

                foreach (var item in system.Motherships ?? new())
                {
                    exploredMotherships.Add(new SValue(item.Id));
                }
            }

            string galaxyMap = onlyVariableMap == false ? JsonNode.Parse(this).ToJsonString() : null;

            var doc = new JsonObject()
            {
                ["galaxymap"] = new SValue(galaxyMap),

                ["variablemap"] = new JsonObject
                {
                    ["renamedsystems"] = new JsonArray(),
                    ["renamedplanets"] = new JsonArray(),
                    ["faction_event"] = new JsonArray(),
                },

                ["charactmap"] = new JsonObject
                {
                    ["exploredneutralplanets"] = exploredPlanets,
                    ["exploredportals"] = exploredPortals,
                    ["exploredmotherships"] = exploredMotherships,
                    ["exploredrepairstations"] = new JsonArray(),
                    ["exploredfuelstations"] = new JsonArray(),
                    ["exploredtradestations"] = new JsonArray(),
                    ["exploredmms"] = new JsonArray(),
                    ["exploredscs"] = new JsonArray(),
                    ["exploredpiratesstations"] = new JsonArray(),
                    ["exploredquicktravelgate"] = exploredQuickTravelGates,
                    ["exploredsystems"] = exploredSystems,
                    ["exploredsecretloc"] = new JsonArray(),
                },
            };

            if (onlyVariableMap == true)
                doc["ok"] = 1;

            return doc;
        }

        public GalaxyMapStarSystem GetStartingSystem(Faction faction)
        {
            if (StartSystems.TryGetValue(faction, out var systemId) == true &&
                GetSystem(systemId) is GalaxyMapStarSystem system)
                return system;

            return Systems?.FirstOrDefault();
        }
    }
}
