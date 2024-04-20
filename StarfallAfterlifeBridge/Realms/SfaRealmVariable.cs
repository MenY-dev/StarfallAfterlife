using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Realms
{
    public class SfaRealmVariable
    {
        public Dictionary<int, RealmObjectRenameInfo> RenamedSystems { get; set; } = new();

        public Dictionary<int, RealmObjectRenameInfo> RenamedPlanets { get; set; } = new();

        public Dictionary<int, RealmObjectNameReport> SystemNameReports { get; set; } = new();

        public Dictionary<int, RealmObjectNameReport> PlanetNameReports { get; set; } = new();

        public virtual bool Load(string path)
        {
            return Load(JsonHelpers.ParseNodeFromFile(path));
        }

        public virtual void Save(string path)
        {
            ToJson()?.WriteToFileUnbuffered(path);
        }

        public bool Load(JsonNode doc)
        {
            try
            {
                RenamedSystems = new();
                RenamedPlanets = new();

                if (doc is not JsonObject)
                    return false;

                void DeserializeItems<TKey, TValue>(Dictionary<TKey, TValue> items, JsonArray node, Func<TValue, TKey> keyGetter)
                {
                    if (keyGetter is null)
                        return;

                    foreach (var item in node)
                    {
                        try
                        {
                            if (item is not JsonObject)
                                continue;

                            var info = item.DeserializeUnbuffered<TValue>();

                            if (info is null)
                                continue;

                            var key = keyGetter(info);

                            if (info is null)
                                continue;

                            items[key] = info;
                        }
                        catch { }
                    }
                }

                if (doc["renamed_systems"]?.AsArraySelf() is JsonArray systems)
                    DeserializeItems(RenamedSystems, systems, i => i.Id);

                if (doc["renamed_planets"]?.AsArraySelf() is JsonArray planets)
                    DeserializeItems(RenamedPlanets, planets, i => i.Id);

                if (doc["system_name_reports"]?.AsArraySelf() is JsonArray systemReports)
                    DeserializeItems(SystemNameReports, systemReports, i => i.Id);

                if (doc["planet_name_reports"]?.AsArraySelf() is JsonArray planetReports)
                    DeserializeItems(PlanetNameReports, planetReports, i => i.Id);

                return true;
            }
            catch { }

            return false;
        }

        public JsonNode ToJson() => ToJson(false);

        public JsonNode ToJson(bool ignoreReports)
        {
            var doc = new JsonObject
            {
                ["renamed_systems"] = RenamedSystems?.Values
                    .Where(r => r is not null)
                    .Select(r => JsonHelpers.ParseNodeUnbuffered(r)).ToJsonArray(),

                ["renamed_planets"] = RenamedPlanets?.Values
                    .Where(r => r is not null)
                    .Select(r => JsonHelpers.ParseNodeUnbuffered(r)).ToJsonArray(),
            };

            if (ignoreReports == false)
            {
                doc["system_name_reports"] = SystemNameReports?.Values
                    .Where(r => r is not null)
                    .Select(r => JsonHelpers.ParseNodeUnbuffered(r)).ToJsonArray();

                doc["planet_name_reports"] = PlanetNameReports?.Values
                    .Where(r => r is not null)
                    .Select(r => JsonHelpers.ParseNodeUnbuffered(r)).ToJsonArray();
            }

            return doc;
        }

        public void ReportSystem(int id, string profileId, string profileName)
        {
            if (id > -1 && profileId is not null)
                Report(SystemNameReports ??= new(), id, profileId, profileName);
        }

        public void ReportPlanet(int id, string profileId, string profileName)
        {
            if (id > -1 && profileId is not null)
                Report(PlanetNameReports ??= new(), id, profileId, profileName);
        }

        protected void Report(Dictionary<int, RealmObjectNameReport> collection, int id, string profileId, string profileName)
        {
            if (collection is null || profileId is null)
                return;

            var report = collection.GetValueOrDefault(id);

            if (report is null)
                collection[id] = report = new() { Id = id };

            var reportAuthor = (report.Authors ??= new()).FirstOrDefault(
                a => profileId.Equals(a.PlayerId, StringComparison.OrdinalIgnoreCase));

            if (reportAuthor is null)
            {
                reportAuthor = new() { PlayerId = profileId };
                report.Authors.Add(reportAuthor);
            }

            if (profileName is not null)
                reportAuthor.PlayerName = profileName;
        }
    }
}
