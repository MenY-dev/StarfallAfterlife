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
    public class KillPiratesStationConditionListener : QuestConditionListener, IBattleInstanceListener
    {
        public int StationId { get; set; }

        public Faction Faction { get; set; }

        public int FactionGroup { get; set; }

        public int Level { get; set; }

        public int SystemId { get; set; }

        public List<int> Systems { get; set; }

        public KillPiratesStationConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }


        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            if (doc is not JsonObject)
                return;

            StationId = (int?)doc["station_id"] ?? -1;
            Faction = (Faction?)(byte?)doc["target_faction"] ?? Faction.None;
            FactionGroup = (int?)doc["group_id"] ?? -1;
            Level = (int?)doc["level"] ?? -1;
            SystemId = (int?)doc["star_system_id"] ?? -1;
            Systems = doc["star_system_ids"]?.DeserializeUnbuffered<List<int>>() ?? new();
        }

        void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
        {
            if (mobInfo is null ||
                mobInfo.MobId != -1 ||
                mobInfo.Tags.Contains("mothership") != true ||
                mobInfo.ObjectType != Discovery.DiscoveryObjectType.PiratesStation ||
                fleetId != Quest?.Character.UniqueId)
                return;

            if ((StationId != -1 && mobInfo.ObjectId != StationId) ||
                (FactionGroup != -1 && mobInfo.FactionGroup != FactionGroup) ||
                (Level != -1 && mobInfo.Level != Level) ||
                (Faction != Faction.None && mobInfo.Faction != Faction))
                return;

            Progress = Math.Min(ProgressRequire, Progress + 1);
        }
    }
}
