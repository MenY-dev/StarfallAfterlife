using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    internal class GalaxyPatrolMobGenerator : GenerationTask
    {
        public SfaRealm Realm { get; set; }

        public Faction Faction { get; set; }

        public int Level { get; set; }

        public DiscoveryMobInfo Result { get; protected set; }

        public GalaxyPatrolMobGenerator(SfaRealm realm)
        {
            Realm = realm;
        }

        protected override bool Generate()
        {
            Result = Build();
            return Result is not null;
        }

        public DiscoveryMobInfo Build()
        {
            var serviceFleet = Realm?.MobsDatabase?.GetServiceFleet(Faction);

            if (serviceFleet is null)
                return null;

            var accessLvl = SfaDatabase.LevelToAccessLevel(Level);
            var availableShips = serviceFleet?.Ships
                .Where(s => s?.ServiceData is not null &&
                            s.ServiceData.FleetMin <= accessLvl &&
                            s.ServiceData.FleetMax >= accessLvl)
                .ToList();

            availableShips.Randomize(new Random128().Next());

            var mob = new DiscoveryMobInfo
            {
                Faction = Faction,
                InternalName = serviceFleet.InternalName,
                Level = Level,
                Ships = availableShips
                        .Take(new Random128().Next(2, 6))
                        .Select(s => s.Clone())
                        .ToList(),
            };

            return mob;
        }
    }
}
