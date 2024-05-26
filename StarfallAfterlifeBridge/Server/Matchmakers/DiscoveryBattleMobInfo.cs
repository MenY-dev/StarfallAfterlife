using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class DiscoveryBattleMobInfo
    {
        public int FleetId { get; set; } = -1;

        public BattleMember Member { get; set; }

        public DiscoveryMobInfo Mob { get; set; }

        public InstanceAIFleet InstanceFleet { get; set; }

        public bool InBattle { get; set; }

        public DiscoveryBattleMobInfo(BattleMember member, DiscoveryMobInfo mob)
        {
            if (mob is null || member is null)
                return;

            Member = member;
            Mob = mob;
            FleetId = member.Fleet?.Id ?? -1;

            InstanceFleet = new InstanceAIFleet
            {
                Id = FleetId,
                Type = 0,
                Faction = mob.Faction,
                Level = mob.Level,
                HexOffsetX = member.HexOffset.X,
                HexOffsetY = member.HexOffset.Y,
                FleetXp = 10000,
                FactionGroup = member.Fleet?.FactionGroup ?? -1,
                Mob = new InstanceMob
                {
                    Id = FleetId,
                    BehaviorTreeName = mob.BehaviorTreeName,
                    MobInternalName = mob.InternalName,
                    MobFaction = mob.Faction,
                    Tags = mob.Tags?.ToList() ?? new(),
                },
                Cargo = new(),
            };
        }
    }
}
