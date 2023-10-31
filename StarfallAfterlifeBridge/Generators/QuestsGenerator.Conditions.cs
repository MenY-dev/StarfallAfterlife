using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public partial class QuestsGenerator
    {
        protected bool GenExploreSystemCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsAtDistance(context.TargetSystemId, info.JumpsToSystem, true)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedSystems = context.Quest.Conditions?
                .Select(q => (int?)q["system_to_explore"])
                .Where(i => i is not null)
                .ToList();

            GalaxyMapStarSystem systemToExplore = null;

            foreach (var item in systems)
            {
                if (item is not null && addedSystems.Contains(item.Id) == false)
                {
                    systemToExplore = item;
                    break;
                }
            }

            if (systemToExplore is null)
                return false;

            condition["system_to_explore"] = systemToExplore.Id;

            return true;
        }

        protected bool GenScanUnknownPlanetCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, info.MaxJumpsToSystem, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedPlanets = context.Quest.Conditions?
                .Select(q => (int?)q["target_object_id"])
                .Where(i => i is not null)
                .ToList();

            foreach (var system in systems)
            {
                if (system?.Planets is List<GalaxyMapPlanet> planets)
                {
                    foreach (var planet in planets)
                    {
                        if (planet is not null &&
                            planet.Faction is Faction.None &&
                            addedPlanets.Contains(planet.Id) == false)
                        {
                            condition["target_object_id"] = planet.Id;
                            condition["target_system"] = system.Id;
                            condition["target_location"] = context.Quest.Id;
                            condition["hidden_loc_params"] = info.HiddenLocParams;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected bool GenScanSystemObjectCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, info.MaxJumpsToSystem, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedObjects = context.Quest.Conditions?
                .Select(q => (
                    System: (int?)q["target_system"],
                    Id: (int?)q["target_object_id"],
                    Type: (int?)q["target_object_type"]))
                .Where(o => o.System is not null && o.Id is not null)
                .ToList();

            if (info.ObjectType == (int)GalaxyMapObjectType.RichAsteroids)
            {
                var targetSystem = Realm.GalaxyMap.GetSystem(addedObjects?
                    .Where(o => o.Type == (int)GalaxyMapObjectType.RichAsteroids)
                    .Select(o => o.System)
                    .FirstOrDefault() ?? -1);

                bool ProcessSystemAsreroids(GalaxyMapStarSystem system)
                {
                    if (system?.RichAsteroids is List<GalaxyMapRichAsteroid> asteroids)
                    {
                        foreach (var asteroid in asteroids)
                        {
                            if (asteroid is not null &&
                                addedObjects.Any(o =>
                                    o.Type == (int)GalaxyMapObjectType.RichAsteroids &&
                                    o.Id == asteroid.Id) == false)
                            {
                                condition["target_system"] = system.Id;
                                condition["target_object_id"] = asteroid.Id;
                                condition["target_object_type"] = (int)GalaxyMapObjectType.RichAsteroids;
                                return true;
                            }
                        }
                    }

                    return false;
                }

                if (targetSystem is null)
                {
                    foreach (var system in systems)
                    {
                        if (system is null)
                            continue;

                        var conditionsCount = Realm.Database.GetQuestLogic(context.Quest.LogicId)?.Conditions.Count ?? 0;

                        if (conditionsCount > 0 &&
                            (system.RichAsteroids?.Count ?? 0) >= conditionsCount &&
                            ProcessSystemAsreroids(system) == true)
                            return true;
                    }
                }
                else
                {
                    if (ProcessSystemAsreroids(targetSystem) == true)
                        return true;
                }
            }
            else if (info.ObjectType == (int)GalaxyMapObjectType.Planet)
            {
                foreach (var system in systems)
                {
                    if (system?.Planets is List<GalaxyMapPlanet> planets)
                    {
                        foreach (var planet in planets)
                        {
                            if (planet is not null &&
                                planet.Faction is Faction.None &&
                                addedObjects.Any(o =>
                                    o.Type == (int)GalaxyMapObjectType.Planet &&
                                    o.Id == planet.Id) == false)
                            {
                                condition["target_system"] = system.Id;
                                condition["target_object_id"] = planet.Id;
                                condition["target_object_type"] = (int)GalaxyMapObjectType.Planet;
                                condition["planet_types"] = JsonHelpers.ParseNodeUnbuffered(info.PlanetTypes);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        protected bool GenDeliverRandomItemsCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var items = info.Items?.Select(i => i.Id)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedItems = context.Quest.Conditions?
                .Select(q => (int?)q["item_to_deliver"])
                .Where(i => i is not null)
                .ToList();

            foreach (var item in items)
            {
                if (addedItems.Contains(item) == true ||
                    (Realm?.Database?.GetItem(item) is var itemInfo &&
                    itemInfo.DisassemblyFrequency < 2))
                    continue;

                condition["item_to_deliver"] = item;
                return true;
            }

            return false;
        }

        protected bool GenDeliverQuestItemCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 6, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            foreach (var system in systems)
            {
                if (system.Id == context.Quest.ObjectSystem)
                    continue;

                var obj = system
                    .GetObjectsWithTaskBoard()
                    .ToList()
                    .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                    .FirstOrDefault();

                if (obj is not null)
                {
                    condition["dest_obj_id"] = obj.Id;
                    condition["dest_sys_id"] = system.Id;
                    condition["dest_obj_type"] = (int)obj.ObjectType;
                    condition["item_to_deliver"] = info.ItemToDeliver;
                    condition["item_to_deliver_unique_data"] = new JsonObject
                    {
                        ["type"] = 1,
                        ["quest_id"] = context.Quest.Id
                    }.ToJsonStringUnbuffered(false);
                    return true;
                }
            }

            return false;
        }

        protected bool GenPickUpAndDeliverQuestItemCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 4, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedObjects = context.Quest.Conditions?
                .Select(q => (
                    Type: (GalaxyMapObjectType?)(byte?)q["storage_obj_type"],
                    Id: (int?)q["storage_obj_id"]))
                .Where(o => o.Type is not null && o.Id is not null)
                .ToList();

            foreach (var system in systems)
            {
                if (system.Id == context.Quest.ObjectSystem)
                    continue;

                foreach (var obj in system.GetObjectsWithTaskBoard())
                {
                    if (addedObjects.Any(i => obj.Id == i.Id && obj.ObjectType == i.Type) == false)
                    {
                        condition["storage_sys_id"] = system.Id;
                        condition["storage_obj_id"] = obj.Id;
                        condition["storage_obj_type"] = (byte)obj.ObjectType;
                        condition["item_to_deliver"] = info.ItemToDeliver;
                        condition["item_to_deliver_unique_data"] = new JsonObject
                        {
                            ["type"] = 1,
                            ["quest_id"] = context.Quest.Id
                        }.ToJsonStringUnbuffered(false);
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool GenKillMobShipCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 3, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedMobs = context.Quest.Conditions?
                .Select(q => (int?)q["target_mob_id"])
                .Where(o => o is not null)
                .ToList();

            foreach (var system in systems)
            {
                foreach (var item in Realm.MobsMap
                    .GetSystemMobs(system.Id)?
                    .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                    .ToList() ?? new())
                {
                    if (item.ObjectType != GalaxyMapObjectType.None ||
                        addedMobs.Contains(item.MobId))
                        continue;

                    if (Realm.MobsDatabase.GetMob(item?.MobId ?? -1) is DiscoveryMobInfo mob)
                    {
                        condition["target_mob_id"] = mob.Id;
                        condition["target_mob_internal_name"] = mob.InternalName;

                        if (info.CountMobsOnlyInTargetSystem == 1)
                            condition["target_systems"] = new JsonArray { system.Id };
                        else
                            condition["target_systems"] = new JsonArray();


                        return true;
                    }
                }
            }

            return false;
        }

        protected bool GenKillFactionGroupShipCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 3, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedMobs = context.Quest.Conditions?
                .Select(q => (int?)q["group_id"])
                .Where(o => o is not null)
                .ToList();

            foreach (var system in systems)
            {
                foreach (var item in Realm.MobsMap
                    .GetSystemMobs(system.Id)?
                    .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                    .ToList() ?? new())
                {
                    if (addedMobs.Contains(item.FactionGroup))
                        continue;

                    if (Realm.MobsDatabase.GetMob(item?.MobId ?? -1) is DiscoveryMobInfo mob)
                    {
                        var factionGroup = item.FactionGroup;

                        if (factionGroup < 0)
                            continue;

                        condition["group_id"] = item.FactionGroup;
                        condition["group_faction"] = (byte)mob.Faction;

                        var targetSystems = new JsonArray();

                        foreach (var nearSystem in Realm.GalaxyMap.GetSystemsArround(system.Id, 4, true))
                        {
                            if (Realm.MobsMap
                                .GetSystemMobs(nearSystem.Key.Id)?
                                .Any(m => m.FactionGroup == factionGroup) == true)
                                targetSystems.Add(nearSystem.Key.Id);
                        }

                        condition["target_systems"] = targetSystems;

                        return true;
                    }
                }
            }

            return false;
        }

        protected bool GenKillPiratesOutpostCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 30, true)
                .Select(s => s.Key);

            var addedGroups = context.Quest.Conditions?
                .Select(q => (int?)q["group_id"])
                .Where(o => o is not null)
                .ToList();

            if (info.SelectGroup == 1)
            {
                foreach (var system in systems)
                {
                    foreach (var item in system.PiratesOutposts?
                        .ToList()
                        .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                        .ToList() ?? new())
                    {
                        if (item is null ||
                            item.FactionGroup == -1 ||
                            (info.Faction != Faction.None && item.Faction != info.Faction) ||
                            addedGroups.Contains(item.FactionGroup))
                            continue;

                        condition["outpost_id"] = -1;
                        condition["target_faction"] = (byte)item.Faction;
                        condition["group_id"] = item.FactionGroup;
                        condition["level"] = context.TargetLevel;

                        var targetSystems = systems
                            .Where(s => s?.PiratesOutposts?.Any(o => o?.FactionGroup == item.FactionGroup) == true)
                            .Select(s => s.Id)
                            .Take(5)
                            .ToList();

                        if (targetSystems.Count > 0)
                            condition["star_system_ids"] = JsonHelpers.ParseNodeUnbuffered(targetSystems);

                        return true;
                    }
                }
            }
            else
            {
                condition["outpost_id"] = -1;
                condition["target_faction"] = (byte)info.Faction;
                condition["group_id"] = -1;
                condition["level"] = context.TargetLevel;

                var targetSystems = systems
                    .Where(s => s?.PiratesOutposts?.Any(o =>
                        info.Faction == Faction.None ||
                        o?.Faction == info.Faction) == true)
                    .Select(s => s.Id)
                    .Take(5)
                    .ToList();

                if (targetSystems.Count > 0)
                    condition["star_system_ids"] = JsonHelpers.ParseNodeUnbuffered(targetSystems);

                return true;
            }

            return false;
        }


        protected bool GenKillPiratesStationCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 30, true)
                .Select(s => s.Key);

            var addedGroups = context.Quest.Conditions?
                .Select(q => (int?)q["group_id"])
                .Where(o => o is not null)
                .ToList();

            if (info.SelectGroup == 1)
            {
                foreach (var system in systems)
                {
                    foreach (var item in system.PiratesStations?
                        .ToList()
                        .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                        .ToList() ?? new())
                    {
                        if (item is null ||
                            item.FactionGroup == -1 ||
                            (info.Faction != Faction.None && item.Faction != info.Faction) ||
                            addedGroups.Contains(item.FactionGroup))
                            continue;

                        condition["station_id"] = -1;
                        condition["target_faction"] = (byte)item.Faction;
                        condition["group_id"] = item.FactionGroup;
                        condition["level"] = context.TargetLevel;

                        var targetSystems = systems
                            .Where(s => s?.PiratesStations?.Any(o => o?.FactionGroup == item.FactionGroup) == true)
                            .Select(s => s.Id)
                            .Take(5)
                            .ToList();

                        if (targetSystems.Count > 0)
                            condition["star_system_ids"] = JsonHelpers.ParseNodeUnbuffered(targetSystems);

                        return true;
                    }
                }
            }
            else
            {
                condition["station_id"] = -1;
                condition["target_faction"] = (byte)info.Faction;
                condition["group_id"] = -1;
                condition["level"] = context.TargetLevel;

                var targetSystems = systems
                    .Where(s => s?.PiratesStations?.Any(o =>
                        info.Faction == Faction.None ||
                        o?.Faction == info.Faction) == true)
                    .Select(s => s.Id)
                    .Take(5)
                    .ToList();

                if (targetSystems.Count > 0)
                    condition["star_system_ids"] = JsonHelpers.ParseNodeUnbuffered(targetSystems);

                return true;
            }

            return false;
        }

        protected bool GenKillBossCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 3, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedBosses = context.Quest.Conditions?
                .Select(q => (int?)q["target_mob_id"])
                .Where(o => o is not null)
                .ToList() ?? new();

            bool CreateForObject(GalaxyMapStarSystem system, IEnumerable<IGalaxyMapObject> mapObjects)
            {
                if (system is null || mapObjects is null)
                    return false;

                foreach (var targetObject in mapObjects
                    .ToList()
                    .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                    .ToList() ?? new())
                {
                    foreach (var item in Realm.MobsMap.GetObjectMobs(system.Id, targetObject.ObjectType, targetObject.Id)
                            .ToList()
                            .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                            .ToList() ?? new())
                    {
                        if (item is not null &&
                            addedBosses.Contains(item.FleetId) == false &&
                            Realm.MobsDatabase.GetMob(item.MobId) is DiscoveryMobInfo mobInfo)
                        {
                            condition["target_mob_id"] = item.FleetId;
                            condition["target_mob_internal_name"] = mobInfo.InternalName;
                            condition["target_object_id"] = targetObject.Id;
                            condition["target_object_type"] = (byte)targetObject.ObjectType;
                            condition["target_systems"] = JsonHelpers.ParseNodeUnbuffered(new List<int> { system.Id });

                            return true;
                        }
                    }
                }

                return false;
            }

            if (context.Quest.LogicName?.EndsWith("outpost") == true)
            {
                foreach (var system in systems)
                {
                    if (system is not null &&
                        CreateForObject(system, system.PiratesOutposts?.Cast<IGalaxyMapObject>()) == true)
                        return true;
                }

            }
            else if (context.Quest.LogicName?.EndsWith("station") == true)
            {
                foreach (var system in systems)
                {
                    if (system is not null &&
                        CreateForObject(system, system.PiratesStations?.Cast<IGalaxyMapObject>()) == true)
                        return true;
                }
            }
            else if (context.Quest.LogicName?.EndsWith("system") == true)
            {
                foreach (var system in systems)
                {
                    foreach (var item in Realm.MobsMap
                        .GetSystemMobs(system.Id)?
                        .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                        .ToList() ?? new())
                    {
                        if (item is not null &&
                            addedBosses.Contains(item.MobId) == false &&
                            Realm.MobsDatabase.GetMob(item.MobId) is DiscoveryMobInfo mobInfo &&
                            mobInfo.Tags?.Contains("Mob.Role.Boss", StringComparer.InvariantCultureIgnoreCase) == true)
                        {
                            condition["target_mob_id"] = item.FleetId;
                            condition["target_mob_internal_name"] = mobInfo.InternalName;
                            condition["target_object_id"] = item.FleetId;
                            condition["target_object_type"] = (byte)DiscoveryObjectType.AiFleet;
                            condition["target_systems"] = JsonHelpers.ParseNodeUnbuffered(new List<int> { system.Id });
                            return true;
                        }
                    }
                }
            }
            else
            {

            }

            return false;
        }

        protected bool GenKillShipCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            condition["factions"] = JsonHelpers.ParseNodeUnbuffered(info.Factions?.Cast<byte>());
            condition["ship_class"] = JsonHelpers.ParseNodeUnbuffered(info.ShipClass);
            condition["min_target_level"] = info.MinTargetLevel;
            return true;
        }

        protected bool GenKillUniquePirateShipCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 3, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedMobs = context.Quest.Conditions?
                .Select(q => (int?)q["target_id"])
                .Where(o => o is not null)
                .ToList();

            foreach (var system in systems)
            {
                foreach (var item in Realm.MobsMap
                    .GetSystemMobs(system.Id)?
                    .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                    .ToList() ?? new())
                {
                    if (item.ObjectType != GalaxyMapObjectType.None ||
                        addedMobs.Contains(item.FactionGroup))
                        continue;

                    if (Realm.MobsDatabase.GetMob(item?.MobId ?? -1) is DiscoveryMobInfo mob &&
                        mob.IsElite() == true &&
                        mob.Ships?.Any(s => s?.IsElite() == true) == true)
                    {
                        condition["target_id"] = item.FleetId;
                        condition["target_system"] = item.SystemId;
                        condition["fleet_faction"] = (byte)mob.Faction;
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool GenDeliverMobDropCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var systems = Realm.GalaxyMap.GetSystemsArround(context.TargetSystemId, 5, true)
                .Select(s => s.Key)
                .ToList()
                .Randomize(context.Quest.ObjectId + context.Quest.LogicId);

            var addedMobs = context.Quest.Conditions?
                .SelectMany(q => q["target_mobs"]?.DeserializeUnbuffered<List<int>>() ?? new())
                .ToList();

            foreach (var system in systems)
            {
                foreach (var item in Realm.MobsMap
                    .GetSystemMobs(system.Id)?
                    .Randomize(context.Quest.ObjectId + context.Quest.LogicId)
                    .ToList() ?? new())
                {
                    if (item.ObjectType != GalaxyMapObjectType.None ||
                        addedMobs.Contains(item.FleetId))
                        continue;

                    if (Realm.MobsDatabase.GetMob(item.MobId) is DiscoveryMobInfo mob)
                    {
                        var targetMobs = new List<int>();
                        var targetSystems = new List<int>();

                        foreach (var targetSystem in Realm.GalaxyMap.GetSystemsArround(system.Id, 2, true)
                                                                    .Select(s => s.Key)
                                                                    .ToList())
                        {
                            var systemMobs = Realm.MobsMap
                                .GetSystemMobs(targetSystem.Id)?
                                .Where(m => m?.MobId == item.MobId && item.ObjectType == GalaxyMapObjectType.None)
                                .Select(m => m.FleetId)
                                .ToList();

                            if (systemMobs is null || systemMobs.Count < 1)
                                continue;

                            targetMobs.AddRange(systemMobs);
                            targetSystems.Add(targetSystem.Id);
                        }

                        if (targetMobs.Count < 3)
                            continue;

                        condition["item_to_deliver"] = info.ItemToDeliver;
                        condition["item_to_deliver_unique_data"] = new JsonObject
                        {
                            ["type"] = 1,
                            ["quest_id"] = context.Quest.Id
                        }.ToJsonStringUnbuffered(false);
                        condition["drop_chance"] = info.DropChance;
                        condition["target_mobs"] = JsonHelpers.ParseNodeUnbuffered(targetMobs);
                        condition["target_systems"] = JsonHelpers.ParseNodeUnbuffered(targetSystems);
                        condition["mob_internal_name"] = mob.InternalName;
                        condition["bindings"] = JsonHelpers.ParseNodeUnbuffered(new List<DiscoveryQuestBinding> { new()
                        {
                            SystemId = context.Quest.ObjectSystem,
                            ObjectId = context.Quest.ObjectId,
                            ObjectType = (DiscoveryObjectType)context.Quest.ObjectType,
                            CanBeAccepted = true,
                            CanBeFinished = true,
                        }});

                        return true;
                    }
                }
            }
            return false;
        }

        protected bool GenCompleteTaskCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            condition["TaskStarSystemLevel"] = info.TaskStarSystemLevel;
            return true;
        }

        protected bool GenReachCharacterLevelCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            condition["level_to_reach"] = info.LevelToReach;
            return true;
        }

        protected bool GenResearchProjectCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            int entity = info.SuitableEntities?.FirstOrDefault() ?? -1;

            if (entity == -1)
                return false;

            condition["entity_id"] = entity;

            if (ExtraMap
                .GetCircle(context.TargetLevel)?
                .GetStartSystem(context.TargetFaction) is GalaxyMapStarSystem starSystem)
            {
                var systems = Realm.GalaxyMap
                    .GetSystemsArround(starSystem.Id, 5, true)
                    .Select(s => s.Key)
                    .ToList() ?? new();

                foreach (var system in systems)
                {
                    foreach (var mob in Realm.MobsMap.GetSystemMobs(system.Id) ?? new())
                    {
                        if (mob is not null &&
                            mob.ObjectType == GalaxyMapObjectType.None &&
                            Realm.MobsDatabase.GetMob(mob.MobId) is DiscoveryMobInfo mobInfo &&
                            mobInfo.GetDropItems()?.Contains(entity) == true)
                        {
                            condition["mob_id"] = mob.FleetId;
                            condition["mob_name"] = mobInfo.InternalName;
                            condition["star_system_ids"] = JsonHelpers.ParseNodeUnbuffered(new List<int> { system.Id });
                            condition["drop_entity"] = 0;
                            return true;
                        }
                    }
                }

                foreach (var system in systems.ToList().Randomize(context.Quest.ObjectId + context.Quest.LogicId))
                {
                    foreach (var mob in Realm.MobsMap.GetSystemMobs(system.Id) ?? new())
                    {
                        if (mob is not null &&
                            mob.ObjectType == GalaxyMapObjectType.None &&
                            Realm.MobsDatabase.GetMob(mob.MobId) is DiscoveryMobInfo mobInfo)
                        {
                            condition["mob_id"] = mob.FleetId;
                            condition["mob_name"] = mobInfo.InternalName;
                            condition["star_system_ids"] = JsonHelpers.ParseNodeUnbuffered(new List<int> { system.Id });
                            condition["drop_entity"] = 1;
                            return true;
                        }
                    }
                }
            }


            return true;
        }

        protected bool GenStatTrackingCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            condition["quest_value_factor"] = info.QuestValueFactor;
            condition["stat_tag"] = info.StatTag;
            return true;
        }

        protected bool GenKillBossOfAnyFactiontCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            var addedGroups = context.Quest.Conditions?
                .Select(q => (int?)q["target_faction_group_id"])
                .Where(o => o is not null)
                .ToList();

            var groups = Realm.GalaxyMap
                .GetSystemsArround(context.TargetSystemId, 30, true)
                .Where(s => s.Key is not null)
                .SelectMany(s => context.MobsMap.GetSystemMobs(s.Key.Id) ?? new())
                .ToList()
                .Where(s => s.FactionGroup != -1 && context.MobsDatabase.GetMob(s.MobId)?.Faction == info.BossFaction)
                .GroupBy(m => m.FactionGroup)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groups)
            {
                var mobs = group.ToList();

                if (addedGroups.Contains(group.Key) ||
                    mobs.Count < 3)
                    continue;

                condition["target_mob_id"] = -1;
                condition["target_object_id"] = -1;
                condition["target_object_type"] = (int)GalaxyMapObjectType.None;
                condition["target_faction"] = (int)info.BossFaction;
                condition["target_faction_group_id"] = group.Key;
                condition["target_systems"] = JsonHelpers.ParseNodeUnbuffered(mobs.Select(m => m.SystemId).Distinct().ToList());

                return true;
            }

            return false;
        }

        protected bool GenDeliverItemsCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            condition["item_to_deliver"] = info.ItemToDeliver;
            return true;
        }

        protected bool GenExploreSystemObjectCondition(QuestContext context, JsonNode condition, QuestConditionInfo info)
        {
            condition["ObjectType"] = info.ObjectType;
            condition["SystemLevel"] = info.SystemLevel;

            if (info.PossibleSystemsCount > 0)
            {
                var systems = Realm.GalaxyMap
                    .GetSystemsArround(context.TargetSystemId, 30, true)
                    .Where(s => s.Key.GetAllObjects().Any(o => (int)o.ObjectType == info.ObjectType))
                    .Select(s => s.Key.Id)
                    .Take(info.PossibleSystemsCount)
                    .ToArray();

                if (systems.Length > 0)
                    condition["PossibleStarSystems"] = JsonHelpers.ParseNodeUnbuffered(systems);
            }

            return true;
        }
    }
}
