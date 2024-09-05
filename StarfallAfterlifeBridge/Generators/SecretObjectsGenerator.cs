using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Generators
{
    public class SecretObjectsGenerator : GenerationTask
    {
        public int Seed { get; }

        public SfaRealm Realm { get; }

        public GalaxyExtraMap ExtraMap { get; protected set; }

        protected int ObjectId { get; set; } = 0;

        public SecretObjectsGenerator(SfaRealm realm, int seed = 0)
        {
            Realm = realm;
            ExtraMap = new GalaxyExtraMap(Realm.GalaxyMap);
            Seed = seed;
        }

        protected override bool Generate()
        {
            ObjectId = 0;
            Realm.SecretObjectsMap = Build();
            return true;
        }

        public virtual SecretObjectsMap Build()
        {
            var map = new SecretObjectsMap();
            var rnd = new Random128(Seed);

            ExtraMap.Build();

            IList<GalaxyMapStarSystem> GetSystems(GalaxyCircle circle, int seed) => circle.Systems.Values
                .Where(s => s.Faction.IsMainFaction() == false)
                .ToList()
                .Randomize(rnd.Next());


            foreach (var circle in ExtraMap.Circles.Values)
            {
                var systems = GetSystems(circle, rnd.Next());
                GenerateObjects(map, systems, SecretObjectType.Stash, Math.Max(20, systems.Count / 6), rnd);

                systems = GetSystems(circle, rnd.Next());
                GenerateObjects(map, systems, SecretObjectType.ShipsGraveyard, Math.Max(20, systems.Count / 8), rnd);
            }

            return map;
        }

        public void GenerateObjects(SecretObjectsMap map, IList<GalaxyMapStarSystem> systems, SecretObjectType type, int count, Random128 rnd)
        {
            if (systems is null or { Count: 0})
                return;

            for (var i = 0; i < count; i++)
            {
                ObjectId++;

                var system = systems[rnd.Next(0, systems.Count)];

                map.AddObject(new SecretObjectInfo
                {
                    Id = ObjectId,
                    SystemId = system.Id,
                    Level = system.Level,
                    Type = type,
                    Hex = CreateObjectHex(system, rnd)
                });
            }
        }

        private static SystemHex CreateObjectHex(GalaxyMapStarSystem system, Random128 rnd)
        {
            var count = SystemHexMap.HexesCount;
            var startIndex = rnd.Next(0, count);
            var starHexes = SystemHex.Zero.GetSpiralEnumerator(GetStarRadius(system));

            for (int i = 0; i < count; i++)
            {
                var index = (startIndex + i) % count;
                var hex = SystemHexMap.ArrayIndexToHex(index);
                var area = hex.GetSpiral(1);

                if (area.Any(h => system.GetObjectAt(h.X, h.Y) is not null || starHexes.Contains(h)) == false)
                    return hex;
            }

            return SystemHex.Zero;
        }

        public static int GetStarRadius(GalaxyMapStarSystem system) =>
            system is not null ? system.Size / 160 : 1;
    }
}
