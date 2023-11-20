using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class NeutralFleetAI : FleetAI
    {
        public float WaitingTime { get; set; } = 3;

        public override void Update()
        {
            if (IsConnected == false)
                return;

            if (CurrentAction is null or not { State: AIActionState.Started })
            {
                StartAction(new AIActionQueue
                {
                    CompletionHandling = QueueCompletionHandling.All,
                    Name = "move_and_wait",
                    Queue =
                    {
                        new MoveToPointAction(CreateRandowWaypoint()),
                        new WaitAction(TimeSpan.FromSeconds(WaitingTime))
                    }
                });
            }

            base.Update();
        }

        protected Vector2 CreateRandowWaypoint()
        {
            var rnd = new Random();
            var waypoint = SystemHexMap.ArrayIndexToHex(
                rnd.Next(0, SystemHexMap.HexesCount));

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;
            return SystemHexMap.HexToSystemPoint(waypoint);
        }
    }
}
