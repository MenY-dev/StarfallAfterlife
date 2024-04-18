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

                if (doc["renamedsystems"]?.AsArraySelf() is JsonArray systems)
                {
                    foreach (var item in systems)
                    {
                        if (item is not JsonObject)
                            continue;

                        var info = item.DeserializeUnbuffered<RealmObjectRenameInfo>();

                        if (info is null)
                            continue;

                        RenamedSystems[info.Id] = info;
                    }
                }

                if (doc["renamedplanets"]?.AsArraySelf() is JsonArray planets)
                {
                    foreach (var item in planets)
                    {
                        if (item is not JsonObject)
                            continue;

                        var info = item.DeserializeUnbuffered<RealmObjectRenameInfo>();

                        if (info is null)
                            continue;

                        RenamedPlanets[info.Id] = info;
                    }
                }

                return true;
            }
            catch { }

            return false;
        }

        public JsonNode ToJson()
        {
            return new JsonObject
            {
                ["renamedsystems"] = RenamedSystems?.Values
                    .Where(r => r is not null)
                    .Select(r => JsonHelpers.ParseNodeUnbuffered(r)).ToJsonArray(),

                ["renamedplanets"] = RenamedPlanets?.Values
                    .Where(r => r is not null)
                    .Select(r => JsonHelpers.ParseNodeUnbuffered(r)).ToJsonArray(),
            };
        }
    }
}
