using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class KillShipConditionListener : QuestConditionListener, IBattleInstanceListener
    {
        public int MinLevel { get; set; }

        public List<byte> Factions { get; set; }

        public List<int> ShipClass { get; set; }

        public KillShipConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is not JsonObject)
                return;

            MinLevel = (int?)doc["min_target_level"] ?? -1;
            Factions = doc["factions"]?.DeserializeUnbuffered<List<byte>>() ?? new();
            ShipClass = doc["ship_class"]?.DeserializeUnbuffered<List<int>>() ?? new();
        }

        void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
        {
            if (mobInfo is null ||
                fleetId != Quest?.Character.UniqueId ||
                (MinLevel != -1 && mobInfo.Level < MinLevel) ||
                (Factions is not null && Factions.Count > 0 && Factions.Contains((byte)mobInfo.Faction) == false) ||
                (ShipClass is not null && ShipClass.Count > 0 && ShipClass.Contains(mobInfo.ShipClass) == false))
                return;

            Progress = Math.Min(ProgressRequire, Progress + 1);
        }
    }
}
