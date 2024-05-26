using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class PatrollingAI : FleetAI
    {
        public float WaitingTime { get; set; } = 3;

        public float AttackTime { get; set; } = 10;

        public float AttackCooldown { get; set; } = 3;

        public float AttackChance { get; set; } = 0.3334f;

        public int TargetLostDistance { get; set; } = 5;

        public DateTime AttackEndTime { get; protected set; }

        private readonly Random128 _rnd = new();

        public override void Update()
        {
            var fleet = Fleet;

            if (IsConnected == false ||
                fleet is null ||
                fleet.State != FleetState.InGalaxy ||
                fleet.GetBattle() is not null)
                return;

            var time = DateTime.UtcNow;
            var faction = fleet.Faction;
            var targetFleet = fleet.AttackTarget as DiscoveryFleet;

            if (targetFleet is null)
            {
                bool IsJoiningAvailable(DiscoveryFleet fleet) =>
                    fleet.GetBattle() is not StarSystemBattle battle ||
                    (battle.IsDungeon == false && battle.Members.Count(m => m.Fleet is DiscoveryAiFleet) < 1);

                var enemies = fleet.System?.Fleets.Where(enemy =>
                    enemy != fleet &&
                    enemy.State == FleetState.InGalaxy &&
                    enemy.Faction.IsEnemy(faction, false) &&
                    IsJoiningAvailable(enemy) == true &&
                    fleet.CanAttack(enemy) &&
                    fleet.IsVisible(enemy));

                if ((time - AttackEndTime).TotalSeconds > AttackCooldown &&
                    enemies.Any() == true)
                {
                    if (enemies.FirstOrDefault(e => _rnd.NextSingle() < AttackChance) is DiscoveryFleet enemy)
                    {
                        StartAction(new AttackAction(
                            enemy,
                            TimeSpan.FromSeconds(AttackTime),
                            TargetLostDistance));
                    }
                    else
                    {
                        AttackEndTime = time;
                    }
                }
            }

            targetFleet = fleet.AttackTarget as DiscoveryFleet;

            if (targetFleet is null &&
                CurrentAction is null or not { State: AINodeState.Started })
            {
                StartAction(new AIActionQueue
                {
                    CompletionHandling = QueueCompletionHandling.All,
                    Name = "patrolling",
                    Queue =
                    {
                        new MoveToPointAction(CreateRandowWaypoint()),
                        new WaitAction(TimeSpan.FromSeconds(WaitingTime))
                    }
                });
            }

            base.Update();
        }

        protected override void OnActionFinished(IAINode action)
        {
            base.OnActionFinished(action);

            if (action is AttackAction)
                AttackEndTime = DateTime.UtcNow;
        }

        protected Vector2 CreateRandowWaypoint()
        {
            var waypoint = SystemHexMap.ArrayIndexToHex(
                _rnd.Next(0, SystemHexMap.HexesCount));

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;
            return SystemHexMap.HexToSystemPoint(waypoint);
        }
    }
}
