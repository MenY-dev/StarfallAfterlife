using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class RichAsteroidsMap : SfaObject
    {
        public string Hash { get; set; }

        public SystemObjectsDictionary<RichAsteroid> Asteroids { get; } = new();

        public IEnumerable<RichAsteroid> GetAsteroids(int systemId) => Asteroids[systemId];

        public void AddAsteroid(int systemId, RichAsteroid richAsteroid) =>
            Asteroids.Add(systemId, richAsteroid);

        public bool RemoveAsteroid(int systemId, RichAsteroid richAsteroid) =>
            Asteroids.Remove(systemId, richAsteroid);

        public bool RemoveAsteroid(RichAsteroid richAsteroid) =>
            Asteroids.Remove(richAsteroid);

        public override void LoadFromJson(JsonNode doc)
        {
            Hash = (string)doc?["hash"];
            Asteroids.Clear();

            if (doc?["data"].AsArray() is JsonArray asteroids)
            {
                foreach (JsonNode item in asteroids)
                {
                    var asteroid = item.Deserialize<RichAsteroid>();
                    Asteroids.Add(asteroid.SystemId, asteroid);
                }
            }
        }

        public override JsonNode ToJson()
        {
            var asteroids = new JsonArray();

            foreach (var system in Asteroids.Values)
            {
                foreach (var item in system)
                {
                    asteroids.Add(JsonNode.Parse(item));
                }
            }

            return new JsonObject
            {
                ["hash"] = Hash,
                ["data"] = asteroids,
            };
        }
    }
}
