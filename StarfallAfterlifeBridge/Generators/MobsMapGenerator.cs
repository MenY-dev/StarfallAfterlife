using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace StarfallAfterlife.Bridge.Generators
{
    public class MobsMapGenerator : GenerationTask
    {
        public SfaRealm Realm { get; set; }

        public GalaxyExtraMap ExtraMap { get; protected set; }

        protected int CurrentMobId { get; set; }


        public MobsMapGenerator(SfaRealm realm)
        {
            if (realm is null)
                return;

            Realm = realm;
            ExtraMap = new GalaxyExtraMap(Realm.GalaxyMap);
        }

        protected override bool Generate()
        {
            Realm.MobsMap = Build();
            return true;
        }

        public virtual MobsMap Build()
        {
            var map = new MobsMap();
            ExtraMap.Build();
            CurrentMobId = 1000000;
            GenerateMobsMap(map);
            return map;
        }

        private void GenerateMobsMap(MobsMap map)
        {
            GenerateForMap(map);
        }


        private void GenerateForMap(MobsMap map)
        {
            if (Realm?.Database is SfaDatabase database)
            {
                foreach (var system in Realm?.GalaxyMap?.Systems ?? new())
                {
                    GenerateForSystem(map, system, database);
                }
            }

        }

        private void GenerateForSystem(MobsMap map, GalaxyMapStarSystem system, SfaDatabase database)
        {
            var rnd = new Random(system.Id);

            foreach (var mob in GetMobsForCircle(system.Level))
            {
                var deltaLvl = Math.Abs(mob.Level - GetSystemMobsLevel(system));

                if ((rnd.Next() % (deltaLvl + 1)) != 0 ||
                    (system.Faction != Faction.None && system.Faction != mob.Faction))
                    continue;

                int count = 1 + rnd.Next() % 3;

                for (int i = 0; i < count; i++)
                {
                    CurrentMobId++;

                    var fleet = new GalaxyMapMob
                    {
                        FleetId = CurrentMobId,
                        MobId = mob.Id,
                        SystemId = system.Id,
                        FactionGroup = system.FactionGroup,
                        ObjectId = -1,
                        ObjectType = GalaxyMapObjectType.None,
                        SpawnHex = SystemHexMap.ArrayIndexToHex(
                            rnd.Next(0, SystemHexMap.HexesCount))
                    };

                    map.AddMob(fleet);
                }
            }

            foreach (var item in system.PiratesOutposts ?? new())
                if (item is not null)
                    GenerateForPiratesOutpost(map, system, item, database);

            foreach (var item in system.PiratesStations ?? new())
                if (item is not null)
                    GenerateForPiratesStation(map, system, item, database);
        }

        private void GenerateForPiratesOutpost(MobsMap map, GalaxyMapStarSystem system, GalaxyMapPiratesOutpost outpost, SfaDatabase database)
        {
            var bosses = GetBosses(outpost.Level, outpost.Faction)
                .Where(m => m.Tags?.Contains("Mob.Role.Outpost", StringComparer.InvariantCultureIgnoreCase) ?? false)
                .ToList();

            var bossesCount = 1 + outpost.Level / 3;
            var rnd = new Random(outpost.Id);

            if (bosses.Count < 1)
                return;

            for (int i = 0; i < bossesCount; i++)
            {
                var bossIndex = rnd.Next() % bosses.Count;
                var boss = bosses[bossIndex];

                if (boss is null)
                    continue;

                CurrentMobId++;

                var fleet = new GalaxyMapMob
                {
                    FleetId = CurrentMobId,
                    MobId = boss.Id,
                    SystemId = system.Id,
                    FactionGroup = system.FactionGroup,
                    ObjectId = outpost.Id,
                    ObjectType = outpost.ObjectType,
                    SpawnHex = SystemHex.Zero
                };

                map.AddMob(fleet);
            }
        }

        private void GenerateForPiratesStation(MobsMap map, GalaxyMapStarSystem system, GalaxyMapPiratesStation station, SfaDatabase database)
        {
            var bosses = GetBosses(station.Level, station.Faction)
                .Where(m => m.Tags?.Contains("Mob.Role.Station", StringComparer.InvariantCultureIgnoreCase) ?? false)
                .ToList();

            var bossesCount = 3 + station.Level / 2;
            var rnd = new Random(station.Id);

            if (bosses.Count < 1)
                return;

            for (int i = 0; i < bossesCount; i++)
            {
                var bossIndex = rnd.Next() % bosses.Count;
                var boss = bosses[bossIndex];

                if (boss is null)
                    continue;

                CurrentMobId++;

                var fleet = new GalaxyMapMob
                {
                    FleetId = CurrentMobId,
                    MobId = boss.Id,
                    SystemId = system.Id,
                    FactionGroup = system.FactionGroup,
                    ObjectId = station.Id,
                    ObjectType = station.ObjectType,
                    SpawnHex = SystemHex.Zero
                };

                map.AddMob(fleet);
            }
        }

        public IEnumerable<DiscoveryMobInfo> GetMobsForCircle(int circle)
        {
            return Realm?.MobsDatabase?
                .GetCircleMobs(circle)?
                .Where(m => m is not null)
                .Where(m => m.IsServiceFleet() == false)
                ?? new List<DiscoveryMobInfo>();
        }

        public IEnumerable<DiscoveryMobInfo> GetBosses(int circle, Faction faction)
        {
            return GetMobsForCircle(circle)
                .Where(m => m.Faction == faction)
                .Where(m => m.IsBoss());
        }

        public int GetSystemMobsLevel(GalaxyMapStarSystem system)
        {
            if (system is null)
                return 0;

            var circle = ExtraMap.GetCircle(system.Level);

            if (circle is null)
                return 0;

            var posRange = Math.Abs(circle.MaxRadius - circle.MinRadius);
            var minLevel = SfaDatabase.GetCircleMinLevel(system.Level);
            var maxLevel = SfaDatabase.GetCircleMaxLevel(system.Level);
            var levelRange = maxLevel - minLevel;

            if (posRange == 0 || levelRange == 0)
                return maxLevel;

            var dt = (circle.DeprivedStartSystem.Location - system.Location).GetSize() / posRange;
            var et = (circle.EclipseStartSystem.Location - system.Location).GetSize() / posRange;
            var vt = (circle.VanguardStartSystem.Location - system.Location).GetSize() / posRange;
            var t = Math.Min(Math.Min(Math.Min(dt, et), vt), 1);

            return minLevel + (int)(levelRange * t);
        }
    }
}
