using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyExtraMap
    {
        public GalaxyMap GalaxyMap { get; set; }

        public Dictionary<int, GalaxyCircle> Circles { get; } = new();

        public GalaxyMapStarSystem DeprivedStartSystem { get; set; }

        public GalaxyMapStarSystem EclipseStartSystem { get; set; }

        public GalaxyMapStarSystem VanguardStartSystem { get; set; }

        public int MaxFactionGroup = -1;

        public GalaxyExtraMap(GalaxyMap galaxyMap)
        {
            GalaxyMap = galaxyMap;
        }

        public virtual void Build()
        {
            var systems = GalaxyMap.Systems;

            if (systems is null)
                return;

            Circles.Clear();

            foreach (var system in systems)
            {
                if (system is null)
                    continue;

                AddToCircles(system);
                UpdateFactionInfo(system);
                MaxFactionGroup = Math.Max(system.FactionGroup, MaxFactionGroup);
            }

            UpdateCirclesStartSystems();

            foreach (var circle in Circles.Values)
                UpdateCircleMeasures(circle);
        }

        protected virtual void AddToCircles(GalaxyMapStarSystem newSystem)
        {
            if (newSystem is null)
                return;

            var circleId = newSystem.Level;

            if (Circles.TryGetValue(circleId, out GalaxyCircle circle) == false)
            {
                circle = new GalaxyCircle(circleId);
                Circles.Add(circleId, circle);
                circle.AddSystem(newSystem);
            }
            else
            {
                circle.AddSystem(newSystem);
            }
        }

        protected virtual void UpdateFactionInfo(GalaxyMapStarSystem newSystem)
        {
            if (newSystem is null)
                return;

            switch ((Faction)newSystem.Faction)
            {
                case Faction.Deprived: DeprivedStartSystem ??= newSystem; break;
                case Faction.Eclipse: EclipseStartSystem ??= newSystem; break;
                case Faction.Vanguard: VanguardStartSystem ??= newSystem; break;
            }
        }

        protected virtual void UpdateCirclesStartSystems()
        {
            if (DeprivedStartSystem?.Location is Vector2 deprivedLocation)
                foreach (var circle in Circles.Values)
                    circle.DeprivedStartSystem =
                        FindNearestQuickTravelGate(circle?.Systems.Values, deprivedLocation) ??
                        FindNearestSystem(circle?.Systems.Values, deprivedLocation);

            if (EclipseStartSystem?.Location is Vector2 eclipseLocation)
                foreach (var circle in Circles.Values)
                    circle.EclipseStartSystem =
                        FindNearestQuickTravelGate(circle?.Systems.Values, eclipseLocation) ??
                        FindNearestSystem(circle?.Systems.Values, eclipseLocation);

            if (VanguardStartSystem?.Location is Vector2 vanguardLocation)
                foreach (var circle in Circles.Values)
                    circle.VanguardStartSystem =
                        FindNearestQuickTravelGate(circle?.Systems.Values, vanguardLocation) ??
                        FindNearestSystem(circle?.Systems.Values, vanguardLocation);
        }

        public virtual void UpdateCircleMeasures(GalaxyCircle circle)
        {
            var minRadius = float.MaxValue;
            var maxRadius = 0f;

            foreach (var system in (circle.Systems ?? new()).Values)
            {
                var radius = system.Location.GetSize();

                if (radius < minRadius)
                    minRadius = radius;

                if (radius > maxRadius)
                    maxRadius = radius;
            }

            circle.MinRadius = minRadius;
            circle.MaxRadius = maxRadius;
        }

        public int GetSystemLevel(GalaxyMapStarSystem system)
        {
            return 0;
        }

        public static GalaxyMapStarSystem FindNearestSystem(IEnumerable<GalaxyMapStarSystem> systems, Vector2 location)
        {
            if (systems is null)
                return null;

            float minDistance = float.MaxValue;
            GalaxyMapStarSystem nearestSystem = null;

            foreach (var system in systems)
            {
                if (system is null)
                    continue;

                var newDistance = location.GetDistanceTo(new(system.X, system.Y));

                if (newDistance < minDistance)
                {
                    minDistance = newDistance;
                    nearestSystem = system;
                }
            }

            return nearestSystem;
        }


        public static GalaxyMapStarSystem FindNearestQuickTravelGate(IEnumerable<GalaxyMapStarSystem> systems, Vector2 location)
        {
            if (systems is null)
                return null;

            float minDistance = float.MaxValue;
            GalaxyMapStarSystem nearestSystem = null;

            foreach (var system in systems)
            {
                if (system is null ||
                    system.QuickTravalGates is null)
                    continue;

                var newDistance = location.GetDistanceTo(new(system.X, system.Y));

                if (newDistance < minDistance)
                {
                    minDistance = newDistance;
                    nearestSystem = system;
                }
            }

            return nearestSystem;
        }

        public GalaxyCircle GetCircle(int circleLevel)
        {
            return Circles?.GetValueOrDefault(circleLevel);
        }
    }
}