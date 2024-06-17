using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
using StarfallAfterlife.Bridge.SfPackageLoader;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryAiFleet : DiscoveryFleet
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.AiFleet;

        public bool UseRespawn { get; set; } = true;

        public float RespawnTimeout { get; set; } = 180;

        public DateTime RespawnTime { get; protected set; }

        public bool IsDynamicMob => MobId < 0;

        public DynamicMobType DynamicMobType { get; protected set; } = DynamicMobType.None;

        public void Init(GalaxyMapMob mob, DiscoveryMobInfo mobInfo, FleetAI ai = null)
        {
            if (mob is not null)
            {
                Id = mob.FleetId;
                FactionGroup = mob.FactionGroup;
                Hex = mob.SpawnHex;
                SetLocation(SystemHexMap.HexToSystemPoint(mob.SpawnHex), true);
            }

            Init(mobInfo, ai);
        }

        public void Init(DynamicMob mob, FleetAI ai = null)
        {
            if (mob?.Info is DiscoveryMobInfo mobInfo)
            {
                Init(mobInfo, ai);
                Id = mobInfo.Id;
                MobId = -mobInfo.Id;
                DynamicMobType = mob.Type;
            }
        }

        public void Init(DiscoveryMobInfo mobInfo, FleetAI ai = null)
        {
            if (mobInfo is null)
                return;

            Faction = mobInfo.Faction;
            Name = mobInfo.InternalName;
            Level = mobInfo.Level;
            MobId = mobInfo.Id;
            Hull = mobInfo.GetMainShipHull();

            if (ai is not null)
                SetAI(ai);
        }

        public override void Update()
        {
            base.Update();

            if (UseRespawn == true &&
                State == FleetState.Destroyed &&
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
                RespawnTime = DateTime.UtcNow.AddSeconds(RespawnTimeout);
            }
        }
    }
}
