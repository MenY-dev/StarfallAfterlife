using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Quests.Conditions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class QuestConditionListener : IDiscoveryListener
    {
        public string Identity { get; protected set; }

        public QuestListener Quest { get; protected set; }

        public QuestConditionType Type { get; protected set; }

        public int Progress
        {
            get => progress;
            set
            {
                if (value != progress)
                {
                    progress = value;
                    RaiseProgressChanged();
                }
            }
        }

        public int ProgressRequire
        {
            get => progressRequire;
            set
            {
                if (value != progressRequire)
                {
                    progressRequire = value;
                    RaiseProgressChanged();
                }
            }
        }

        public bool ListeningStarted { get; protected set; } = false;

        public virtual bool IsCompleted => Progress >= ProgressRequire;

        private int progressRequire = 0;
        private int progress = 0;

        protected List<DiscoveryQuestBinding> Bindings { get; set; }

        public QuestConditionListener(QuestListener quest, JsonNode info)
        {
            Quest = quest;
            
            if (info is JsonObject)
                LoadConditionInfo(info);
        }

        public static QuestConditionListener Create(QuestListener quest, JsonNode info)
        {
            if (info is JsonObject condotion &&
                (QuestConditionType?)(int?)condotion["type"] is QuestConditionType type)
            {
                switch (type)
                {
                    case QuestConditionType.DeliverItems:
                        return new DeliverItemsConditionListener(quest, info);

                    case QuestConditionType.DeliverRandomItems:
                        return new DeliverRandomItemsConditionListener(quest, info);

                    case QuestConditionType.PickUpAndDeliverQuestItem:
                        return new PickUpAndDeliverQuestItemConditionListener(quest, info);

                    case QuestConditionType.DeliverQuestItem:
                        return new DeliverQuestItemConditionListener(quest, info);

                    case QuestConditionType.ScanSystemObject:
                        return new ScanSystemObjectConditionListener(quest, info);

                    case QuestConditionType.ScanUnknownPlanet:
                        return new ScanUnknownPlanetConditionListener(quest, info);

                    case QuestConditionType.ExploreSystem:
                        return new ExploreSystemConditionListener(quest, info);

                    case QuestConditionType.KillMobShip:
                        return new KillMobShipConditionListener(quest, info);

                    case QuestConditionType.KillFactionGroupShip:
                        return new KillFactionGroupShipConditionListener(quest, info);

                    case QuestConditionType.KillPiratesOutpost:
                        return new KillPiratesOutpostConditionListener(quest, info);

                    case QuestConditionType.KillPiratesStation:
                        return new KillPiratesStationConditionListener(quest, info);

                    case QuestConditionType.KillBoss:
                        return new KillBossConditionListener(quest, info);

                    case QuestConditionType.KillShip:
                        return new KillShipConditionListener(quest, info);

                    case QuestConditionType.KillUniquePirateShip:
                        return new KillUniquePirateShipConditionListener(quest, info);

                    case QuestConditionType.DeliverMobDrop:
                        return new DeliverMobDropConditionListener(quest, info);

                    case QuestConditionType.CompleteTask:
                        return new CompleteTaskConditionListener(quest, info);

                    case QuestConditionType.ReachCharacterLevel:
                        return new ReachCharacterLevelConditionListener(quest, info);

                    case QuestConditionType.ResearchProject:
                        return new ResearchProjectConditionListener(quest, info);

                    case QuestConditionType.KillBossOfAnyFactiont:
                        return new KillBossOfAnyFaciontConditionListener(quest, info);

                    case QuestConditionType.StatTracking:
                        return new StatTrackingConditionListener(quest, info);

                    case QuestConditionType.ExploreObject:
                        return new ExploreSystemObjectConditionListener(quest, info);

                    case QuestConditionType.ExploreRelictShip:
                        return new ExploreRelictShipConditionListener(quest, info);

                    default:
                        return new QuestConditionListener(quest, info);
                }
            }

            return null;
        }

        protected virtual void LoadConditionInfo(JsonNode doc)
        {
            Identity = (string)doc["identity"];
            Type = (QuestConditionType?)(int?)doc["type"] ?? QuestConditionType.None;
            ProgressRequire = (int?)doc["progress_require"] ?? 0;
            Bindings = doc["bindings"]?.DeserializeUnbuffered<List<DiscoveryQuestBinding>>();
        }

        public virtual void LoadProgress(ConditionProgress progressInfo)
        {
            Progress = progressInfo?.Progress ?? 0;
        }

        public virtual ConditionProgress SaveProgress()
        {
            return new ConditionProgress() { Progress = Progress };
        }

        public virtual void StartListening()
        {
            ListeningStarted = true;
            Quest?.Fleet?.AddListener(this);
            Quest?.Character.Events?.Add(this);
            Update();
        }

        public virtual void StopListening()
        {
            ListeningStarted = false;
            Quest?.Fleet?.RemoveListener(this);
            Quest?.Character.Events?.Remove(this);
        }

        public virtual void Update()
        {

        }

        private void OnProgressChanged()
        {
            if (ListeningStarted == false)
                return;

            Quest?.OnProgressChanged(this);
        }

        public virtual void DeliverQuestItems()
        {
            //if (Quest?.Owner is DockableObject obj)
            //{
            //    var storages = obj.Storages.Where(s => s.Type == StorageType.Cargo).ToList();
            //    var targetProgress = Math.Max(Info.ProgressRequire - Progress, 0);
            //    var targetItem = Info.ItemToDeliver;
            //    var newProgress = 0;

            //    foreach (var item in obj.Storages.Where(s => s.Type == StorageType.Inventory))
            //        storages.Add(item);

            //    foreach (var storage in storages)
            //    {
            //        if (newProgress == targetProgress)
            //            break;

            //        newProgress += storage.Remove(targetItem, targetProgress - newProgress);
            //        newProgress.ToString();
            //    }

            //    Progress += newProgress;
            //}

        }

        public virtual QuestLocationInfo GetLocationInfo()
        {
            return null;
        }

        public virtual List<DiscoveryQuestBinding> CreateBindings()
        {
            return Bindings?.ToList();
        }

        public virtual void RaiseAction(string data)
        {

        }

        public virtual void RaiseInitialActions()
        {

        }

        public virtual void RaiseProgressChanged() => OnProgressChanged();

        public virtual List<DiscoveryDropRule> CreateDropRules()
        {
            return null;
        }

        public virtual List<QuestTileParams> CreateInstanceTileParams(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            return null;
        }
    }
}
