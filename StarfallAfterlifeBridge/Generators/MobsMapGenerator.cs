using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        protected record struct InfluenceInfo(Faction Faction, int Group, int Influence);

        protected Dictionary<int, InfluenceInfo> InfluenceMap { get; set; } = new();

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
            GenerateInfluenceMap(InfluenceMap = new(), 4);
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
            var influenceInfo = InfluenceMap.GetValueOrDefault(system.Id, new(Faction.None, -1, 0));

            var countRange = GetMobsCount(influenceInfo);
            var mobsCount = countRange.Min + (rnd.Next() % (countRange.Max - countRange.Min + 1));
            var totalCount = 0;

            for (int n = 0; n < mobsCount; n++)
            {
                foreach (var mob in GetMobsForCircle(system.Level))
                {
                    var deltaLvl = mob.Level - GetSystemMobsLevel(system);

                    if (deltaLvl < 0)
                        deltaLvl /= -3;

                    if ((rnd.Next() % (deltaLvl + 1)) != 0 ||
                        IsHabitationPossible(influenceInfo, mob.Faction) == false)
                        continue;

                    var variantCount = 1 + rnd.Next() % 4;

                    for (int i = 0; i < variantCount; i++)
                    {
                        CurrentMobId++;

                        var fleet = new GalaxyMapMob
                        {
                            FleetId = CurrentMobId,
                            MobId = mob.Id,
                            SystemId = system.Id,
                            FactionGroup = influenceInfo.Group,
                            ObjectId = -1,
                            ObjectType = GalaxyMapObjectType.None,
                            SpawnHex = SystemHexMap.ArrayIndexToHex(
                                rnd.Next(0, SystemHexMap.HexesCount))
                        };

                        map.AddMob(fleet);
                        totalCount++;

                        if (totalCount >= mobsCount)
                            break;
                    }

                    if (totalCount >= mobsCount)
                        break;
                }

                if (totalCount >= mobsCount)
                    break;
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

        protected bool IsHabitationPossible(InfluenceInfo influence, Faction mobFaction)
        {
            if (influence.Faction is Faction.NeutralPlanets &&
                mobFaction is (
                Faction.FreeTraders or
                Faction.MineworkerUnion or
                Faction.Scientists or
                Faction.NeutralPlanets))
                return true;

            return influence.Faction == mobFaction;
        }

        protected (int Min, int Max) GetMobsCount(InfluenceInfo influence)
        {
            if (influence.Faction.IsMainFaction() == true)
            {
                return new(4, 4);
            }

            if (influence.Faction.IsPirates() == true)
            {
                return new(influence.Influence switch
                {
                    > 3 => 3,
                    3 => 2,
                    2 => 2,
                    1 => 1,
                    _ => 0
                }, influence.Influence switch
                {
                    > 3 => 6,
                    3 => 5,
                    2 => 3,
                    1 => 2,
                    _ => 0
                });
            }

            return new(influence.Influence switch
            {
                > 3 => 4,
                3 => 3,
                2 => 2,
                1 => 1,
                _ => 0
            }, influence.Influence switch
            {
                > 3 => 8,
                3 => 6,
                2 => 4,
                1 => 2,
                _ => 0
            });
        }

        public IEnumerable<DiscoveryMobInfo> GetMobsForCircle(int circle)
        {
            return Realm?.MobsDatabase?
                .GetCircleMobs(circle)?
                .Where(m => m is not null)
                .Where(m => m.IsServiceFleet() == false && m.IsSpecOps() == false)
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

        private void GenerateInfluenceMap(Dictionary<int, InfluenceInfo> influenceMap, int maxInfluence = 4)
        {
            var radius = maxInfluence - 1;
            var galaxyMap = Realm?.GalaxyMap;

            if (radius < 1 || influenceMap is null || galaxyMap is null)
                return;

            foreach (var system in galaxyMap.Systems ?? new())
            {
                if (IsActiveFaction(system.Faction) == false)
                    continue;

                var faction = system.Faction;
                var factionGroup = system.FactionGroup;
                var radiusMultiplier = faction.IsPirates() ? 2 : 1;

                influenceMap[system.Id] = new(faction, factionGroup, maxInfluence);

                foreach (var neighbor in galaxyMap.GetSystemsArround(system.Id, radius * radiusMultiplier, true))
                {
                    var id = neighbor.Key.Id;
                    var newInfluence = maxInfluence - (int)Math.Round((float)neighbor.Value / radiusMultiplier);

                    if (IsActiveFaction(neighbor.Key.Faction) == true)
                        continue;

                    if (influenceMap.TryGetValue(id, out var currentInfluence) == false)
                    {
                        influenceMap[id] = new(faction, factionGroup, newInfluence);
                    }
                    else if (newInfluence > currentInfluence.Influence ||
                            (newInfluence == currentInfluence.Influence && faction.IsPirates()))
                    {
                        influenceMap[id] = new(faction, factionGroup, newInfluence);
                    }
                }
            }
        }

        private bool IsActiveFaction(Faction faction) => faction is
            Faction.Deprived or
            Faction.Eclipse or
            Faction.Vanguard or
            Faction.Screechers or
            Faction.Nebulords or
            Faction.Pyramid or
            Faction.FreeTraders or
            Faction.Scientists or
            Faction.NeutralPlanets or
            Faction.MineworkerUnion;
    }
}
