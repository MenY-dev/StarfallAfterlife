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
    internal class KillBossOfAnyFaciontConditionListener : QuestConditionListener, IBattleInstanceListener
    {
        public Faction Faction { get; set; } = Faction.None;
        public int FactionGroup { get; set; } = -1;

        public KillBossOfAnyFaciontConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }


        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is not JsonObject)
                return;

            Faction = (Faction?)(byte?)doc["target_faction"] ?? Faction.None;
            FactionGroup = (int?)doc["target_faction_group_id"] ?? -1;
        }

        void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
        {
            if (mobInfo is null ||
                mobInfo.Tags.Contains("Mob.SpecialShip.Boss") != true ||
                fleetId != Quest?.Character.UniqueId)
                return;

            if (mobInfo.Faction != Faction)
                return;

            Progress = Math.Min(ProgressRequire, Progress + 1);
        }
    }
}
