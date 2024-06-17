using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    internal class GalaxyPatrolMobGenerator : GenerationTask
    {
        public SfaRealm Realm { get; set; }

        public Faction Faction { get; set; }

        public AIArchetype Archetype { get; set; }

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
            var rnd = new Random128();
            var serviceFleet = Realm?.MobsDatabase?.GetServiceFleet(Faction);

            if (serviceFleet is null)
                return null;

            var accessLvl = SfaDatabase.LevelToAccessLevel(Level);
            var localLevel =  Math.Max(1, Level - SfaDatabase.GetCircleMinLevel(accessLvl) + 1);
            var minShipLvl = Math.Max(0, accessLvl - localLevel switch { < 5 => 2, < 10 => 1, _ => 0, });
            var minShipCount = accessLvl switch { < 3 => 2, < 5 => 3, _ => 4, };
            var maxShipCount = accessLvl switch { < 3 => 4, < 5 => 5, _ => 6, };
            var targetShipCount = (int)SfMath.Lerp(minShipCount, maxShipCount, Math.Max(1, localLevel / 14f));
            var roles = GetArchetypeRoles(Archetype);
            var availableShips = serviceFleet?.Ships
                .Where(s => s?.ServiceData is not null &&
                            s.ServiceData.FleetMin <= accessLvl &&
                            s.ServiceData.FleetMax >= minShipLvl)
                .GroupBy(s => roles.GetValueOrDefault((ShipRole)s.ServiceData.Role, int.MaxValue))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var roleShips = g.ToList();
                    roleShips.Randomize(rnd.Next());
                    return roleShips;
                })
                .ToList();

            if (availableShips.Count < 1)
                return null;

            var ships = new List<DiscoveryMobShipData>();

            foreach (var roleShips in availableShips)
            {
                var remainingCount = Math.Max(0, targetShipCount - ships.Count);
                var requiredCount = Math.Max(1, rnd.Next(remainingCount / 2, remainingCount + 1));

                foreach (var ship in roleShips)
                {
                    var count = Math.Max(1, rnd.Next(requiredCount + 1));

                    for (int i = 0; i < count; i++)
                    {
                        ships.Add(ship.Clone());
                        requiredCount--;
                    }

                    if (ships.Count >= targetShipCount)
                        break;
                }
            }

            if (ships.Count < targetShipCount)
            {
                var allShips = availableShips.SelectMany(g => g).ToList();

                while (ships.Count < targetShipCount)
                {
                    var ship = allShips.ElementAtOrDefault(
                        rnd.Next(availableShips.Count / 3));

                    if (ship is null)
                        break;

                    ships.Add(ship.Clone());
                }
            }

            if (ships.Count < 1)
                return null;

            return new DiscoveryMobInfo
            {
                Faction = Faction,
                InternalName = Archetype switch
                {
                    AIArchetype.Scientist => "scientists",
                    AIArchetype.Trader => "strangersfleet",
                    AIArchetype.Miner => "miningfleet",
                    AIArchetype.Aggressor => "warfleet",
                    _ => Faction switch
                    {
                        Faction.Deprived => "dpatrol",
                        Faction.Eclipse => "epatrol",
                        Faction.Vanguard => "vpatrol",
                        _ => "dpatrol",
                    },
                },
                Level = Level,
                MainShipIndex = 0,
                Ships = ships,
            };
        }

        protected Dictionary<ShipRole, int> GetArchetypeRoles(AIArchetype archetype)
        {
            List<ShipRole> roles = archetype switch
            {
                AIArchetype.Trader or
                AIArchetype.Miner => new()
                {
                    ShipRole.Freighter,
                    ShipRole.Support,
                    ShipRole.Battleship,
                    ShipRole.Assault,
                    ShipRole.Sniper,
                    ShipRole.Fighter,
                    ShipRole.Scout,
                    ShipRole.SpecOps,
                    ShipRole.None,
                },
                AIArchetype.Scientist or
                AIArchetype.Trader => new()
                {
                    ShipRole.Freighter,
                    ShipRole.Sniper,
                    ShipRole.Scout,
                    ShipRole.Support,
                    ShipRole.Battleship,
                    ShipRole.Fighter,
                    ShipRole.SpecOps,
                    ShipRole.Assault,
                    ShipRole.None,
                },
                _ => new()
                {
                    ShipRole.Battleship,
                    ShipRole.Fighter,
                    ShipRole.Sniper,
                    ShipRole.Scout,
                    ShipRole.Support,
                    ShipRole.Assault,
                    ShipRole.SpecOps,
                    ShipRole.Freighter,
                    ShipRole.None,
                }
            };

            return new(roles.Select((r, i) => KeyValuePair.Create(r, i)));
        }
    }
}
