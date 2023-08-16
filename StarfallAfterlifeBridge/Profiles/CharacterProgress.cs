using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class CharacterProgress : SfaObject
    {
        public int CharacterId { get; set; } = -1;
        public HashSet<int> Planets { get; set; } = new();
        public HashSet<int> Portals { get; set; } = new();
        public HashSet<int> Motherships { get; set; } = new();
        public HashSet<int> RepairStations { get; set; } = new();
        public HashSet<int> Fuelstations { get; set; } = new();
        public HashSet<int> TradeStations { get; set; } = new();
        public HashSet<int> MMS { get; set; } = new();
        public HashSet<int> SCS { get; set; } = new();
        public HashSet<int> PiratesStations { get; set; } = new();
        public HashSet<int> QuickTravelGates { get; set; } = new();
        public HashSet<int> SecretLocs { get; set; } = new();

        public Dictionary<int, SystemHexMap> Systems { get; set; } = new();
        public HashSet<int> WarpSystems { get; set; } = new();

        public Dictionary<int, QuestProgress> ActiveQuests { get; set; } = new();
        public HashSet<int> CompletedQuests { get; set; } = new();

        public string Path { get; set; }


        public bool AddObject(GalaxyMapObjectType type, int id, int system = -1) =>
            AddObject((DiscoveryObjectType)type, id, system);

        public bool AddObject(DiscoveryObjectType type, int id, int system = -1)
        {
            if (system > -1 && type == DiscoveryObjectType.QuickTravelGate)
                WarpSystems.Add(system);

            return GetCollectionFromType(type)?.Add(id) ?? false;
        }


        public void SetSystemProgress(int systemId, SystemHexMap progress)
        {
            Systems ??= new();
            Systems[systemId] = progress ?? new();
        }

        protected virtual HashSet<int> GetCollectionFromType(DiscoveryObjectType type)
        {
            switch (type)
            {
                case DiscoveryObjectType.Planet: return Planets ??= new();
                case DiscoveryObjectType.WarpBeacon: return Portals ??= new();
                case DiscoveryObjectType.Mothership: return Motherships ??= new();
                case DiscoveryObjectType.Repairstation: return RepairStations ??= new();
                case DiscoveryObjectType.Fuelstation: return Fuelstations ??= new();
                case DiscoveryObjectType.Tradestation: return TradeStations ??= new();
                case DiscoveryObjectType.PiratesStation: return PiratesStations ??= new();
                case DiscoveryObjectType.MinerMothership: return MMS ??= new();
                case DiscoveryObjectType.ScienceStation: return SCS ??= new();
                case DiscoveryObjectType.QuickTravelGate: return QuickTravelGates ??= new();
                case DiscoveryObjectType.SecretObject: return SecretLocs ??= new();
                default: return null;
            }
        }

        public override void LoadFromJson(JsonNode doc)
        {
            if (doc is not JsonObject)
                return;

            CharacterId = (int?)doc["id"] ?? -1;
            Planets = doc["planets"]?.Deserialize<HashSet<int>>() ?? new();
            Portals = doc["portals"]?.Deserialize<HashSet<int>>() ?? new();
            Motherships = doc["motherships"]?.Deserialize<HashSet<int>>() ?? new();
            RepairStations = doc["repairStations"]?.Deserialize<HashSet<int>>() ?? new();
            Fuelstations = doc["fuelstations"]?.Deserialize<HashSet<int>>() ?? new();
            TradeStations = doc["tradeStations"]?.Deserialize<HashSet<int>>() ?? new();
            MMS = doc["mms"]?.Deserialize<HashSet<int>>() ?? new();
            SCS = doc["scs"]?.Deserialize<HashSet<int>>() ?? new();
            PiratesStations = doc["pirates_stations"]?.Deserialize<HashSet<int>>() ?? new();
            QuickTravelGates = doc["quick_travel_gates"]?.Deserialize<HashSet<int>>() ?? new();
            SecretLocs = doc["secret_locs"]?.Deserialize<HashSet<int>>() ?? new();
            WarpSystems = doc["warp_systems"]?.Deserialize<HashSet<int>>() ?? new();

            Systems = new();

            if (doc["systems"] is JsonArray systems)
            {
                foreach (var system in systems)
                {
                    if ((int?)system["id"] is int id &&
                        (string)system["map"] is string map)
                        Systems.Add(id, new SystemHexMap(map));
                }
            }

            CompletedQuests = doc["completed_quests"]?.Deserialize<HashSet<int>>() ?? new();
            ActiveQuests = doc["active_quests"]?.Deserialize<Dictionary<int, QuestProgress>>() ?? new();
        }

        public override JsonNode ToJson()
        {
            var doc = new JsonObject
            {
                ["id"] = CharacterId,
                ["planets"] = JsonNode.Parse(Planets),
                ["portals"] = JsonNode.Parse(Portals),
                ["motherships"] = JsonNode.Parse(Motherships),
                ["repairStations"] = JsonNode.Parse(RepairStations),
                ["fuelstations"] = JsonNode.Parse(Fuelstations),
                ["tradeStations"] = JsonNode.Parse(TradeStations),
                ["mms"] = JsonNode.Parse(MMS),
                ["scs"] = JsonNode.Parse(SCS),
                ["pirates_stations"] = JsonNode.Parse(PiratesStations),
                ["quick_travel_gates"] = JsonNode.Parse(QuickTravelGates),
                ["secret_locs"] = JsonNode.Parse(SecretLocs),
                ["warp_systems"] = JsonNode.Parse(WarpSystems)
            };

            doc["completed_quests"] = JsonNode.Parse(CompletedQuests) ?? new JsonArray();
            doc["active_quests"] = JsonNode.Parse(ActiveQuests) ?? new JsonArray();

            var systems = new JsonArray();

            foreach (var item in Systems)
            {
                systems.Add(new JsonObject
                {
                    ["id"] = item.Key,
                    ["map"] = item.Value.ToBase64String()
                });
            }

            doc["systems"] = systems;
            return doc;
        }

        public bool Load()
        {
            try
            {
                if (File.Exists(Path) == false)
                    return false;

                var text = File.ReadAllText(Path);
                var doc = JsonNode.Parse(text);
                LoadFromJson(doc);
                return true;
            }
            catch { }

            return false;
        }

        public bool Save()
        {
            try
            {
                if (System.IO.Path.GetDirectoryName(Path) is string dir &&
                    Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                var doc = ToJson();
                var text = doc.ToJsonString(false);
                File.WriteAllText(Path, text);
                return true;
            }
            catch { }

            return false;
        }

        public bool RemoveProgressFile()
        {
            try
            {
                File.Delete(Path);
                return true;
            }
            catch { }

            return false;
        }
    }
}
