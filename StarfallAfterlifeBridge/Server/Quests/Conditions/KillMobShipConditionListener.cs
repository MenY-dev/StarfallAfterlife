using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class KillMobShipConditionListener : QuestConditionListener, IBattleInstanceListener
    {
        public int MobId { get; set; }

        public List<int> Systems { get; set; }

        public KillMobShipConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is not JsonObject)
                return;

            MobId = (int?)doc["target_mob_id"] ?? -1;
            Systems = doc["target_systems"]?.DeserializeUnbuffered<List<int>>() ?? new();
        }

        void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
        {
            if (mobInfo is null ||
                fleetId != Quest?.Character.UniqueId ||
                (Systems is not null && Systems.Count > 0 && Systems.Contains(mobInfo.SystemId) == false))
                return;

            if (mobInfo.MobId == MobId)
            {
                Progress = Math.Min(ProgressRequire, Progress + 1);
            }
        }
    }
}
