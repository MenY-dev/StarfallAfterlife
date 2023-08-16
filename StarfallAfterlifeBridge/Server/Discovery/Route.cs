using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class Route
    {
        public List<Vector2> Path { get; } = new();

        public Vector2 Location { get; protected set; }

        public Vector2 CurrentDirection { get; protected set; }

        public Vector2 TargetLocation { get; protected set; }

        public Vector2 TargetWaypointLocation { get; protected set; }

        public int CurrentWaypoint { get; protected set; } = 0;

        public int WaypointCount { get; protected set; } = 0;

        public void Update(Vector2 location)
        {
            Path.Clear();
            Path.Add(location);
            Location = location;
            TargetLocation = location;
            TargetWaypointLocation = location;
            CurrentWaypoint = 1;
            WaypointCount = 1;
        }

        public void Update(Vector2 from, Vector2 to)
        {
            Path.Clear();
            Path.Add(from);
            Path.Add(to);
            Location = from;
            TargetLocation = to;
            TargetWaypointLocation = to;
            CurrentWaypoint = 1;
            WaypointCount = 2;
        }

        public void Update(IEnumerable<Vector2> path)
        {
            Path.Clear();
            Path.AddRange(path);
            Location = Path.FirstOrDefault();
            TargetLocation = Path.LastOrDefault();
            CurrentWaypoint = 1;
            WaypointCount = Path.Count;
            TargetWaypointLocation = Path.Count > 1 ? Path[1] : Location;
        }

        public RouteMoveResult Move(float distance)
        {
            var result = RouteMoveResult.None;
            WaypointCount = Path.Count;

            if (CurrentWaypoint >= WaypointCount)
                return result;

            do
            {
                TargetWaypointLocation = Path[CurrentWaypoint];
                var delta = TargetWaypointLocation - Location;
                var distanceToNext = delta.GetSize();

                if (distanceToNext > distance)
                {
                    CurrentDirection = delta.Normalize() * distance;
                    Location += CurrentDirection;
                    return result;
                }

                distance -= distanceToNext;
                Location = Path[CurrentWaypoint];
                CurrentWaypoint++;
                result = RouteMoveResult.WaypointReached;
            }
            while (CurrentWaypoint < WaypointCount);

            Location = Path.LastOrDefault();
            result = RouteMoveResult.TargetLocationReached;
            return result;
        }
    }
}
