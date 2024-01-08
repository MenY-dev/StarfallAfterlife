using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapBuilder
    {
        public virtual GalaxyMap Create(int seed, float density = 5000, float radius = 300000)
        {
            GalaxyMap map = new();
            Random128 rnd = new Random128(seed);
            GalacticSystemsInfo info = GenerateSystemsLocations(rnd, radius, density);
            IList<Vector2> systemsLocations = info.Locations;
            IList<int> orbitsIndices = info.OrbitsIndices;
            Triangulator triangulator = new(systemsLocations);

            for (int i = 0; i < systemsLocations.Count; i++)
            {
                GalaxyMapStarSystem system = GalaxyMapStarSystem.Create(i, systemsLocations[i]);

                system.StarType = new Random128(i).Next(0, 15);
                system.Z = new Random128(i).Next(-3000, 3000);
                system.Faction = (Faction)new Random128(i).Next(10, 15);
                system.FactionGroup = 1;

                map.Systems.Add(system);
            }

            triangulator.Build();

            int portalID = 1000000;
            int lastOrbitIndex = orbitsIndices.Last();

            foreach (var node in triangulator.Nodes)
            {
                foreach (var childPoint in node.Children)
                {
                    if (node.Point >= lastOrbitIndex && childPoint >= lastOrbitIndex)
                    {
                        float distance = Vector2.Distance(
                            systemsLocations[node.Point],
                            systemsLocations[childPoint]);

                        if (distance > density)
                            continue;
                    }

                    var portalsA = map.Systems[node.Point].Portals;
                    var portalsB = map.Systems[childPoint].Portals;

                    portalsA.Add(new GalaxyMapPortal(
                        portalID,
                        childPoint,
                        GetPortalLocation(systemsLocations[node.Point], systemsLocations[childPoint])));
                    portalID++;

                    portalsB.Add(new GalaxyMapPortal(
                        portalID,
                        node.Point,
                        GetPortalLocation(systemsLocations[childPoint], systemsLocations[node.Point])));
                    portalID++;
                }
            }

            return map;
        }



        protected GalacticSystemsInfo GenerateSystemsLocations(Random128 rnd, float radius, float density)
        {
            GalacticSystemsInfo info = new GalacticSystemsInfo();
            int orbitsCount = (int)MathF.Floor(radius / density);
            float pi = 3.14159265359f;
            int maxStarError = (int)(density * 0.35f);
            IList<Vector2> locations = info.Locations;
            IList<int> orbitsIndices = info.OrbitsIndices;

            locations.Add(new Vector2(0, 0));
            orbitsIndices.Add(0);

            for (int i = 0; i < orbitsCount; i++)
            {
                float orbit = density * i;
                float orbitLength = pi * 2 * orbit;
                int starsCount = (int)MathF.Floor(orbitLength / density);

                orbitsIndices.Add(locations.Count);

                for (int n = 0; n < starsCount; n++)
                {
                    float azimuth = density * n / orbitLength * pi * 2;

                    locations.Add(new Vector2(
                        orbit * MathF.Cos(azimuth) + rnd.Next(-maxStarError, maxStarError),
                        orbit * MathF.Sin(azimuth) + rnd.Next(-maxStarError, maxStarError)));
                }
            }

            return info;
        }

        static readonly Vector2[] HexAngles = new Vector2[]
        {
            new Vector2(16, 0),
            new Vector2(0, 16),
            new Vector2(-16, 16),
            new Vector2(-16, 0),
            new Vector2(0, -16),
            new Vector2(16, -16),
        };

        protected Vector2 GetPortalLocation(Vector2 from, Vector2 to)
        {
            Vector2 direction = (to - from).Normalize();
            float angle = MathF.Atan2(direction.Y, direction.X);

            if (angle < MathF.PI / 6)
                angle += MathF.PI * 2;

            float progress = angle / MathF.PI * 3;
            int p1 = (int)(MathF.Floor(progress) % 6);
            int p2 = (p1 + 1) % 6;

            return Vector2.Lerp(HexAngles[p1], HexAngles[p2], (progress - p1) % 6);
        }

        public class GalacticSystemsInfo
        {
            public IList<Vector2> Locations { get; } = new List<Vector2>();

            public IList<int> OrbitsIndices { get; } = new List<int>();

        }
    }
}
