using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public partial class SfaDatabase
    {
        public Dictionary<int, Blueprint> Blueprints { get; } = new();

        public Dictionary<int, ShipBlueprint> Ships { get; } = new();

        public Dictionary<int, EquipmentBlueprint> Equipments { get; } = new();

        public Dictionary<int, DiscoveryItem> DiscoveryItems { get; } = new();

        public Dictionary<int, ShipSkin> Skins { get; } = new();

        public Dictionary<int, string> SkinColors { get; } = new();

        public Dictionary<int, ShipDecal> Decals { get; } = new();

        public Dictionary<int, LevelInfo> Levels { get; } = new();

        public Dictionary<int, QuestLogicInfo> QuestsLogics { get; } = new();

        public Dictionary<int, QuestLineInfo> QuestLines { get; } = new();

        public List<LevelQuestInfo> LevelQuests { get; } = new();

        public Dictionary<int, AbilityInfo> Abilities { get; } = new();

        public TagNode MobTags { get; } = new();

        public Dictionary<int, SfaCircleData> CircleDatabase { get; } = new()
        {
            { 0, new SfaCircleData(0) },
            { 1, new SfaCircleData(1) },
            { 2, new SfaCircleData(2) },
            { 3, new SfaCircleData(3) },
            { 4, new SfaCircleData(4) },
            { 5, new SfaCircleData(5) },
            { 6, new SfaCircleData(6) },
            { 7, new SfaCircleData(7) },
        };

        private static readonly Lazy<SfaDatabase> _lazyInstance = new(LoadDatabase);

        public static SfaDatabase Instance => _lazyInstance.Value;

        protected static SfaDatabase LoadDatabase()
        {
            var dtb = new SfaDatabase();
            var databasePath = Path.Combine("Database", "database.json");
            JsonNode doc;

            try
            {
                doc = JsonNode.Parse(File.ReadAllText(databasePath));
            }
            catch
            {
                return dtb;
            }

            if (doc is not JsonObject)
                return dtb;


            if (doc["blueprints"]?.AsArray() is JsonArray blueprints)
            {
                foreach (var item in blueprints)
                {
                    var type = (TechType?)(byte?)item["techtype"] ?? TechType.Unknown;
                    var info = JsonNode.Parse((string)item["additionalparams"]);

                    if (type == TechType.Ship)
                    {
                        var ship = new ShipBlueprint(item, info);
                        dtb.Blueprints.Add(ship.Id, ship);
                        dtb.Ships.Add(ship.Id, ship);
                    }
                    else
                    {
                        var equipment = new EquipmentBlueprint(item, info);
                        dtb.Blueprints.Add(equipment.Id, equipment);
                        dtb.Equipments.Add(equipment.Id, equipment);

                        for (int i = equipment.MinLvl; i <= equipment.MaxLvl; i++)
                            if (dtb.CircleDatabase.TryGetValue(i, out var circle) == true)
                                circle.Equipments.Add(equipment.Id, equipment);
                    }
                }
            }

            if (doc["discovery_items"]?.AsArray() is JsonArray discoveryItems)
            {
                var itemsForProduction = dtb.Blueprints.Where(b =>
                    b.Value is SfaItem item &&
                    item.TechLvl != 0 &&
                    item.GalaxyValue != 0 &&
                    item.IsBoundToCharacter == false &&
                    item.IsRareShopItem == false &&
                    item.IsUniqueReward == false)
                    .ToList();

                foreach (var item in discoveryItems)
                {
                    var discoveryItem = new DiscoveryItem(
                        item, JsonHelpers.ParseNodeUnbuffered((string)item["additionalparams"]));

                    discoveryItem.ProductionFrequency += itemsForProduction
                        .SelectMany(b => b.Value.Materials ?? new())
                        .Where(i => i.Id == discoveryItem.Id)
                        .Count();

                    discoveryItem.DisassemblyFrequency += itemsForProduction
                        .SelectMany(b => b.Value.DisassembleMaterialsDrop ?? new())
                        .Where(i => i.Id == discoveryItem.Id)
                        .Count();

                    dtb.DiscoveryItems.Add(discoveryItem.Id, discoveryItem);

                    var minLvl = discoveryItem.MinLvl;
                    var maxLvl = discoveryItem.MaxLvl;

                    if (dtb.GetItem(discoveryItem.ProductItem) is SfaItem product)
                    {
                        minLvl = Math.Max(minLvl, product.MinLvl);
                        maxLvl = Math.Max(maxLvl, product.MaxLvl);
                    }

                    for (int i = minLvl; i <= maxLvl; i++)
                        if (dtb.CircleDatabase.TryGetValue(i, out var circle) == true)
                            circle.DiscoveryItems.Add(discoveryItem.Id, discoveryItem);
                }
            }

            if (doc["shipskins"]?.AsArray() is JsonArray shipSkins)
            {
                foreach (var item in shipSkins)
                {
                    var skin = new ShipSkin(item);
                    dtb.Skins.Add(skin.Id, skin);
                }
            }

            if (doc["skincolor"]?.AsArray() is JsonArray colors)
            {
                foreach (var item in colors)
                {
                    dtb.SkinColors.TryAdd(
                        (int?)item["id"] ?? -1,
                        (string)item["name"]);
                }
            }

            if (doc["decals"]?.AsArray() is JsonArray decals)
            {
                foreach (var item in decals)
                {
                    int id = (int?)item["id"] ?? -1;

                    dtb.Decals.TryAdd(id, new()
                    {
                        Id = id,
                        Name = (string)item["name"],
                        IsPrenium = (int?)item["is_premium"] == 1,
                    });
                }
            }

            if (doc["quest_logics"]?.AsArray() is JsonArray quests)
            {
                foreach (var item in quests)
                {
                    var logic = new QuestLogicInfo(item);

                    if (logic.Id > 0)
                        dtb.QuestsLogics.TryAdd(logic.Id, logic);
                }
            }

            if (doc["character_leveling"]?["levels"]?.AsArray() is JsonArray charLevels)
            {
                var levelsCount = charLevels.Count;
                var totalAbilityCells = 0;
                var currentAccessLevel = 0;

                for (int i = 0; i < levelsCount; i++)
                {
                    if (charLevels.FirstOrDefault(l => (int?)l?["level"] == i) is JsonObject levelDoc)
                    {
                        var level = new LevelInfo
                        {
                            Level = i,
                            Xp = (int?)levelDoc["xp_for_level"] ?? 0,
                            NewAbilityCells = (int?)levelDoc["ability_cells"] ?? 0,
                            NewAccessLevel = (int?)levelDoc["access_level"] ?? 0,
                        };

                        totalAbilityCells += level.NewAbilityCells;
                        currentAccessLevel = Math.Max(currentAccessLevel, level.NewAccessLevel);
                        level.AbilityCells = totalAbilityCells;
                        level.AccessLevel = currentAccessLevel;
                        dtb.Levels[i] = level;
                    }
                }
            }

            if (doc["quest_lines"]?.AsArray() is JsonArray questLines)
            {
                foreach (var line in questLines)
                {
                    if (line is not JsonObject)
                        continue;

                    var info = new QuestLineInfo
                    {
                        Id = (int?)line["id"] ?? -1,
                        Name = (string)line["name"],
                        Logics = new(),

                    };

                    if (info.Name is string name)
                    {
                        var tags = name.Split('_');

                        info.Faction = Enum.TryParse(
                            tags.ElementAtOrDefault(0),true, out Faction faction) ?
                            faction : Faction.None;

                        info.Type = tags.ElementAtOrDefault(1) switch
                        {
                            null => QuestType.Task,
                            var type when type.Equals("main", StringComparison.InvariantCultureIgnoreCase) => QuestType.MainQuestLine,
                            var type when type.Equals("yoba", StringComparison.InvariantCultureIgnoreCase) => QuestType.UniqueQuestLine,
                            _ => QuestType.Task
                        };

                        if (info.Type == QuestType.UniqueQuestLine)
                        {
                            info.TargetFaction = Enum.TryParse(
                                tags.ElementAtOrDefault(2), true, out Faction targetFaction) ?
                                targetFaction : Faction.None;
                        }
                    }

                    foreach (var logic in line["logics"]?.AsArray() ?? new())
                    {
                        if (logic is not JsonObject)
                            continue;

                        var genParams = JsonNode.Parse((string)logic["gen_params"]);

                        info.Logics.Add(new()
                        {
                            Position = (int?)logic["pos"] ?? -1,
                            LogicId = (int?)logic["logic_id"] ?? -1,
                            AccessLevel = (int?)genParams?["access_level"] ?? 1,
                            UniqueMobGroup = (int?)genParams?["unique_mob_group"] ?? -1,
                        });
                    }

                    if (info.Logics.Count > 0)
                        dtb.QuestLines.Add(info.Id, info);
                }
            }

            if (doc["charact_level_quests"]?.AsArray() is JsonArray levelQuests)
            {
                foreach (var quest in levelQuests)
                {
                    if ((int?)quest["level"] is int level &&
                        (Faction?)(byte?)quest["faction"] is Faction faction &&
                        quest["logics"]?.DeserializeUnbuffered<List<int>>() is List<int> logics)
                        dtb.LevelQuests.Add(new() { Level = level, Faction = faction, Logics = logics });

                }
            }

            if (doc["mob_tags"]?.AsArray() is JsonArray mobTags)
            {
                foreach (var tagNode in mobTags)
                {
                    if ((string)tagNode is string tag)
                        dtb.MobTags.AddTag(tag);
                }
            }

            if (doc["character_abilities"]?["abilities"]?.AsArray() is JsonArray abilities)
            {
                foreach (var ability in abilities)
                {
                    if ((int?)ability["id"] is int id)
                    {
                        var info = new AbilityInfo
                        {
                            Id = id,
                            Logic = (AbilityLogic?)(byte?)ability["logic"] ?? AbilityLogic.Unknown,
                            TargetType = (AbilityTargetType?)(byte?)ability["target_type"] ?? AbilityTargetType.Passive,
                        };

                        if (ability["additionalparams"] is JsonObject additionalParams)
                        {
                            if (additionalParams["params"] is JsonObject logicParams)
                            {
                                info.Cooldown = (float?)logicParams["cooldown"] ?? 0;
                                info.AgroVision = (float?)logicParams["agrovision"] ?? 0;
                                info.NebulaVision = (float?)logicParams["nebula_vision"] ?? 0;
                                info.SensorRadius = (float?)logicParams["radius"] ?? 0;
                            }
                            
                            if (additionalParams["effects"]?.AsArray() is JsonArray effects)
                            {
                                var abilityEffects = new List<FleetEffectInfo>();

                                foreach(var effect in effects)
                                {
                                    var fleetEffect = new FleetEffectInfo
                                    {
                                        Logic = (GameplayEffectType?)(int?)effect["logic"] ?? GameplayEffectType.Unknown,
                                        Duration = (float?)effect["duration"] ?? 0,
                                    };

                                    if (effect["params"] is JsonObject effectParams)
                                    {
                                        fleetEffect.EngineBoost = (float?)effectParams["boost"] ?? 0;
                                        fleetEffect.Vision = (int?)(double?)effectParams["vision"] ?? 0;
                                        fleetEffect.NebulaVision = (int?)(double?)effectParams["nebula_vision"] ?? 0;
                                    }

                                    abilityEffects.Add(fleetEffect);
                                }

                                info.Effects = abilityEffects;
                            }
                        }

                        dtb.Abilities[id] = info;
                    }
                }
            }

            return dtb;
        }

        public int GetShipCargo(int shipId)
        {
            if (Ships?.TryGetValue(shipId, out ShipBlueprint ship) == true && ship is not null)
                return ship.CargoHoldSize;

            return 0;
        }

        public string GetShipName(int shipId)
        {
            if (Ships?.TryGetValue(shipId, out ShipBlueprint ship) == true && ship is not null)
                return ship.Name;

            return null;
        }

        public ShipBlueprint GetShip(int shipId)
        {
            if (Ships?.TryGetValue(shipId, out ShipBlueprint ship) == true)
                return ship;

            return null;
        }

        public AbilityInfo? GetAbility(int abilityId)
        {
            if (Abilities?.TryGetValue(abilityId, out AbilityInfo ability) == true)
                return ability;

            return default;
        }

        public int CalculateUsedCargoSpace(ICollection<InventoryItem> items)
            => items?.Sum(i => (GetItem(i.Id)?.Cargo ?? 0) * i.Count) ?? 0;

        public SfaItem GetItem(int id)
        {
            if (Blueprints?.TryGetValue(id, out Blueprint blueprint) == true)
                return blueprint;
            else if (DiscoveryItems?.TryGetValue(id, out DiscoveryItem item) == true)
                return item;

            return null;
        }

        public SfaCircleData GetCircleDatabase(int lvl)
        {
            if (CircleDatabase?.TryGetValue(lvl, out var circle) == true)
                return circle;

            return null;
        }

        public QuestLogicInfo GetQuestLogic(int id)
        {
            if (QuestsLogics?.TryGetValue(id, out QuestLogicInfo logic) == true)
                return logic;

            return null;
        }

        public QuestLineInfo GetQuestLine(int id)
        {
            if (QuestLines?.TryGetValue(id, out QuestLineInfo line) == true)
                return line;

            return null;
        }

        public static int LevelToAccessLevel(int level) => level switch
        {
            >= 85 => 7,
            >= 71 => 6,
            >= 57 => 5,
            >= 43 => 4,
            >= 29 => 3,
            >= 15 => 2,
            _ => 1
        };

        public static int AccessLevelToQuestBGReward(int level) => level switch
        {
            <= 1 => 2,
            2 => 4,
            3 => 6,
            4 => 10,
            5 => 15,
            6 => 20,
            _ => 25
        };

        public static int GetCircleMinLevel(int circle) => circle switch
        {
            <= 1 => 1,
            2 => 15,
            3 => 29,
            4 => 43,
            5 => 57,
            6 => 71,
            _ => 85
        };

        public static int GetCircleMaxLevel(int circle) => circle switch
        {
            <= 1 => 14,
            2 => 28,
            3 => 42,
            4 => 56,
            5 => 70,
            6 => 84,
            _ => 100
        };

        public static int GetWarpingCost(int circle) => circle switch
        {
            <= 1 => 100,
            2 => 250,
            3 => 500,
            4 => 750,
            5 => 1000,
            6 => 1250,
            _ => 1500
        };


        public static int GetRefuelCost(int circle) => circle switch
        {
            <= 1 => 50,
            2 => 75,
            3 => 100,
            4 => 200,
            5 => 300,
            6 => 400,
            _ => 500
        };

        public int GetLevelForShipXp(int shipId, int shipXp) =>
            GetLevelForShipXp(GetShip(shipId), shipXp);

        public static int GetLevelForShipXp(ShipBlueprint ship, int shipXp)
        {
            var levels = ship?.Levels;
            float levelTotalXp = 0;
            var currentLevel = 0;

            if (levels is null ||
                levels.Count < 1)
                return 0;

            foreach (var item in levels)
            {
                levelTotalXp += item;

                if (item > shipXp)
                    return Math.Max(0, currentLevel - 1);

                currentLevel++;
            }

            return Math.Max(0, currentLevel - 1);
        }


        public LevelInfo GetLevelInfo(int lvl)
        {
            if (Levels?.TryGetValue(lvl, out LevelInfo info) == true)
                return info;

            return null;
        }


        public LevelInfo GetLevelInfoForCharXp(int xp)
        {
            LevelInfo currentLevel = null;

            if (Levels.Count < 1)
                return null;

            for (int i = 0; i < Levels.Count; i++)
            {
                currentLevel = GetLevelInfo(i);

                if (currentLevel.Xp > xp)
                    return GetLevelInfo(Math.Max(0, i - 1));
            }

            return currentLevel;
        }
    }
}
