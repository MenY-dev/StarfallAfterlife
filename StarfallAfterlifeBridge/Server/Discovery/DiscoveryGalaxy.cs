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

        public Dictionary<int, StarSystem> ActiveSystems { get; } = new();

        public MulticastEvent Listeners { get; } = new();

        public int GalaxyFrameRate { get; set; } = 30;

        private DiscoveryLoop DiscoveryLoop { get; set; }

        protected object UpdateLockher { get; } = new();

        protected object UpdateActionsLockher { get; } = new();

        public Dictionary<int, TaskBoardEntry> Quests { get; } = new();

        protected List<Action<DiscoveryGalaxy>> PreUpdateActions { get; } = new();

        protected List<Action<DiscoveryGalaxy>> PostUpdateActions { get; } = new();

        public DiscoveryGalaxy(SfaRealm realm)
        {
            Realm = realm;
        }

        public StarSystem GetActiveSystem(int systemId, bool activateSystem = false)
        {
            if (ActiveSystems.ContainsKey(systemId) == true)
                return ActiveSystems[systemId];

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
                    fleet.Route.Update(loc);

                if (system is not null)
                {
                    system.AddFleet(fleet);
                    fleet.State = FleetState.InGalaxy;
                }
            }
        }

        public StarSystem ActivateStarSystem(int systemId)
        {
            lock (UpdateLockher)
            {
                if (ActiveSystems.ContainsKey(systemId) == true)
                    return ActiveSystems[systemId];

                var system = new StarSystem(this, systemId);
                system.Init();

                if (system.Info is null)
                    return null;

                ActiveSystems[systemId] = system;

                //var testFleet = new DiscoveryAiFleet
                //{
                //    Name = "Abaas",
                //    Id = 1000000 + systemId * 100 + 0,
                //    Faction = (Faction)system.Info.Faction,
                //    FactionGroup = system.Info.FactionGroup,
                //    Level = 85,
                //    Speed = 4,
                //    MobId = 519298432
                //};

                //testFleet.SetAI(new PatrollingAI(new Vector2[]
                //{
                //    new Vector2(5, 4),
                //    new Vector2(16, -8),
                //    new Vector2(-14, -8),
                //    new Vector2(3, 2),
                //    new Vector2(-7, 1),
                //}));

                //system.AddFleet(testFleet);

                return system;
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

                foreach (var system in ActiveSystems.Values)
                    system?.Update();

                HandleUpdateActions(PostUpdateActions);
            }
        }
    }
}
