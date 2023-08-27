using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class MobsMap : SfaObject
    {
        public string Hash { get; set; }

        public Dictionary<int, GalaxyMapMob> Mobs { get; } = new();

        protected Dictionary<int, HashSet<GalaxyMapMob>> SystemsMap { get; } = new();

        public void AddMob(GalaxyMapMob mob)
        {
            if (mob is null)
                return;

            Mobs[mob.FleetId] = mob;

            if (SystemsMap.TryGetValue(mob.SystemId, out HashSet<GalaxyMapMob> system) == true &&
                system is not null)
                system.Add(mob);
            else
                SystemsMap[mob.SystemId] = new() { mob };
        }

        public GalaxyMapMob GetMob(int fleetId) =>
            Mobs.GetValueOrDefault(fleetId);

        public List<GalaxyMapMob> GetSystemMobs(int systemId) =>
            SystemsMap.GetValueOrDefault(systemId)?.ToList();

        public List<GalaxyMapMob> GetObjectMobs(int systemId, DiscoveryObjectType objectType, int objectId) =>
            GetObjectMobs(systemId, (GalaxyMapObjectType)objectType, objectId);

        public List<GalaxyMapMob> GetObjectMobs(int systemId, GalaxyMapObjectType objectType, int objectId) =>
            SystemsMap
            .GetValueOrDefault(systemId)?
            .Where(m => m.ObjectType == objectType && m.ObjectId == objectId)
            .ToList();

        public override void LoadFromJson(JsonNode doc)
        {
            if (doc is null)
                return;

            Hash = (string)doc?["hash"];

            Mobs.Clear();
            SystemsMap.Clear();

            if (doc["mobs"]?.AsArray() is JsonArray mobs)
                foreach (var mobDoc in mobs)
                    if (mobDoc.DeserializeUnbuffered<GalaxyMapMob>() is GalaxyMapMob mob)
                        AddMob(mob);
        }

        public override JsonNode ToJson()
        {
            var mobs = new JsonArray();

            if (Mobs is not null)
                foreach (var mob in Mobs.Values)
                    mobs.Add(JsonHelpers.ParseNodeUnbuffered(mob));

            return new JsonObject
            {
                ["hash"] = Hash,
                ["mobs"] = mobs,
            };
        }
    }
}
