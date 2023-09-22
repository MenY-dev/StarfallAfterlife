using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryAiFleet : DiscoveryFleet
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.AiFleet;

        public float RespawnTimeout { get; set; } = 180;

        public DateTime RespawnTime { get; protected set; }

        public override void Update()
        {
            base.Update();

            if (State == FleetState.Destroyed &&
                LastUpdateTime >= RespawnTime)
            {
                Broadcast<IStarSystemObjectListener>(l => l.OnObjectSpawned(this));
                SetFleetState(FleetState.InGalaxy);
            }
        }

        protected override void OnSystemChanged(StarSystem system)
        {
            base.OnSystemChanged(system);
        }

        protected override void OnFleetStateChanged(FleetState oldState, FleetState newState)
        {
            base.OnFleetStateChanged(oldState, newState);

            if (newState == FleetState.Destroyed)
            {
                RespawnTime = DateTime.Now.AddSeconds(RespawnTimeout);
            }
        }
    }
}
