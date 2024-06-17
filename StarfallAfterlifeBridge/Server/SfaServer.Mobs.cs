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

        protected object DynamicMobsLocker { get; } = new();
        
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
                            < 0.25 => AIArchetype.Miner,
                            < 0.5 => AIArchetype.Trader,
                            < 0.75 => AIArchetype.Scientist,
                            < 0.90 => AIArchetype.Patroller,
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
                            AgroVision = -3,
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
    }
}
