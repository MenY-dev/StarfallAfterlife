using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class MinersAI : FleetAI
    {
        public float WaitingTime { get; set; } = 10;

        public override void Update()
        {
            if (IsConnected == false)
                return;

            if (CurrentAction is null or not { State: AIActionState.Started })
            {
                StartAction(new AIActionQueue
                {
                    CompletionHandling = QueueCompletionHandling.All,
                    Name = "mine",
                    Queue =
                    {
                        new MoveToPointAction(CreateNextWaypoint()),
                        new MineAction(TimeSpan.FromSeconds(WaitingTime))
                    }
                });
            }

            base.Update();
        }

        protected Vector2 CreateNextWaypoint()
        {
            var rnd = new Random();
            var hexes = new List<SystemHex>();
            var map = System?.AsteroidsMap;

            if (map is not null and { Filling: > 0 })
            {
                for (int i = 0; i < SystemHexMap.HexesCount; i++)
                    if (map[i] == true)
                        hexes.Add(SystemHexMap.ArrayIndexToHex(i));
            }

            var waypoint = hexes.Count > 0 ?
                hexes[rnd.Next(0, hexes.Count)] :
                SystemHexMap.ArrayIndexToHex(rnd.Next(0, SystemHexMap.HexesCount));

            waypoint = System?.GetNearestSafeHex(Fleet, waypoint, false) ?? waypoint;
            return SystemHexMap.HexToSystemPoint(waypoint);
        }
    }
}
