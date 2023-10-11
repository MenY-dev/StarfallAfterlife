﻿using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public partial class QuestsGenerator : GenerationTask
    {
        public SfaRealm Realm { get; set; }

        public GalaxyExtraMap ExtraMap { get; protected set; }

        protected int QurrentQuestId { get; set; } = 0;

        public QuestsGenerator(SfaRealm realm)
        {
            if (realm is null)
                return;

            Realm = realm;
            ExtraMap = new GalaxyExtraMap(Realm.GalaxyMap);
        }

        protected override bool Generate()
        {
            Realm.QuestsDatabase = Build();
            return true;
        }

        public virtual DiscoveryQuestsDatabase Build()
        {
            var qd = new DiscoveryQuestsDatabase();
            ExtraMap.Build();
            QurrentQuestId = 1;
            GenerateQuestLines(qd);
            //GenerateTaskBoardQuests(qd);
            ExtraMap = null;
            return qd;
        }

        protected virtual void GenerateQuestLines(DiscoveryQuestsDatabase qd)
        {
            var lines = Realm.Database.QuestLines;

            if (lines is not null)
            {
                foreach (var info in lines.Values)
                    GenerateQuestsLine(qd, info);
            }
        }

        protected virtual void GenerateQuestsLine(DiscoveryQuestsDatabase qd, QuestLineInfo info)
        {
            if (qd is null || info is null)
                return;

            var questLine = new DiscoveryQuestLine()
            {
                Stages = new(),
            };

            foreach (var logicInfo in info.Logics)
            {
                if (logicInfo.Position != 1090)
                    continue;

                var quest = GenerateQuestLinesQuest(qd, info, logicInfo, QurrentQuestId);

                if (quest is null)
                    continue;

                qd.AddQuest(quest);
                QurrentQuestId++;

                var stageEntry = new DiscoveryQuestLineEntry()
                {
                    QuestId = quest.Id,
                    AccesLevel = logicInfo.AccessLevel,
                    UniqueMobGroup = logicInfo.UniqueMobGroup,
                };

                if (questLine.Stages.TryGetValue(logicInfo.Position, out var stage) == true)
                {
                    (stage.Entries ??= new()).Add(stageEntry);
                }
                else
                {
                    stage = new DiscoveryQuestLineStage
                    {
                        Position = logicInfo.Position,
                        Entries = new List<DiscoveryQuestLineEntry>() { stageEntry }
                    };

                    questLine.Stages.Add(stage.Position, stage);
                }
            }
        }

        protected virtual DiscoveryQuest GenerateQuestLinesQuest(DiscoveryQuestsDatabase qd, QuestLineInfo questLine, QuestLineLogicInfo logic, int id)
        {
            if (qd is null || questLine is null || logic is null)
                return null;

            var info = Realm.Database.QuestsLogics?.FirstOrDefault(l => l.Key == logic.LogicId).Value;
            var faction = GetQuestLineFaction(questLine);
            var startingSystem = Realm.GalaxyMap.GetStartingSystem(faction);
            var targetSystem = ExtraMap.GetCircle(logic.AccessLevel)?.GetStartSystem(faction);

            if (info is null ||
                faction.IsMainFaction() == false ||
                startingSystem is null ||
                targetSystem is null)
                return null;

            var obj = startingSystem.Motherships?.FirstOrDefault() ?? startingSystem.GetObjectsWithTaskBoard()?.FirstOrDefault();

            if (obj is null)
                return null;

            var quest = new DiscoveryQuest()
            {
                Id = id,
                LogicId = info.Id,
                LogicName = info.UniqueLogicIdentifier,
                Type = info.Type,
                Level = logic.AccessLevel,
                Reward = new(),
                ObjectSystem = startingSystem.Id,
                ObjectType = obj.ObjectType,
                ObjectId = obj.Id,
                ObjectFaction = faction,
                Conditions = new JsonArray(),
            };

            var context = new QuestContext
            {
                Quest = quest,
                MobsDatabase = Realm.MobsDatabase,
                MobsMap = Realm.MobsMap,
                TargetLevel = logic.AccessLevel,
                TargetSystemId = targetSystem.Id,
                TargetFaction = faction,
            };

            foreach (var item in info.Conditions)
            {
                if (GenerateQuestCondition(item, context) is JsonObject condition)
                    quest.Conditions.Add(condition);
                else
                    return null;
            }

            if (info.Conditions.Count > 0 && quest.Conditions.Count < 1)
                return null;

            quest.Reward = info.Rewards.FirstOrDefault();

            return quest;
        }

        protected virtual void GenerateTaskBoardQuests(DiscoveryQuestsDatabase qd)
        {
            var logics = Realm.Database.QuestsLogics;

            if (logics is not null)
            {
                foreach (var logic in logics.Values)
                {
                    if (logic is null || logic.Type != QuestType.Task)
                        continue;

                    foreach (var item in GetObjectsForQuestLogic(logic))
                    {
                        var quest = GenerateQuestForTaskBoard(logic, item, QurrentQuestId);

                        if (quest is not null)
                        {
                            qd.AddQuest(quest);
                            QurrentQuestId++;
                        }
                    }
                }
            }
        }

        protected virtual DiscoveryQuest GenerateQuestForTaskBoard(QuestLogicInfo info, StarSystemObjectInfo systemObject, int id)
        {
            var quest = new DiscoveryQuest()
            {
                Id = id,
                LogicId = info.Id,
                LogicName = info.UniqueLogicIdentifier,
                Type = info.Type,
                Reward = new(),
                ObjectSystem = systemObject.SystemId,
                ObjectType = systemObject.Type,
                ObjectId = systemObject.Id,
                Level = systemObject.Level,
                ObjectFaction = systemObject.Faction,
                Conditions = new JsonArray(),
            };

            var context = new QuestContext
            {
                Quest = quest,
                MobsDatabase = Realm.MobsDatabase,
                MobsMap = Realm.MobsMap,
                TargetLevel = systemObject.Level,
                TargetSystemId = systemObject.SystemId,
                TargetFaction = systemObject.Faction,
            };

            foreach (var item in info.Conditions)
            {
                if (GenerateQuestCondition(item, context) is JsonObject condition)
                    quest.Conditions.Add(condition);
                else
                    return null;
            }

            if (info.Conditions.Count > 0 && quest.Conditions.Count < 1)
                return null;

            quest.Reward = info.Rewards.FirstOrDefault();

            return quest;
        }

        protected virtual JsonObject GenerateQuestCondition(QuestConditionInfo info, QuestContext context)
        {
            var result = false;
            var condition = new JsonObject
            {
                ["type"] = (byte)info.Type,
                ["identity"] = info.Identity,
                ["progress_require"] = info.ProgressRequire,
            };

            switch (info.Type)
            {
                case QuestConditionType.None:
                    break;
                case QuestConditionType.DeliverItems:
                    result = GenDeliverItemsCondition(context, condition, info);
                    break;
                case QuestConditionType.ExploreSystem:
                    result = GenExploreSystemCondition(context, condition, info);
                    break;
                case QuestConditionType.KillShip:
                    result = GenKillShipCondition(context, condition, info);
                    break;
                case QuestConditionType.KillUniquePirateShip:
                    result = GenKillUniquePirateShipCondition(context, condition, info);
                    break;
                case QuestConditionType.InterceptShip:
                    /// Need cross system mobs
                    break;
                case QuestConditionType.ScanUnknownPlanet:
                    result = GenScanUnknownPlanetCondition(context, condition, info);
                    break;
                case QuestConditionType.CompleteTask:
                    result = GenCompleteTaskCondition(context, condition, info);
                    break;
                case QuestConditionType.CompleteSpecialInstance:
                    //Tutorial
                    break;
                case QuestConditionType.ExploreObject:
                    //Level Quests
                    break;
                case QuestConditionType.StatTracking:
                    result = GenStatTrackingCondition(context, condition, info);
                    break;
                case QuestConditionType.Client:
                    //Tutorial
                    break;
                case QuestConditionType.ExploreRelictShip:
                    //Relic quest line
                    break;
                case QuestConditionType.DeliverMobDrop:
                    result = GenDeliverMobDropCondition(context, condition, info);
                    break;
                case QuestConditionType.KillUniquePersonalPirateShip:
                    //Tutorial
                    break;
                case QuestConditionType.KillMobShip:
                    result = GenKillMobShipCondition(context, condition, info);
                    break;
                case QuestConditionType.KillFactionGroupShip:
                    result = GenKillFactionGroupShipCondition(context, condition, info);
                    break;
                case QuestConditionType.KillPiratesOutpost:
                    result = GenKillPiratesOutpostCondition(context, condition, info);
                    break;
                case QuestConditionType.KillPiratesStation:
                    result = GenKillPiratesStationCondition(context, condition, info);
                    break;
                case QuestConditionType.KillBoss:
                    result = GenKillBossCondition(context, condition, info);
                    break;
                case QuestConditionType.DeliverQuestItem:
                    result = GenDeliverQuestItemCondition(context, condition, info);
                    break;
                case QuestConditionType.ScanSystemObject:
                    result = GenScanSystemObjectCondition(context, condition, info);
                    break;
                case QuestConditionType.KillBossOfAnyFactiont:
                    result = GenKillBossOfAnyFactiontCondition(context, condition, info);
                    break;
                case QuestConditionType.ResearchProject:
                    result = GenResearchProjectCondition(context, condition, info);
                    break;
                case QuestConditionType.ReachCharacterLevel:
                    result = GenReachCharacterLevelCondition(context, condition, info);
                    break;
                case QuestConditionType.InstanceEvent:
                    //SpaceFarm
                    break;
                case QuestConditionType.PickUpAndDeliverQuestItem:
                    result = GenPickUpAndDeliverQuestItemCondition(context, condition, info);
                    break;
                case QuestConditionType.MineQuestItem:
                    //Unknown quest
                    break;
                case QuestConditionType.DeliverRandomItems:
                    result = GenDeliverRandomItemsCondition(context, condition, info);
                    break;
                case QuestConditionType.InterceptPersonalMob:
                    /// Need cross system mobs
                    break;
                default:
                    break;
            }

            if (result == false)
                return null;

            return condition;
        }

        protected IEnumerable<StarSystemObjectInfo> GetObjectsForQuestLogic(QuestLogicInfo logic)
        {
            if (logic is null)
                yield break;

            var info = LogicAdditionalInfo.Parce(logic);

            if (logic.Type == QuestType.Task)
            {
                switch (info.QuestLine)
                {
                    case "addon_inhabited_planet":
                    case "addon_small_colony":
                    case "core_populous_system":
                        foreach (var item in
                            from system in Realm.GalaxyMap.Systems
                            where (Faction)system.Faction is Faction.Vanguard
                            where system.Level == info.TargetLevel &&
                                  system.Planets != null
                            from planet in system.Planets
                            where planet.Faction != Faction.None
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = planet.ObjectType,
                                Id = planet.Id,
                                Level = system.Level,
                                Faction = (Faction)system.Faction,
                            })
                            yield return item;
                        yield break;

                    case "":
                    case null:
                        yield break;

                    case "explore_relic_ship":
                        yield break;

                    case "space_farm":
                        yield break;

                    case "research_project":
                        yield break;

                    case "deprived":
                    case "eclipse":
                    case "vanguard":
                        yield break;


                    default:
                        foreach (var item in
                            from system in Realm.GalaxyMap.Systems
                            where (Faction)system.Faction is Faction.Vanguard
                            where info.TargetLevel < 0 ? true : system.Level == info.TargetLevel
                            let objects = system.GetObjectsWithTaskBoard()
                            where objects != null
                            from obj in objects
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = obj.ObjectType,
                                Id = obj.Id,
                                Level = system.Level,
                                Faction = (Faction)system.Faction
                            })
                            yield return item;
                        yield break;
                }
            }
        }

        protected static Faction GetQuestLineFaction(QuestLineInfo questLine)
        {
            if (questLine.Name is null)
                return Faction.None;
            else if (questLine.Name.Contains("deprived"))
                return Faction.Deprived;
            else if (questLine.Name.Contains("eclipse"))
                return Faction.Eclipse;
            else if (questLine.Name.Contains("vanguard"))
                return Faction.Vanguard;

            return Faction.None;
        }

        protected struct StarSystemObjectInfo
        {
            public int SystemId;
            public GalaxyMapObjectType Type;
            public int Id;
            public int Level;
            public Faction Faction;
        }

        protected class QuestContext
        {
            public DiscoveryQuest Quest;
            public MobsDatabase MobsDatabase;
            public MobsMap MobsMap;
            public Faction TargetFaction;
            public int TargetLevel;
            public int TargetSystemId;
        }

        protected class LogicAdditionalInfo
        {
            public string QuestLine { get; set; }

            public int TargetLevel { get; set; } = -1;

            public static readonly string[] QuestLines = new[]
            {
                "addon_inhabited_planet",
                "addon_small_colony",
                "core_populous_system",
                "addon_trade_station",
                "core_trade_station",
                "core_science_station",
                "core_miner_station",
                "explore_relic_ship",
                "space_farm",
                "deprived",
                "eclipse",
                "vanguard",
                "deliver_mob_drop",
                "research_project",
            };

            public static LogicAdditionalInfo Parce(QuestLogicInfo logic)
            {
                var info = new LogicAdditionalInfo();

                if (logic is null)
                    return info;

                info.QuestLine = logic.UniqueLogicIdentifier;

                if (logic.UniqueLogicIdentifier is string identifier)
                {
                    if (logic.Type == 0)
                    {
                        foreach (var item in QuestLines)
                        {
                            if (identifier.StartsWith(item))
                            {
                                info.QuestLine = item;
                            }
                        }

                        int lvlStartPos = identifier.LastIndexOf('_') + 1;

                        if (identifier.StartsWith("scan_rich_asteroids") == false &&
                            lvlStartPos > 0 && lvlStartPos < identifier.Length &&
                            int.TryParse(identifier[lvlStartPos..], out int lvl))
                        {
                            info.TargetLevel = lvl;
                        }
                    }
                }

                return info;
            }
        }
    }
}
