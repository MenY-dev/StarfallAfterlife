using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Events;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryGalaxy : SfaObject
    {
        public SfaRealm Realm { get; }

        public GalaxyMap Map => Realm.GalaxyMap;

        public SfaDatabase Database => Realm.Database;

        public SortedList<int, StarSystem> ActiveSystems { get; } = new();

        public MulticastEvent Listeners { get; } = new();

        public int GalaxyFrameRate { get; set; } = 30;

        private DiscoveryLoop DiscoveryLoop { get; set; }

        protected object UpdateLockher { get; } = new();

        protected object UpdateActionsLockher { get; } = new();

        protected List<Action<DiscoveryGalaxy>> PreUpdateActions { get; } = new();

        protected List<Action<DiscoveryGalaxy>> PostUpdateActions { get; } = new();

        public DiscoveryGalaxy(SfaRealm realm)
        {
            Realm = realm;
        }

        public StarSystem GetActiveSystem(int systemId, bool activateSystem = false)
        {
            if (ActiveSystems.TryGetValue(systemId, out var system) == true)
                return system;

            if (activateSystem == true)
                return ActivateStarSystem(systemId);

            return null;
        }

        public void EnterToStarSystem(int systemId, DiscoveryFleet fleet, Vector2? location = null)
        {
            lock (UpdateLockher)
            {
                if (fleet is null)
                    return;

                var system = ActivateStarSystem(systemId);

                if (location is Vector2 loc)
                {
                    fleet.SetLocation(loc, true);
                }

                if (system is not null)
                {
                    system.AddFleet(fleet);
                    fleet.SetFleetState(FleetState.InGalaxy);
                }
            }
        }

        public StarSystem ActivateStarSystem(int systemId)
        {
            lock (UpdateLockher)
            {
                StarSystem system = null;

                if (ActiveSystems.TryGetValue(systemId, out system) == true)
                    return system;

                system = new StarSystem(this, systemId);
                system.Init();

                if (system.Info is null)
                    return null;

                if (ActiveSystems.TryAdd(systemId, system))
                    return system;

                return null;
            }
        }

        public void Start()
        {
            Stop();

            DiscoveryLoop = new(UpdateGalaxy, GalaxyFrameRate);
            DiscoveryLoop?.Start();
        }

        public void Stop()
        {
            DiscoveryLoop?.Stop();
            DiscoveryLoop = null;
        }

        public void BeginPreUpdateAction(Action<DiscoveryGalaxy> action)
        {
            lock (UpdateActionsLockher)
                PreUpdateActions.Add(action);
        }

        public void BeginPostUpdateAction(Action<DiscoveryGalaxy> action)
        {
            lock (UpdateActionsLockher)
                PostUpdateActions.Add(action);
        }

        protected void HandleUpdateActions(IList<Action<DiscoveryGalaxy>> actions)
        {
            lock (UpdateActionsLockher)
            {
                var buffer = actions.ToList();
                actions.Clear();

                foreach (var action in buffer)
                {
                    try
                    {
                        action?.Invoke(this);
                    }
                    catch (Exception e)
                    {
                        SfaDebug.Print(e, GetType().Name);
                    }
                }
            }
        }

        protected void UpdateGalaxy()
        {
            lock (UpdateLockher)
            {
                HandleUpdateActions(PreUpdateActions);

                for (int i = 0; i < ActiveSystems.Count; i++)
                {
                    try
                    {
                        ActiveSystems.GetValueAtIndex(i)?.Update();
                    }
                    catch (Exception e)
                    {
                        SfaDebug.Print(e, GetType().Name);
                    }
                }

                HandleUpdateActions(PostUpdateActions);
            }
        }
    }
}
