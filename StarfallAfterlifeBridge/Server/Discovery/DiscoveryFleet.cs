using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDebug = System.Diagnostics.Debug;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public abstract partial class DiscoveryFleet : DockableObject
    {
        public int Hull { get; set; } = -1;

        public int MobId { get; internal set; } = -1;

        public string Name { get; set; }

        public float Speed { get; set; } = 8;

        public int Level { get; set; } = 0;

        public int Vision { get; set; } = 5;

        public int NebulaVision { get; set; } = 3;

        public FleetState State { get; set; } = FleetState.WaitingGalaxy;

        //public bool InBattle { get; set; } = false;

        public int DockObjectId { get; set; } = -1;

        public DiscoveryObjectType DockObjectType { get; set; } = DiscoveryObjectType.None;

        public override DiscoveryObjectType Type => base.Type = DiscoveryObjectType.AiFleet;

        public Route Route { get; } = new();

        public Vector2 TargetLocation => Route.TargetLocation;

        public StarSystemObject AttackTarget { get; protected set; }

        public FleetAI AI { get; protected set; }

        public event EventHandler<EventArgs> TargetLocationReached;

        protected DateTime LastUpdateTime { get; set; }

        protected float DeltaTime { get; set; }

        public virtual void Stop()
        {
            AttackTarget = null;
            SetTargetLocation(Location);
        }

        public virtual void SetAttackTarget(StarSystemObject target)
        {
            if (target is DiscoveryFleet fleet)
                SetTargetFleet(fleet);
            else if (target is PiratesStation or PiratesOutpost)
            {
                AttackTarget = target;
            }
            else if (target is not null)
                MoveTo(target.Location);
        }

        public void MoveTo(SystemHex hex)
        {
            SetTargetFleet(null);
            SetTargetLocation(SystemHexMap.HexToSystemPoint(hex));
        }

        public void MoveTo(Vector2 location)
        {
            SetTargetFleet(null);
            SetTargetLocation(location);
        }

        public void SetLocation(Vector2 location)
        {
            AttackTarget = null;
            Route.Update(location);
            Location = location;
            Broadcast<IFleetListener>(l => l.OnFleetMoved(this));
            Broadcast<IFleetListener>(l => l.OnFleetRouteChanged(this));
        }

        public void SetFleetState(FleetState state)
        {
            State = state;
            Broadcast<IFleetListener>(l => l.OnFleetDataChanged(this));

            if (state != FleetState.InGalaxy)
                Stop();
        }

        protected virtual void SetTargetLocation(Vector2 location)
        {
            if (Location != location)
            {
                Route.Update(CreateRoute(location));
                Broadcast<IFleetListener>(l => l.OnFleetMoved(this));
                Broadcast<IFleetListener>(l => l.OnFleetRouteChanged(this));
            }
            else
            {
                OnTargetLocationReached();
            }
        }

        protected virtual void SetTargetFleet(DiscoveryFleet target)
        {
            AttackTarget = target;
        }

        protected virtual IEnumerable<Vector2> CreateRoute(Vector2 target)
        {
            var map = System?.NavigationMap;

            if (map is null)
            {
                yield return Location;
                yield return target;
            }
            else
            {
                foreach (var point in map.CalculatePath(Location, target))
                    yield return point;
            }
        }

        public virtual void Update()
        {
            DeltaTime = (float)(DateTime.Now - LastUpdateTime).TotalSeconds;
            LastUpdateTime = DateTime.Now;

            if (State == FleetState.InGalaxy)
            {
                UpdateRouteToAttackTarget();
                TickActions();
                UpdateLocation();
                AI?.Update();
            }
        }

        protected virtual void UpdateLocation()
        {
            var translation = Speed * DeltaTime;
            var moveResult = Route.Move(translation);

            if (moveResult != RouteMoveResult.None)
            {

                if (moveResult == RouteMoveResult.TargetLocationReached)
                    OnTargetLocationReached();

                Broadcast<IFleetListener>(l => l.OnFleetMoved(this));
                Broadcast<IFleetListener>(l => l.OnFleetRouteChanged(this));
            }

            if (Location != Route.Location)
                Location = Route.Location;
        }

        protected virtual void UpdateRouteToAttackTarget()
        {
            if (AttackTarget is null ||
                AttackTarget.System != System)
                return;

            if (TargetLocation.GetDistanceTo(AttackTarget.Location) > 0.8f)
                SetTargetLocation(AttackTarget.Location);

            if (Location.GetDistanceTo(AttackTarget.Location) < 0.8f)
            {
                OnAttackTargetObjectReached(AttackTarget);
                AttackTarget = null;
            }
        }

        protected virtual void OnAttackTargetObjectReached(StarSystemObject target)
        {
            if (target is null || target.System != System)
                return;

            if (System?.GetBattle(target) is StarSystemBattle currentBattle)
            {
                currentBattle.AddToBattle(this, BattleRole.Join, CreateHexOffset());
                return;
            }

            if (target is DiscoveryFleet targetFleet)
            {
                var battle = new StarSystemBattle
                {
                    Hex = Hex,
                };

                System?.AddBattle(battle);

                battle.AddToBattle(this, BattleRole.Attack, CreateHexOffset());
                battle.AddToBattle(targetFleet, BattleRole.Defense, targetFleet.CreateHexOffset());
                battle.HasNebula = System?.NebulaMap?[battle.Hex] == true;
                battle.HasAsteroids = System?.AsteroidsMap?[battle.Hex] == true;
                battle.RichAsteroidField = System?.GetObjectAt(battle.Hex, DiscoveryObjectType.RichAsteroids) as StarSystemRichAsteroid;
                battle.Init();
                battle.Start();
            }
            else if (target is PiratesStation or PiratesOutpost)
            {
                var battle = new StarSystemBattle
                {
                    Hex = Hex,
                    IsDungeon = true,
                    DungeonInfo = new()
                    {
                        Target = target
                    }
                };

                System?.AddBattle(battle);

                battle.AddToBattle(this, BattleRole.Attack, CreateHexOffset());
                battle.Init();
                battle.Start();
            }
        }

        public virtual void ExploreCurrentHex()
        {
            var battle = new StarSystemBattle
            {
                Hex = Hex,
            };

            System?.AddBattle(battle);

            battle.AddToBattle(this, BattleRole.Explore, CreateHexOffset());
            battle.HasNebula = System?.NebulaMap?[battle.Hex] == true;
            battle.HasAsteroids = System?.AsteroidsMap?[battle.Hex] == true;
            battle.RichAsteroidField = System?.GetObjectAt(battle.Hex, DiscoveryObjectType.RichAsteroids) as StarSystemRichAsteroid;
            battle.Init();
            battle.Start();
        }

        public void SetAI(FleetAI ai)
        {
            AI?.Disconnect();
            ai?.Connect(this);
            AI = ai;
        }

        public virtual Vector2 CreateHexOffset()
        {
            if (Route is null || Route.CurrentWaypoint >= Route.WaypointCount)
                return Vector2.Zero;

            var offset = Route.CurrentDirection;

            if (offset != Vector2.Zero)
                offset = offset.Normalize();

            return offset.GetNegative();
        }

        protected virtual void OnTargetLocationReached()
        {
            TargetLocationReached?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnSystemChanged(StarSystem system)
        {
            LastUpdateTime = DateTime.Now;
            base.OnSystemChanged(system);
        }
    }
}