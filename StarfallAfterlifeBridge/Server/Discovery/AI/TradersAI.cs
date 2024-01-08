using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class TradersAI : FleetAI
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
                    Name = "move_ant_trade",
                    Queue =
                    {
                        new MoveToPointAction(CreateNextWaypoint()),
                        new WaitAction(TimeSpan.FromSeconds(WaitingTime))
                    }
                });
            }

            base.Update();
        }

        protected Vector2 CreateNextWaypoint()
        {
            var rnd = new Random128();
            var objects = System?.GetAllObjects(false).ToArray();

            SystemHex waypoint;

            if (objects is not null and { Length: > 0 })
            {
                var target = objects[rnd.Next(0, objects.Length)];
                var hexes = target.Hex.GetRingEnumerator(1).Where(h => h.GetSize() < 17).ToArray();
                waypoint = hexes[rnd.Next(0, hexes.Length)];
            }
            else
            {
                waypoint = SystemHexMap.ArrayIndexToHex(rnd.Next(0, SystemHexMap.HexesCount));
            }

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;
            return SystemHexMap.HexToSystemPoint(waypoint);
        }
    }
}
