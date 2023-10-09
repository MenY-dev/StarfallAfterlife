using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class MobAI : FleetAI
    {
        public float WaitingTime { get; set; } = 3;

        public float AttackTime { get; set; } = 10;

        public float AttackCooldown { get; set; } = 3;

        public float AttackChance { get; set; } = 0.3334f;

        public int TargetLostDistance { get; set; } = 5;

        public DateTime WaitingStartTime { get; protected set; }

        public DateTime AttackStartTime { get; protected set; }

        public DateTime AttackEndTime { get; protected set; }

        public bool IsInWaiting { get; protected set; } = false;

        private readonly Random _rnd = new();

        public override void Update()
        {
            base.Update();

            var fleet = Fleet;

            if (IsConnected == false ||
                fleet is null ||
                fleet.State != FleetState.InGalaxy)
                return;

            var time = DateTime.Now;
            var faction = fleet.Faction;
            var targetFleet = fleet.AttackTarget as DiscoveryFleet;

            if (targetFleet is null)
            {
                var enemies = fleet.System?.Fleets.Where(f =>
                    f != fleet &&
                    f.State == FleetState.InGalaxy &&
                    f.Faction.IsEnemy(faction) &&
                    fleet.CanAttack(f) &&
                    fleet.IsVisible(f));

                if ((time - AttackEndTime).TotalSeconds > AttackCooldown &&
                    enemies.Any() == true)
                {
                    if (enemies.FirstOrDefault(e => _rnd.NextSingle() < AttackChance) is DiscoveryFleet enemy)
                    {
                        Attack(enemy);
                    }
                    else
                    {
                        AttackEndTime = time;
                    }
                }
            }
            else if(fleet.Hex.GetDistanceTo(targetFleet.Hex) >= TargetLostDistance||
                    fleet.CanAttack(targetFleet) == false ||
                    (time - AttackStartTime).TotalSeconds > AttackTime)
            {
                AttackEndTime = time;
                MoveTo(CreateRandowWaypoint());
            }

            targetFleet = fleet.AttackTarget as DiscoveryFleet;

            if (targetFleet is null)
            {
                if (IsInWaiting == true)
                {
                    if ((time - WaitingStartTime).TotalSeconds > WaitingTime)
                        MoveTo(CreateRandowWaypoint());
                }
                else if (fleet.IsTargetLocationReached)
                {
                    Wait();
                }
            }
        }

        public virtual void Wait()
        {
            if (Fleet is DiscoveryFleet fleet)
            {
                IsInWaiting = true;
                WaitingStartTime = DateTime.Now;
                fleet?.Stop();
            }
        }

        public virtual void MoveTo(Vector2 location)
        {
            if (Fleet is DiscoveryFleet fleet)
            {
                IsInWaiting = false;
                fleet?.MoveTo(location);
            }

        }

        public virtual void Attack(DiscoveryFleet target)
        {
            if (Fleet is DiscoveryFleet fleet)
            {
                AttackStartTime = DateTime.Now;
                fleet.SetAttackTarget(target);
            }
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
