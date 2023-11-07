using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

        public HashSet<int> TakenRewards { get; set; } = new();

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

        public bool IsExplored(DiscoveryObjectType type, int id) =>
            GetObjects(type)?.Contains(id) == true;

        public IReadOnlyCollection<int> GetObjects(DiscoveryObjectType type) => GetCollectionFromType(type);

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
            Planets = doc["planets"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            Portals = doc["portals"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            Motherships = doc["motherships"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            RepairStations = doc["repairStations"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            Fuelstations = doc["fuelstations"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            TradeStations = doc["tradeStations"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            MMS = doc["mms"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            SCS = doc["scs"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            PiratesStations = doc["pirates_stations"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            QuickTravelGates = doc["quick_travel_gates"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            SecretLocs = doc["secret_locs"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            WarpSystems = doc["warp_systems"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();

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

            CompletedQuests = doc["completed_quests"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
            ActiveQuests = doc["active_quests"]?.DeserializeUnbuffered<Dictionary<int, QuestProgress>>() ?? new();
            TakenRewards = doc["taken_rewards"]?.DeserializeUnbuffered<HashSet<int>>() ?? new();
        }

        public override JsonNode ToJson()
        {
            var doc = new JsonObject
            {
                ["id"] = CharacterId,
                ["planets"] = JsonHelpers.ParseNodeUnbuffered(Planets),
                ["portals"] = JsonHelpers.ParseNodeUnbuffered(Portals),
                ["motherships"] = JsonHelpers.ParseNodeUnbuffered(Motherships),
                ["repairStations"] = JsonHelpers.ParseNodeUnbuffered(RepairStations),
                ["fuelstations"] = JsonHelpers.ParseNodeUnbuffered(Fuelstations),
                ["tradeStations"] = JsonHelpers.ParseNodeUnbuffered(TradeStations),
                ["mms"] = JsonHelpers.ParseNodeUnbuffered(MMS),
                ["scs"] = JsonHelpers.ParseNodeUnbuffered(SCS),
                ["pirates_stations"] = JsonHelpers.ParseNodeUnbuffered(PiratesStations),
                ["quick_travel_gates"] = JsonHelpers.ParseNodeUnbuffered(QuickTravelGates),
                ["secret_locs"] = JsonHelpers.ParseNodeUnbuffered(SecretLocs),
                ["warp_systems"] = JsonHelpers.ParseNodeUnbuffered(WarpSystems)
            };

            doc["completed_quests"] = JsonHelpers.ParseNodeUnbuffered(CompletedQuests) ?? new JsonArray();
            doc["active_quests"] = JsonHelpers.ParseNodeUnbuffered(ActiveQuests) ?? new JsonArray();
            doc["taken_rewards"] = JsonHelpers.ParseNodeUnbuffered(TakenRewards) ?? new JsonArray();

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
