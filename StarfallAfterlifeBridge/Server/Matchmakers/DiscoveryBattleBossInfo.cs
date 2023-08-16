using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class DiscoveryBattleBossInfo
    {
        public int FleetId { get; set; }

        public DiscoveryMobInfo Mob { get; set; }

        public InstanceMob InstanceMob { get; set; }

        public DiscoveryBattleBossInfo(int fleetId, DiscoveryMobInfo mob)
        {
            FleetId = fleetId;
            Mob = mob;

            if (mob is null)
                return;

            InstanceMob = new InstanceMob
            {
                Id = FleetId,
                BehaviorTreeName = mob.BehaviorTreeName,
                MobInternalName = mob.InternalName,
                MobFaction = mob.Faction,
                Tags = mob.Tags.ToList(),
            };
        }
    }
}