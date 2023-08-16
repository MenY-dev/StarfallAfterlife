using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class KillBossConditionListener : QuestConditionListener, IBattleInstanceListener
    {
        public int FleetId { get; set; }

        public int ObjectId { get; set; }

        public DiscoveryObjectType ObjectType { get; set; }

        public List<int> Systems { get; set; }

        public KillBossConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }


        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is not JsonObject)
                return;

            FleetId = (int?)doc["target_mob_id"] ?? -1;
            ObjectId = (int?)doc["target_object_id"] ?? -1;
            ObjectType = (DiscoveryObjectType?)(byte?)doc["target_object_type"] ?? DiscoveryObjectType.None;
            Systems = doc["target_systems"]?.DeserializeUnbuffered<List<int>>() ?? new();
        }

        void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
        {
            if (mobInfo is null ||
                mobInfo.Tags.Contains("Mob.SpecialShip.Boss") != true ||
                fleetId != Quest?.Character.UniqueId ||
                (Systems is not null && Systems.Count > 0 && Systems.Contains(mobInfo.SystemId) == false))
                return;

            if (mobInfo.FleetId == -1 ||
                mobInfo.FleetId != FleetId)
                return;

            Progress = Math.Min(ProgressRequire, Progress + 1);
        }
    }
}
