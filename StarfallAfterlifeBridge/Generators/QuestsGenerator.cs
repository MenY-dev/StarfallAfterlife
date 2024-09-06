﻿using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Primitives;
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
        public int Seed { get; }

        public SfaRealm Realm { get; set; }

        public GalaxyExtraMap ExtraMap { get; protected set; }

        protected Random128 Rnd { get; set; }

        protected static readonly List<LevelQuestInfo> RelictShipLevelingQuests = new()
        {
            new() { Faction = Faction.Deprived, Level = 0, Logics = new(){ 1315019028 } },
            new() { Faction = Faction.Eclipse,  Level = 0, Logics = new(){ 1315019028 } },
            new() { Faction = Faction.Vanguard, Level = 0, Logics = new(){ 1315019028 } },

            new() { Faction = Faction.Deprived, Level = 15, Logics = new(){ 1704588733 } },
            new() { Faction = Faction.Eclipse,  Level = 15, Logics = new(){ 1704588733 } },
            new() { Faction = Faction.Vanguard, Level = 15, Logics = new(){ 1704588733 } },

            new() { Faction = Faction.Deprived, Level = 29, Logics = new(){ 2094158438 } },
            new() { Faction = Faction.Eclipse,  Level = 29, Logics = new(){ 2094158438 } },
            new() { Faction = Faction.Vanguard, Level = 29, Logics = new(){ 2094158438 } },

            new() { Faction = Faction.Deprived, Level = 43, Logics = new(){ 336244495 } },
            new() { Faction = Faction.Eclipse,  Level = 43, Logics = new(){ 336244495 } },
            new() { Faction = Faction.Vanguard, Level = 43, Logics = new(){ 336244495 } },

            new() { Faction = Faction.Deprived, Level = 57, Logics = new(){ 725814200 } },
            new() { Faction = Faction.Eclipse,  Level = 57, Logics = new(){ 725814200 } },
            new() { Faction = Faction.Vanguard, Level = 57, Logics = new(){ 725814200 } },

            new() { Faction = Faction.Deprived, Level = 71, Logics = new(){ 1115383905 } },
            new() { Faction = Faction.Eclipse,  Level = 71, Logics = new(){ 1115383905 } },
            new() { Faction = Faction.Vanguard, Level = 71, Logics = new(){ 1115383905 } },

            new() { Faction = Faction.Deprived, Level = 85, Logics = new(){ 1504953610 } },
            new() { Faction = Faction.Eclipse,  Level = 85, Logics = new(){ 1504953610 } },
            new() { Faction = Faction.Vanguard, Level = 85, Logics = new(){ 1504953610 } },
        };

        public QuestsGenerator(SfaRealm realm, int seed = 0)
        {
            if (realm is null)
                return;

            Realm = realm;
            ExtraMap = new GalaxyExtraMap(Realm.GalaxyMap);
            Seed = seed;
        }

        public void Init()
        {
            Rnd = new(Seed);
            ExtraMap.Build();
        }

        protected override bool Generate()
        {
            Realm.QuestsDatabase = Build();
            return true;
        }

        public virtual DiscoveryQuestsDatabase Build()
        {
            var qd = new DiscoveryQuestsDatabase();
            Init();
            GenerateQuestLines(qd);
            GenerateTaskBoardQuests(qd);
            GenerateLevelingQuests(qd);
            return qd;
        }

        protected virtual void GenerateQuestLines(DiscoveryQuestsDatabase qd)
        {
            var lines = Realm.Database.QuestLines;

            if (lines is not null)
            {
                foreach (var info in lines.Values)
                    if (GenerateQuestsLine(qd, info) is DiscoveryQuestLine questLine)
                        qd.AddQuestLine(questLine);
            }
        }

        protected virtual DiscoveryQuestLine GenerateQuestsLine(DiscoveryQuestsDatabase qd, QuestLineInfo info)
        {
            if (qd is null || info is null)
                return null;

            var idInfo = new QuestIdInfo
            {
                Type = info.Type,
                Faction = info.Faction,
                TargetFaction = info.TargetFaction,
                IsLevelingQuest = false,
                LocalId = 0
            };

            var questLine = new DiscoveryQuestLine()
            {
                Id = info.Id,
                Name = info.Name,
                Stages = new(),
            };

            foreach (var logicInfo in info.Logics
                .OrderBy(i => i.Position)
                .ThenBy(i => i.LogicId))
            {
                idInfo.LocalId++;

                var quest = GenerateQuestLinesQuest(qd, info, logicInfo, idInfo.ToId());

                if (quest is null)
                    continue;

                qd.AddQuest(quest);

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

            if (questLine.Stages.Count == 0)
                return null;

            return questLine;
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

        protected virtual void GenerateLevelingQuests(DiscoveryQuestsDatabase qd)
        {
            var database = Realm?.Database ?? SfaDatabase.Instance;
            var groups = Realm.Database.LevelQuests.Concat(RelictShipLevelingQuests)
                .GroupBy(x => x.Faction);

            foreach (var group in groups)
            {
                var faction = group.Key;
                var idInfo = new QuestIdInfo
                {
                    Type = QuestType.MainQuestLine,
                    Faction = faction,
                    IsLevelingQuest = true,
                    LocalId = 0
                };

                foreach (var levelInfo in group.OrderBy(q => q.Level))
                {
                    foreach (var logicId in levelInfo.Logics)
                    {
                        idInfo.LocalId++;

                        var logic = Realm.Database.GetQuestLogic(logicId);
                        var accessLevel = SfaDatabase.LevelToAccessLevel(levelInfo.Level);
                        var startingSystem = Realm.GalaxyMap.GetStartingSystem(faction);
                        var targetSystem = ExtraMap.GetCircle(accessLevel).GetStartSystem(faction);
                        var obj = startingSystem.Motherships?.FirstOrDefault() ??
                                  startingSystem.GetObjectsWithTaskBoard()?.FirstOrDefault();

                        var quest = new DiscoveryQuest()
                        {
                            Id = idInfo.ToId(),
                            LogicId = logic.Id,
                            LogicName = logic.UniqueLogicIdentifier,
                            Type = logic.Type,
                            Level = levelInfo.Level,
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
                            TargetLevel = accessLevel,
                            TargetSystemId = targetSystem.Id,
                            TargetFaction = faction,
                        };

                        foreach (var item in logic.Conditions)
                        {
                            if (GenerateQuestCondition(item, context) is JsonObject condition)
                                quest.Conditions.Add(condition);
                            else
                                break;
                        }

                        if (logic.Conditions.Count > quest.Conditions.Count)
                            continue;

                        var reward = logic.Rewards.FirstOrDefault();

                        reward.Items = (reward.Items ?? new()).Select(i => new QuestRevardItemInfo()
                        {
                            Id = i.Id,
                            Count = i.Count * 4
                        }).ToList();

                        quest.Reward = reward;
                        qd.AddLevelingQuest(quest);
                    }
                }
            }
        }

        public DiscoveryQuest GenerateDailyTask(QuestLogicInfo info)
        {
            if (info is null || info.Type is not QuestType.Daily)
                return null;

            var level = 1;

            if (info.UniqueLogicIdentifier is string logicId &&
                logicId.Length > 0 &&
                logicId[^1..] is string levelString &&
                int.TryParse(levelString, out int parsedLevel) == true)
                level = Math.Max(1, parsedLevel);

            var quest = new DiscoveryQuest()
            {
                Id = new QuestIdInfo
                {
                    Type = info.Type,
                    LocalId = Rnd.Next(0, 10000),
                }.ToId(),
                LogicId = info.Id,
                LogicName = info.UniqueLogicIdentifier,
                Type = info.Type,
                Reward = new(),
                ObjectSystem = -1,
                ObjectType = GalaxyMapObjectType.None,
                ObjectId = -1,
                Level = level,
                ObjectFaction = Faction.None,
                Conditions = new JsonArray(),
            };

            var context = new QuestContext
            {
                Quest = quest,
                MobsDatabase = Realm.MobsDatabase,
                MobsMap = Realm.MobsMap,
                TargetLevel = quest.Level,
                TargetSystemId = quest.ObjectSystem,
                TargetFaction = quest.ObjectFaction,
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

            quest.Reward = info.Rewards?.FirstOrDefault() ?? new();

            return quest;
        }

        public DiscoveryQuest GenerateHouseTask(string identity, int systemId)
        {
            if (identity is null)
                return null;

            var system = Realm?.GalaxyMap?.GetSystem(systemId);
            var info = Realm?.Database?.QuestsLogics
                .Where(l => l.Value?.Type == QuestType.HouseTask &&
                            l.Value.UniqueLogicIdentifier?.Equals(identity, StringComparison.OrdinalIgnoreCase) == true)
                .ToList()
                .Randomize(Rnd.Next())
                .FirstOrDefault().Value;

            if (system is null || info is null)
                return null;

            var quest = new DiscoveryQuest()
            {
                Id = new QuestIdInfo
                {
                    Type = info.Type,
                    LocalId = Rnd.Next(0, 10000),
                }.ToId(),
                LogicId = info.Id,
                LogicName = info.UniqueLogicIdentifier,
                Type = info.Type,
                Reward = new(),
                ObjectSystem = systemId,
                ObjectType = GalaxyMapObjectType.None,
                ObjectId = Rnd.Next(0, 10000),
                Level = system.Level,
                ObjectFaction = Faction.None,
                Conditions = new JsonArray(),
            };

            var context = new QuestContext
            {
                Quest = quest,
                MobsDatabase = Realm.MobsDatabase,
                MobsMap = Realm.MobsMap,
                TargetLevel = quest.Level,
                TargetSystemId = quest.ObjectSystem,
                TargetFaction = quest.ObjectFaction,
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

            quest.Reward = info.Rewards.FirstOrDefault().Combine(GenerateRewardForTaskBoardQuest(quest));

            return quest;
        }

        protected virtual void GenerateTaskBoardQuests(DiscoveryQuestsDatabase qd)
        {
            var logics = Realm.Database.QuestsLogics;
            var idInfo = new QuestIdInfo { Type = QuestType.Task, LocalId = 0 };


            if (logics is not null)
            {
                foreach (var logic in logics.Values)
                {
                    if (logic is null || logic.Type != QuestType.Task)
                        continue;

                    foreach (var item in GetObjectsForQuestLogic(logic))
                    {
                        var chance = item.Type is GalaxyMapObjectType.Planet ? 15 : 10;
                        idInfo.LocalId++;

                        if ((Rnd.Next() % chance) != 0 ||
                            qd.GetQuests(QuestType.Task, (byte)item.Type, item.Id).Count() >= 6)
                            continue;

                        var quest = GenerateQuestForTaskBoard(logic, item, idInfo.ToId());

                        if (quest is not null)
                        {
                            qd.AddQuest(quest);
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

            quest.Reward = info.Rewards.FirstOrDefault().Combine(GenerateRewardForTaskBoardQuest(quest));

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
                    result = GenExploreSystemObjectCondition(context, condition, info);
                    break;
                case QuestConditionType.StatTracking:
                    result = GenStatTrackingCondition(context, condition, info);
                    break;
                case QuestConditionType.Client:
                    //Tutorial
                    break;
                case QuestConditionType.ExploreRelictShip:
                    result = GenExploreRelictShipCondition(context, condition, info);
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
                            where system.Level == info.TargetLevel &&
                                  system.Planets != null
                            from planet in system.Planets
                            where planet.Faction is not Faction.None or Faction.Other
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = planet.ObjectType,
                                Id = planet.Id,
                                Level = system.Level,
                                Faction = planet.Faction
                            })
                            yield return item;
                        yield break;

                    case "core_trade_station":
                    case "addon_trade_station":
                        foreach (var item in
                            from system in Realm.GalaxyMap.Systems
                            where system.Level == info.TargetLevel &&
                                  system.TradeStations != null
                            from station in system.TradeStations
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = station.ObjectType,
                                Id = station.Id,
                                Level = system.Level,
                                Faction = Faction.FreeTraders,
                            })
                            yield return item;
                        yield break;

                    case "core_science_station":
                        foreach (var item in
                            from system in Realm.GalaxyMap.Systems
                            where system.Level == info.TargetLevel &&
                                  system.ScienceStations != null
                            from station in system.ScienceStations
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = station.ObjectType,
                                Id = station.Id,
                                Level = system.Level,
                                Faction = Faction.Scientists,
                            })
                            yield return item;
                        yield break;

                    case "core_miner_station":
                        foreach (var item in
                            from system in Realm.GalaxyMap.Systems
                            where system.Level == info.TargetLevel &&
                                  system.MinerMotherships != null
                            from station in system.MinerMotherships
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = station.ObjectType,
                                Id = station.Id,
                                Level = system.Level,
                                Faction = Faction.MineworkerUnion,
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
                            where info.TargetLevel < 0 ? true : system.Level == info.TargetLevel
                            let objects = system.GetObjectsWithTaskBoard()
                            where objects != null
                            from obj in objects
                            where obj.ObjectType != GalaxyMapObjectType.Planet ||
                                  obj is GalaxyMapPlanet { Faction: not (Faction.None or Faction.Other) }
                            select new StarSystemObjectInfo
                            {
                                SystemId = system.Id,
                                Type = obj.ObjectType,
                                Id = obj.Id,
                                Level = system.Level,
                                Faction = system.Faction
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
                "kill_group_ships",
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
