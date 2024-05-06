using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Quests.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DoctrineQuestConditionListener : QuestConditionListener
    {
        public DoctrineQuestConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        public override void RaiseInitialActions()
        {
            base.RaiseInitialActions();
            Update();
        }

        public override void Update()
        {
            base.Update();

            Quest?.Character?.DiscoveryClient.Server?.RealmInfo?.Use(_ =>
            {
                if (Quest is DoctrineQuestListener doctrineListener &&
                    doctrineListener.Character?.GetHouse() is SfHouse house &&
                    house.GetDoctrine(doctrineListener.DoctrineId) is HouseDoctrine doctrine)
                {
                    ProgressRequire = doctrine.Target;
                    Progress = doctrine.Progress;
                }
            });
        }

        public static DoctrineQuestConditionListener Create(DoctrineQuestListener quest, JsonNode info)
        {
            if (info is JsonObject condotion &&
                (QuestConditionType?)(int?)condotion["type"] is QuestConditionType type)
            {
                switch (type)
                {
                    case QuestConditionType.KillShip:
                        return new WartimeDoctrineQuestConditionListener(quest, info);

                    case QuestConditionType.CompleteTask:
                        return new MercenaryDoctrineQuestConditionListener(quest, info);

                    default:
                        return new DoctrineQuestConditionListener(quest, info);
                }
            }

            return null;
        }

        public class MercenaryDoctrineQuestConditionListener : DoctrineQuestConditionListener, ICharacterListener
        {
            public MercenaryDoctrineQuestConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
            {
            }

            void ICharacterListener.OnCurrencyUpdated(ServerCharacter character) { }

            void ICharacterListener.OnProjectResearch(ServerCharacter character, int projectId) { }

            void ICharacterListener.OnQuestCompleted(ServerCharacter character, QuestListener quest)
            {
                if (quest is null ||
                    quest.Info.Type != QuestType.Task)
                    return;

                if (Quest is DoctrineQuestListener doctrineListener)
                    doctrineListener?.Character.AddDoctrineProgress(doctrineListener.DoctrineId, 1);
            }

            void ICharacterListener.OnNewStatsReceived(ServerCharacter character, string tag, float value) { }
        }

        public class WartimeDoctrineQuestConditionListener : DoctrineQuestConditionListener, IBattleInstanceListener
        {
            public List<byte> Factions { get; set; }

            public List<int> ShipClass { get; set; }

            public WartimeDoctrineQuestConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
            {

            }

            protected override void LoadConditionInfo(JsonNode doc)
            {
                base.LoadConditionInfo(doc);

                if (doc is not JsonObject)
                    return;

                Factions = doc["factions"]?.DeserializeUnbuffered<List<byte>>() ?? new();
                ShipClass = doc["ship_class"]?.DeserializeUnbuffered<List<int>>() ?? new();
            }

            void IBattleInstanceListener.OnMobDestroyed(int fleetId, MobKillInfo mobInfo)
            {
                if (mobInfo is null ||
                    fleetId != Quest?.Character.UniqueId ||
                    (Factions is not null && Factions.Count > 0 && Factions.Contains((byte)mobInfo.Faction) == false) ||
                    (ShipClass is not null && ShipClass.Count > 0 && ShipClass.Contains(mobInfo.ShipClass) == false))
                    return;

                if (Quest is DoctrineQuestListener doctrineListener)
                    doctrineListener?.Character.AddDoctrineProgress(doctrineListener.DoctrineId, 1);
            }
        }
    }
}
