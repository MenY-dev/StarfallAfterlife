using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer
    {
        public DynamicMobDatabase DynamicMobs { get; } = new();

        public Dictionary<int, (DiscoveryMobInfo Fleet, DateTime UpdateTime)> FleetsCache { get; } = new();

        public TimeSpan FleetCacheLifetime { get; set; } = TimeSpan.FromMinutes(5);

        protected object DynamicMobsLocker { get; } = new();

        protected object FleetsCacheLocker { get; } = new();

        public void SpawnBlockadeInSystem(StarSystem system)
        {
            Galaxy?.BeginPreUpdateAction(_ =>
            {
                try
                {
                    var systemInfo = system.Info;

                    if (systemInfo is null)
                        return;

                    var faction = systemInfo.Faction;
                    var factionGroup = systemInfo.FactionGroup;

                    if ((faction.IsPirates() || faction == Faction.None) == false)
                        return;

                    if (faction.IsPirates() == false)
                    {
                        var piratesFleet = system.Fleets.FirstOrDefault(f => f.Faction.IsPirates() == true);

                        if (piratesFleet is null)
                            return;

                        faction = piratesFleet.Faction;
                        factionGroup = piratesFleet.FactionGroup;
                    }

                    var allObjects = system
                        .GetAllObjects()
                        .Select(o => (Object: o, Hex: o.Hex, Radius: system.GetObjectRadius(o)))
                        .Append((null, SystemHex.Zero, system.GetStarRadius()))
                        .ToList();

                    var rnd = new Random128(Realm?.Seed ?? 0 + system.Id * 1000);
                    var targets = Enumerable.Empty<StarSystemObject>()
                        .Concat(system.Planets)
                        .Concat(system.MinerMotherships)
                        .Concat(system.ScienceStations)
                        .Concat(system.RepairStations)
                        .Concat(system.FuelStation)
                        .Concat(system.TradeStations)
                        .Where(i => (rnd.Next() & 100) < 10)
                        .Select(o => (Object: o, Hex: o.Hex, Radius: system.GetObjectRadius(o)))
                        .Where(i => i.Radius < 1.8f)
                        .Where(i =>
                        {
                            var selfHex = i.Object.Hex;
                            var selfRadius = i.Radius;

                            return allObjects.All(o =>
                                o.Object == i.Object ||
                                o.Hex.GetDistanceTo(selfHex) >= (o.Radius + selfRadius));
                        })
                        .ToList();

                    Invoke(() =>
                    {
                        foreach (var item in targets)
                            SpawnBlockadeForObject(item.Object, faction, factionGroup);
                    });
                }
                catch { }
            });
        }

        public void SpawnBlockadeForObject(StarSystemObject targetObject, Faction blockadeFaction, int blockadeFactionGroup = -1)
        {
            if (targetObject is null)
                return;

            var map = Galaxy?.Map;
            var system = targetObject.System;

            if (map is null)
                return;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var hexesCompletionSource = new TaskCompletionSource<SystemHex[]>();

                    Galaxy.BeginPreUpdateAction(g =>
                    {
                        try
                        {
                            var allObjects = system.GetAllObjects(false).Select(o => o.Hex);
                            var hexes = targetObject.Hex
                                .GetRing(1)
                                .Where(h => h.GetSize() <= 16)
                                .Where(h => allObjects.All(o => o != h))
                                .ToArray();

                            hexesCompletionSource.TrySetResult(hexes);
                        }
                        catch
                        {
                            hexesCompletionSource.TrySetCanceled();
                        }
                    });

                    hexesCompletionSource.Task.Wait(TimeSpan.FromSeconds(2));

                    if (hexesCompletionSource.Task.IsCompletedSuccessfully == false)
                        return;

                    var hexes = hexesCompletionSource.Task.Result;
                    var rnd = new Random128((Realm?.Seed ?? 0) + targetObject.Id * 1000);
                    var mobType = blockadeFaction.ToBlockadeType();

                    if (hexes.Length < 1 || mobType is DynamicMobType.None)
                        return;

                    var mobs = new List<(SystemHex Hex, DynamicMob Mob)>();
                    var accessLvl = system.Info?.Level ?? 1;
                    var level = rnd.Next(
                                SfaDatabase.GetCircleMinLevel(accessLvl),
                                SfaDatabase.GetCircleMaxLevel(accessLvl) + 1);

                    foreach (var item in hexes)
                    {
                        var archetype = rnd.NextDouble() switch
                        {
                            < 0.5 => AIArchetype.Patroller,
                            _ => AIArchetype.Aggressor,
                        };

                        var mobInfo = new GalaxyPatrolMobGenerator(Realm)
                        {
                            Faction = blockadeFaction,
                            Archetype = archetype,
                            Level = level,
                        }.Build();

                        if (mobInfo is null)
                            continue;

                        mobInfo.InternalName = "vpatrol";

                        var mob = new DynamicMob()
                        {
                            Info = mobInfo,
                            Type = mobType,
                        };

                        if (AddDynamicMob(mob) == true)
                            mobs.Add((item, mob));
                    }

                    Galaxy.BeginPreUpdateAction(_ =>
                    {
                        foreach (var item in mobs)
                        {
                            var fleet = new DiscoveryAiFleet
                            {
                                FactionGroup = blockadeFactionGroup,
                                AgroVision = -100,
                                UseRespawn = true,
                                RespawnTimeout = 300f,
                            };

                            fleet.Init(item.Mob, new BlockadeAI()
                            {
                                TargetHex = item.Hex,
                            });
                            fleet.SetLocation(SystemHexMap.HexToSystemPoint(item.Hex), true);
                            system.AddFleet(fleet);
                        }
                    });
                }
                catch { }
            });
        }

        public void SpawnMainFactionPatrol(int systemId)
        {
            Galaxy.BeginPreUpdateAction(g =>
            {
                if (g.GetActiveSystem(systemId, true) is StarSystem system &&
                    g.Map is GalaxyMap map)
                {
                    var availableFactions = FactionExtension
                        .GetMainFactions()
                        .Where(f => GetDynamicMobStats(f.ToPatrolType()) <= 1000)
                        .ToList();

                    if (availableFactions.Count < 1)
                        return;

                    var rnd = new Random128();
                    var faction = availableFactions.ElementAtOrDefault(
                        rnd.Next(0, availableFactions.Count));
                    var mobType = faction.ToPatrolType();
                    var similar = map
                        .GetSystemsArround(system.Id, 2, false)
                        .Select(s => g.GetActiveSystem(s.Key.Id))
                        .Where(s =>
                        {
                            return s?.Info is GalaxyMapStarSystem info &&
                                   (info.Faction.IsMainFaction() == false || info.Faction == faction) &&
                                   s.Fleets.Any(f => f is DiscoveryAiFleet ai && ai.IsDynamicMob == true && ai.DynamicMobType == mobType);
                        })
                        .Count();

                    if (similar > 5)
                        return;

                    Task.Factory.StartNew(() =>
                    {
                        var accessLvl = system.Info?.Level ?? 1;
                        var archetype = rnd.NextDouble() switch
                        {
                            < 0.15 => AIArchetype.Miner,
                            < 0.30 => AIArchetype.Trader,
                            < 0.50 => AIArchetype.Scientist,
                            < 0.75 => AIArchetype.Patroller,
                            _ => AIArchetype.Aggressor,
                        };

                        var mobInfo = new GalaxyPatrolMobGenerator(Realm)
                        {
                            Faction = faction,
                            Archetype = archetype,
                            Level = rnd.Next(
                                SfaDatabase.GetCircleMinLevel(accessLvl),
                                SfaDatabase.GetCircleMaxLevel(accessLvl) + 1),
                        }.Build();

                        if (mobInfo is null)
                            return;

                        var mob = new DynamicMob()
                        {
                            Info = mobInfo,
                            Type = faction.ToPatrolType(),
                        };

                        if (AddDynamicMob(mob) == false)
                            return;

                        var fleet = new DiscoveryAiFleet
                        {
                            FactionGroup = 1,
                            AgroVision = -2,
                            UseRespawn = false,
                        };

                        fleet.Init(mob, new GalaxyPatrollingAI()
                        {
                            Archetype = archetype,
                        });

                        Galaxy.BeginPreUpdateAction(_ =>
                        {
                            var spawnHex = SystemHexMap.ArrayIndexToHex(rnd.Next(0, SystemHexMap.HexesCount));

                            fleet.SetLocation(SystemHexMap.HexToSystemPoint(
                                system.GetNearestSafeHex(fleet, spawnHex, false, true)), true);

                            system.AddFleet(fleet);
                        });
                    });
                };
            });
        }

        public void UseDynamicMobs(Action<DynamicMobDatabase> action)
        {
            lock (DynamicMobsLocker)
                action?.Invoke(DynamicMobs);
        }

        public bool AddDynamicMob(DynamicMob mob)
        {
            var result = false;
            UseDynamicMobs(dtb => result = dtb.Add(mob));
            return result;
        }

        public bool RemoveDynamicMob(int mobId)
        {
            var result = false;
            UseDynamicMobs(dtb => result = dtb.Remove(mobId));
            return result;
        }

        public DynamicMob GetDynamicMob(int mobId)
        {
            DynamicMob result = null;
            UseDynamicMobs(dtb => result = dtb.Get(mobId));
            return result;
        }

        public int GetDynamicMobStats(DynamicMobType mobType)
        {
            var result = 0;
            UseDynamicMobs(dtb => result = dtb.GetStats(mobType));
            return result;
        }

        public DiscoveryMobInfo GetMobInfo(int id)
        {
            if (id < -1)
                return GetDynamicMob(-id)?.Info;

            return Realm?.MobsDatabase?.GetMob(id);
        }

        public DiscoveryMobInfo GetMobInfo(string name)
        {
            return Realm?.MobsDatabase?.GetMob(name);
        }

        public DiscoveryMobInfo GetMobCache(int fleetId)
        {
            if (fleetId <= -1) fleetId = -fleetId;

            lock (FleetsCacheLocker)
            {
                var now = DateTime.UtcNow;

                if (FleetsCache.TryGetValue(fleetId, out var cache) == true)
                {
                    if ((now - cache.UpdateTime) > FleetCacheLifetime)
                    {
                        RemoveMobCache(fleetId);
                        return null;
                    }

                    return cache.Fleet;
                }

                return null;
            }
        }

        public void AddMobCache(DiscoveryMobInfo mobInfo)
        {
            if (mobInfo is null)
                return;

            lock (FleetsCacheLocker)
            {
                var fleetId = mobInfo.Id < -1 ? -mobInfo.Id : mobInfo.Id;
                FleetsCache[fleetId] = (mobInfo, DateTime.UtcNow);
            }
        }

        public bool RemoveMobCache(int fleetId)
        {
            if (fleetId <= -1) fleetId = -fleetId;

            lock (FleetsCacheLocker)
            {
                return FleetsCache.Remove(fleetId);
            }
        }
    }
}
