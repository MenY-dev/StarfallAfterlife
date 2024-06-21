using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.SfPackageLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class GalaxyPatrollingAI : FleetAI
    {
        public AIArchetype Archetype { get; set; }

        public TimeSpan SystemChangePeriod { get; set; } = TimeSpan.FromMinutes(2);

        public TimeSpan WaitingTime { get; set; } = TimeSpan.FromSeconds(3);

        public TimeSpan DockingTime { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan AttackTime { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan AttackCooldown { get; set; } = TimeSpan.FromSeconds(3);

        public float AttackChance { get; set; } = 0.333f;

        public int TargetLostDistance { get; set; } = 5;

        public StarSystemObject Target { get; protected set; }

        public DateTime AttackEndTime { get; protected set; }

        private readonly Random128 _rnd = new();

        public AIStateMachine StateMachine { get; protected set; }

        public AIState DefaultState { get; protected set; }

        public AIState CurrentState => StateMachine?.CurrentState;

        public override void Update()
        {
            if (IsConnected == true &&
                CurrentAction is not AIStateMachine or not { State: AINodeState.Started })
            {
                StartAction(StateMachine = CreateBehavior());
            }

            base.Update();
        }

        protected virtual AIStateMachine CreateBehavior()
        {
            var sm = new AIStateMachine();

            sm.States.Add(DefaultState = new()
            {
                Name = "routine",
                Default = true,
                Looped = true,
                Action = (AIState s) => CreateRoutineAction(),
            });
            
            sm.States.Add(new()
            {
                Name = "idle",
                Looped = true,
            });

            sm.States.Add(new()
            {
                Name = "attack_target",
                Action = (AIState s) => new AttackAction(
                    s.Context as StarSystemObject, AttackTime, TargetLostDistance),
                OnEnd = (s, a) => AttackEndTime = DateTime.UtcNow,
            });

            sm.States.Add(new()
            {
                Name = "change_system",
                Action = (AIState s) => new MoveToSystemAction(s.Context as int? ?? -1)
            });

            if (Archetype is AIArchetype.Patroller or AIArchetype.Aggressor)
            {
                sm.Watchdogs.Add(new()
                {
                    Name = "find_enemy_watchdog",
                    Action = FindEnemyWatchdog,
                });
            }

            if (SystemChangePeriod != default)
            {
                sm.Watchdogs.Add(new()
                {
                    Name = "change_system_watchdog",
                    Action = ChangeSystemWatchdog,
                    Period = SystemChangePeriod,
                });
            }

            AttackEndTime = DateTime.UtcNow + AttackCooldown;

            return sm;
        }

        public IAINode CreateRoutineAction()
        {
            IAINode action;

            if (_rnd.Next(0, 3) == 0)
            {
                action = _rnd.NextSingle() switch
                {
                    < 0.2f => CreateDockAction(),
                    < 0.4f => CreateTradeAction(),
                    < 0.6f => CreateExploreAction(),
                    _ => CreatePatrollingAction(),
                };
            }
            else
            {
                action = Archetype switch
                {
                    AIArchetype.Patroller or
                    AIArchetype.Aggressor => CreatePatrollingAction(),
                    AIArchetype.Scientist => CreateExploreAction(),
                    AIArchetype.Miner => CreateMineAction(),
                    AIArchetype.Trader => CreateTradeAction(),
                    _ => null,
                };
            }

            if (Fleet is not null and { DockObjectId: not -1 })
            {
                action = new AIActionQueue
                {
                    CompletionHandling = QueueCompletionHandling.All,
                    Name = "undock_and_action",
                    Queue = { new UndockAction(), action }
                };
            }

            return action;
        }

        public IAINode CreatePatrollingAction()
        {
            var waypoint = SystemHexMap.ArrayIndexToHex(
                _rnd.Next(0, SystemHexMap.HexesCount));

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;

            var action = new AIActionQueue
            {
                CompletionHandling = QueueCompletionHandling.All,
                Name = "patrolling",
                Queue =
                {
                    new MoveToPointAction(SystemHexMap.HexToSystemPoint(waypoint)),
                }
            };

            if (_rnd.Next(3) != 0)
                action.Queue.Add(new WaitAction(WaitingTime));

            return action;
        }

        public IAINode CreateExploreAction()
        {
            var objects = System?.GetAllObjects(false).ToArray();

            SystemHex waypoint;

            if (objects is not null and { Length: > 0 })
            {
                var target = objects[_rnd.Next(0, objects.Length)];
                var hexes = target.Hex.GetRingEnumerator(1).Where(h => h.GetSize() < 17).ToArray();
                waypoint = hexes[_rnd.Next(0, hexes.Length)];
            }
            else
            {
                waypoint = SystemHexMap.ArrayIndexToHex(_rnd.Next(0, SystemHexMap.HexesCount));
            }

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;

            return new AIActionQueue
            {
                CompletionHandling = QueueCompletionHandling.All,
                Name = "move_and_scan",
                Queue =
                {
                    new MoveToPointAction(SystemHexMap.HexToSystemPoint(waypoint)),
                    new ScanAction(TimeSpan.FromSeconds(10))
                }
            };
        }

        public IAINode CreateMineAction()
        {
            var hexes = new List<SystemHex>();
            var map = System?.AsteroidsMap;

            if (map is not null and { Filling: > 0 })
            {
                for (int i = 0; i < SystemHexMap.HexesCount; i++)
                    if (map[i] == true)
                        hexes.Add(SystemHexMap.ArrayIndexToHex(i));
            }

            var waypoint = hexes.Count > 0 ?
                hexes[_rnd.Next(0, hexes.Count)] :
                SystemHexMap.ArrayIndexToHex(_rnd.Next(0, SystemHexMap.HexesCount));

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;

            return new AIActionQueue
            {
                CompletionHandling = QueueCompletionHandling.All,
                Name = "move_and_mine",
                Queue =
                {
                    new MoveToPointAction(SystemHexMap.HexToSystemPoint(waypoint)),
                    new MineAction(TimeSpan.FromSeconds(12))
                }
            };
        }

        public IAINode CreateTradeAction()
        {
            var objects = System?.GetAllObjects(false).ToArray();

            SystemHex waypoint;

            if (objects is not null and { Length: > 0 })
            {
                var target = objects[_rnd.Next(0, objects.Length)];
                var hexes = target.Hex.GetRingEnumerator(1).Where(h => h.GetSize() < 17).ToArray();
                waypoint = hexes[_rnd.Next(0, hexes.Length)];
            }
            else
            {
                waypoint = SystemHexMap.ArrayIndexToHex(_rnd.Next(0, SystemHexMap.HexesCount));
            }

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;

            return new AIActionQueue
            {
                CompletionHandling = QueueCompletionHandling.All,
                Name = "move_and_trade",
                Queue =
                {
                    new MoveToPointAction(SystemHexMap.HexToSystemPoint(waypoint)),
                    new WaitAction(WaitingTime)
                }
            };
        }

        public IAINode CreateDockAction()
        {
            var system = System;
            var fleet = Fleet;

            if (system is null || fleet is null)
                return default;

            bool isMainFaction = Fleet?.Faction.IsMainFaction() ?? false;
            var objectsEnum = Enumerable.Empty<StarSystemObject>()
                .Concat(system.Planets)
                .Concat(system.MinerMotherships)
                .Concat(system.ScienceStations)
                .Concat(system.RepairStations)
                .Concat(system.FuelStation)
                .Concat(system.TradeStations)
                .Concat(system.Motherships)
                .Concat(system.QuickTravelGates);

            if (fleet.Faction.IsMainFaction() == true)
            {
                var fleetFaction = fleet.Faction;
                objectsEnum = objectsEnum.Where(
                    o => o.Faction.IsMainFaction() == false || o.Faction == fleetFaction);
            }

            var objects = objectsEnum.ToArray();

            if (objects.Length == 0)
                return default;

            var target = objects[_rnd.Next(0, objects.Length)];
            var hexes = target.Hex.GetRingEnumerator(1).Where(h => h.GetSize() < 17).ToArray();
            SystemHex waypoint = system.GetNearestSafeHex(Fleet, hexes[_rnd.Next(0, hexes.Length)], false, false);

            return new AIActionQueue
            {
                CompletionHandling = QueueCompletionHandling.All,
                Name = "move_and_dock",
                Queue =
                {
                    new MoveToPointAction(SystemHexMap.HexToSystemPoint(waypoint)),
                    new DockAction(target.Type, target.Id),
                    new WaitAction(DockingTime),
                    new UndockAction(),
                }
            };
        }

        protected void FindEnemyWatchdog(AIWatchdog watchdog)
        {
            var fleet = Fleet;

            if (IsConnected == false ||
                fleet is null ||
                fleet.State != FleetState.InGalaxy ||
                fleet.DockObjectId != -1 ||
                fleet.Immortal == true ||
                CurrentState != DefaultState ||
                fleet.GetBattle() is not null)
                return;

            var time = DateTime.UtcNow;
            var faction = fleet.Faction;

            if (fleet.AttackTarget is null && (time - AttackEndTime) > AttackCooldown)
            {
                bool IsJoiningAvailable(DiscoveryFleet fleet) =>
                    fleet.GetBattle() is not StarSystemBattle battle ||
                    (battle.IsDungeon == false && battle.Members.Count(m => m.Fleet is DiscoveryAiFleet) < 2);

                var enemies = fleet.System?.Fleets.Where(enemy =>
                    enemy != fleet &&
                    enemy.State == FleetState.InGalaxy &&
                    enemy.Faction.IsEnemy(faction, true) &&
                    Math.Abs(enemy.Level - fleet.Level) < 7 &&
                    IsJoiningAvailable(enemy) == true &&
                    fleet.CanAttack(enemy) &&
                    fleet.IsVisible(enemy, Archetype is not AIArchetype.Aggressor));

                if ((time - AttackEndTime) > AttackCooldown &&
                    enemies.Any() == true)
                {
                    if (enemies.FirstOrDefault(e => _rnd.NextSingle() < AttackChance) is DiscoveryFleet enemy)
                    {
                        StateMachine.StartStateByName("attack_target", enemy);
                    }
                    else
                    {
                        AttackEndTime = time;
                    }
                }
            }
        }

        protected void ChangeSystemWatchdog(AIWatchdog watchdog)
        {
            var fleet = Fleet;

            if (SystemChangePeriod == default ||
                IsConnected == false ||
                fleet is null ||
                fleet.State != FleetState.InGalaxy ||
                CurrentState != DefaultState ||
                fleet.GetBattle() is not null)
                return;

            if (fleet.System is StarSystem system &&
                system.Galaxy is DiscoveryGalaxy galaxy &&
                galaxy.Map is GalaxyMap map)
            {
                var accesLevel = SfaDatabase.LevelToAccessLevel(fleet.Level);
                var activeSystems = map
                    .GetSystemsArround(system.Id, 3, false)
                    .Where(s => s.Key.Level <= accesLevel)
                    .Select(s => galaxy.GetActiveSystem(s.Key.Id))
                    .Where(s => s is not null)
                    .ToArray();

                var targetSystem = activeSystems.ElementAtOrDefault(
                    _rnd.Next(0, activeSystems.Length));

                if (targetSystem is not null)
                    StateMachine.StartStateByName("change_system", targetSystem.Id);
            }
        }

        protected override void OnActionFinished(IAINode action)
        {
            base.OnActionFinished(action);
            CurrentAction = null;
        }
    }
}
