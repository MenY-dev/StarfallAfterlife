using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class SecretObjectsMap : SfaObject
    {
        public string Hash { get; set; }

        public Dictionary<int, SecretObjectInfo> Objects { get; } = new();

        protected Dictionary<int, HashSet<int>> Systems { get; } = new();

        public SecretObjectInfo GetObject(int id) =>
            Objects.GetValueOrDefault(id);

        public SecretObjectInfo[] GetSystemObjects(int systemId) => Systems
            .GetValueOrDefault(systemId)?
            .Select(GetObject)
            .Where(o => o is not null)
            .ToArray();

        public void AddObject(SecretObjectInfo secretObject)
        {
            if (secretObject is null)
                return;

            Objects[secretObject.Id] = secretObject;

            if (Systems.TryGetValue(secretObject.SystemId, out var system) == true && system is not null)
                system.Add(secretObject.Id);
            else
                Systems.Add(secretObject.SystemId, new() { secretObject.Id });
        }

        public bool RemoveObject(int id)
        {
            foreach (var system in Systems.Values)
                system.Remove(id);

            return Objects.Remove(id);
        }

        public override void LoadFromJson(JsonNode doc)
        {
            Hash = (string)doc?["hash"];
            Objects.Clear();

            if (doc?["data"].AsArray() is JsonArray asteroids)
            {
                foreach (JsonNode item in asteroids)
                    AddObject(item.DeserializeUnbuffered<SecretObjectInfo>());
            }
        }

        public override JsonNode ToJson()
        {
            var objects = new JsonArray();

            foreach (var item in Objects.Values)
                objects.Add(JsonHelpers.ParseNodeUnbuffered(item));

            return new JsonObject
            {
                ["hash"] = Hash,
                ["data"] = objects,
            };
        }

    }
}
