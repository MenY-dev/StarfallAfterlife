using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class KillUniquePirateShipConditionListener : QuestConditionListener, IBattleInstanceListener
    {
        public int FleetId { get; set; }

        public Faction Faction { get; set; }

        public int SystemId { get; set; }



        public KillUniquePirateShipConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is not JsonObject)
                return;

            FleetId = (int?)doc["target_id"] ?? -1;
            Faction = (Faction?)(byte?)doc["fleet_faction"] ?? Faction.None;
            SystemId = (int?)doc["target_system"] ?? -1;
        }

        void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
        {
            if (mobInfo is null ||
                fleetId != Quest?.Character.UniqueId ||
                mobInfo.FleetId != FleetId ||
                mobInfo.Tags is null ||
                mobInfo.Tags.Contains("Mob.specialship.Elite", StringComparer.InvariantCultureIgnoreCase) == false)
                return;

            Progress = Math.Min(ProgressRequire, Progress + 1);
        }
    }
}
