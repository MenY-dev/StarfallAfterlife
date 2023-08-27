using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.Database
{
    //[JsonConverter(typeof(QuestConditionInfoJsonConverter))]
    public struct QuestConditionInfo : ICloneable
    {
        [JsonInclude, JsonPropertyName("identity")]
        public string Identity;

        [JsonInclude, JsonPropertyName("type")]
        public QuestConditionType Type;

        [JsonInclude, JsonPropertyName("progress_require")]
        public int ProgressRequire;

        [JsonInclude, JsonPropertyName("jumps_to_system")]
        public int JumpsToSystem;

        [JsonInclude, JsonPropertyName("quest_value_factor")]
        public float QuestValueFactor;

        [JsonInclude, JsonPropertyName("select_group")]
        public int SelectGroup;

        [JsonInclude, JsonPropertyName("faction")]
        public Faction Faction;

        [JsonInclude, JsonPropertyName("factions")]
        public List<Faction> Factions;

        [JsonInclude, JsonPropertyName("ship_class")]
        public List<int> ShipClass;

        [JsonInclude, JsonPropertyName("min_target_level")]
        public int MinTargetLevel;

        [JsonInclude, JsonPropertyName("item_to_deliver")]
        public int ItemToDeliver;

        [JsonInclude, JsonPropertyName("drop_chance")]
        public float DropChance;

        [JsonInclude, JsonPropertyName("TaskStarSystemLevel")]
        public int TaskStarSystemLevel;

        [JsonInclude, JsonPropertyName("level_to_reach")]
        public int LevelToReach;

        [JsonInclude, JsonPropertyName("ObjectType")]
        public int ObjectType;

        [JsonInclude, JsonPropertyName("planet_types")]
        public List<int> PlanetTypes;

        [JsonInclude, JsonPropertyName("need_specific_object")]
        public bool NeedSpecificObject;

        [JsonInclude, JsonPropertyName("hidden_loc_params")]
        public string HiddenLocParams;

        [JsonInclude, JsonPropertyName("max_jumps_to_system")]
        public int MaxJumpsToSystem;

        [JsonInclude, JsonPropertyName("gen_in_same_system")]
        public bool GenInSameSystem;

        [JsonInclude, JsonPropertyName("SystemLevel")]
        public int SystemLevel;

        [JsonInclude, JsonPropertyName("PossibleSystemsCount")]
        public int PossibleSystemsCount;

        [JsonInclude, JsonPropertyName("stat_tag")]
        public string StatTag;

        [JsonInclude, JsonPropertyName("boss_faction")]
        public Faction BossFaction;

        [JsonInclude, JsonPropertyName("SuitableEntities")]
        public List<int> SuitableEntities;

        [JsonInclude, JsonPropertyName("InstanceId")]
        public string InstanceId;

        [JsonInclude, JsonPropertyName("CustomInstanceType")]
        public int CustomInstanceType;

        [JsonInclude, JsonPropertyName("EventId")]
        public string EventId;

        [JsonInclude, JsonPropertyName("spawn_pirate_in_userfleet_system")]
        public int SpawnPirateInUserFleetSystem;

        [JsonInclude, JsonPropertyName("count_mobs_only_in_target_system")]
        public int CountMobsOnlyInTargetSystem;

        [JsonInclude, JsonPropertyName("items")]
        public List<QuestItemInfo> Items;

        public QuestConditionInfo(JsonNode doc)
        {
            if (doc is null)
                return;

            Identity = (string)doc["identity"] ?? string.Empty;
            Type = (QuestConditionType?)(byte?)doc["type"] ?? QuestConditionType.None;
            ProgressRequire = (int?)doc["progress_require"] ?? 0;
            JumpsToSystem = (int?)doc["jumps_to_system"] ?? 0;
            QuestValueFactor = (float?)doc["quest_value_factor"] ?? 1;
            SelectGroup = (int?)doc["select_group"] ?? 0;
            Faction = (Faction?)(byte?)doc["faction"] ?? Faction.None;
            Factions = new();

            foreach (var item in doc["factions"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
                if ((byte?)item is byte faction)
                    Factions.Add((Faction)faction);

            ShipClass = new();

            foreach (var item in doc["ship_class"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
                if ((byte?)item is byte shipClass)
                    Factions.Add((Faction)shipClass);

            MinTargetLevel = (int?)doc["min_target_level"] ?? 0;
            ItemToDeliver = (int?)doc["item_to_deliver"] ?? 0;
            DropChance = (float?)doc["drop_chance"] ?? 0;
            TaskStarSystemLevel = (int?)doc["TaskStarSystemLevel"] ?? 0;
            LevelToReach = (int?)doc["level_to_reach"] ?? 0;
            ObjectType = (int?)doc["object_type"] ?? (int?)doc["ObjectType"] ?? 0;
            PlanetTypes = new();

            foreach (var item in doc["planet_types"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
                if ((byte?)item is byte type)
                    Factions.Add((Faction)type);

            NeedSpecificObject = (bool?)doc["need_specific_object"] ?? false;
            HiddenLocParams = (string)doc["hidden_loc_params"] ?? string.Empty;
            MaxJumpsToSystem = (int?)doc["max_jumps_to_system"] ?? 0;
            GenInSameSystem = (bool?)doc["gen_in_same_system"] ?? false;
            SystemLevel = (int?)doc["SystemLevel"] ?? 0;
            PossibleSystemsCount = (int?)doc["PossibleSystemsCount"] ?? 0;
            StatTag = (string)doc["stat_tag"] ?? string.Empty;
            BossFaction = (Faction?)(byte?)doc["boss_faction"] ?? Faction.None;
            SuitableEntities = new();

            foreach (var item in doc["SuitableEntities"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
                if ((int?)item is int entity && entity > 0)
                    SuitableEntities.Add(entity);

            InstanceId = (string)doc["InstanceId"];
            CustomInstanceType = (int?)doc["CustomInstanceType"] ?? 0;
            EventId = (string)doc["EventId"] ?? string.Empty;
            SpawnPirateInUserFleetSystem = (int?)doc["spawn_pirate_in_userfleet_system"] ?? 0;
            CountMobsOnlyInTargetSystem = (int?)doc["count_mobs_only_in_target_system"] ?? 0;
            Items = new();
            foreach (var item in doc["items"]?.AsArray() ?? Enumerable.Empty<JsonNode>())
            {
                var type = (byte?)item["type"] ?? 0;
                var id = (int?)item["id"] ?? 0;

                if (id < 1)
                    continue;

                Items.Add(new() { Id = id, Type = type });
            }
        }

        object ICloneable.Clone() => Clone();

        public QuestConditionInfo Clone()
        {
            var c = this;

            c.Factions = Factions?.ToList() ?? new();
            c.ShipClass = ShipClass?.ToList() ?? new();
            c.PlanetTypes = PlanetTypes?.ToList() ?? new();
            c.SuitableEntities = SuitableEntities?.ToList() ?? new();
            c.Items = Items?.ToList() ?? new();

            return c;
        }

        public class QuestConditionInfoJsonConverter : JsonConverter<QuestConditionInfo>
        {
            private static readonly Dictionary<QuestConditionType, string[]> Fields = new()
            {
                { QuestConditionType.Client, new string[] { "EventId" } },
                { QuestConditionType.CompleteSpecialInstance, new string[] { "InstanceId", "CustomInstanceType" } },
                { QuestConditionType.CompleteTask, new string[] { "TaskStarSystemLevel" } },
                { QuestConditionType.DeliverItems, new string[] { "item_to_deliver" } },
                { QuestConditionType.DeliverMobDrop, new string[] { "item_to_deliver", "drop_chance" } },
                { QuestConditionType.DeliverQuestItem, new string[] { "item_to_deliver" } },
                { QuestConditionType.DeliverRandomItems, new string[] { "item_to_deliver", "items" } },
                { QuestConditionType.ExploreObject, new string[] { "ObjectType", "SystemLevel", "PossibleSystemsCount" } },
                { QuestConditionType.ExploreRelictShip, new string[] { } },
                { QuestConditionType.ExploreSecretLocation, new string[] { } },
                { QuestConditionType.ExploreSystem, new string[] { "jumps_to_system" } },
                { QuestConditionType.InstanceEvent, new string[] { "difficulty_value" } },
                { QuestConditionType.InterceptPersonalMob, new string[] { } },
                { QuestConditionType.InterceptShip, new string[] { } },
                { QuestConditionType.KillBoss, new string[] { "boss_faction" } },
                { QuestConditionType.KillBossOfAnyFactiont, new string[] { "boss_faction" } },
                { QuestConditionType.KillFactionGroupShip, new string[] { "specified_group_faction" } },
                { QuestConditionType.KillFleet, new string[] { } },
                { QuestConditionType.KillMobShip, new string[] { "count_mobs_only_in_target_system" } },
                { QuestConditionType.KillPiratesOutpost, new string[] { "select_group", "faction" } },
                { QuestConditionType.KillPiratesStation, new string[] { "faction" } },
                { QuestConditionType.KillShip, new string[] { "factions", "ship_class", "min_target_level" } },
                { QuestConditionType.KillUniquePersonalPirateShip, new string[] { "spawn_pirate_in_userfleet_system" } },
                { QuestConditionType.KillUniquePirateShip, new string[] { } },
                { QuestConditionType.MineQuestItem, new string[] { "item_to_deliver" } },
                { QuestConditionType.None, new string[] { } },
                { QuestConditionType.PickUpAndDeliverQuestItem, new string[] { "item_to_deliver" } },
                { QuestConditionType.ReachCharacterLevel, new string[] { "level_to_reach" } },
                { QuestConditionType.ResearchProject, new string[] { "SuitableEntities" } },
                { QuestConditionType.ScanSystemObject, new string[] { "object_type", "planet_types", "need_specific_object", } },
                { QuestConditionType.ScanUnknownPlanet, new string[] { "object_type", "planet_types", "need_specific_object",
                                                                      "max_jumps_to_system", "gen_in_same_system", "hidden_loc_params", } },
                { QuestConditionType.StatTracking, new string[] { "stat_tag", } },
            };

            public override QuestConditionInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return JsonSerializer.Deserialize<QuestConditionInfo>(ref reader, options);
            }

            public override void Write(Utf8JsonWriter writer, QuestConditionInfo value, JsonSerializerOptions options)
            {
                var elementOptions = new JsonSerializerOptions(options);
                elementOptions.Converters.Remove(this);
                var element = JsonSerializer.SerializeToElement(value);

                writer.WriteString("identity", value.Identity);
                writer.WriteNumber("type", (byte)value.Type);
                writer.WriteNumber("progress_require", value.ProgressRequire);
                writer.WriteNumber("quest_value_factor", value.QuestValueFactor);

                if (element.ValueKind != JsonValueKind.Object)
                    return;

                writer.WriteStartArray();

                if (Fields.TryGetValue(value.Type, out var fields) == true)
                {
                    foreach (var item in element.EnumerateObject())
                        if (fields.Contains(item.Name))
                            item.WriteTo(writer);
                }
            }
        }
    }
}
