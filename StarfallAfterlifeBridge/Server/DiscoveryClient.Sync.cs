using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.Server.Quests;
using System.Text.Json.Nodes;
using StarfallAfterlife.Bridge.Serialization;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public virtual void SyncObjectInfo(StarSystemObject obj)
        {
            if (obj is null)
                return;

            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.ObjectInfoUpdated,
                writer =>
                {
                    var doc = new JsonObject
                    {

                    };

                    var character = CurrentCharacter;
                    var locs = new JsonArray();

                    if (character is null)
                        return;

                    //"pmc", // Mining complex
                    //"msf", // Faction shop
                    //"psc", // Supply farm
                    //"pcm", // Colony resources controll
                    //"msm", // Spaceport resources controll
                    //"mspc", // Character inventory
                    //"mstp", // Docks
                    //"bmo", // Resouces diller
                    //"bmb", // Smuggler
                    //"frp", // Mods workshop
                    //"fd", // Fuel docks
                    //"mshr", // House registry
                    //"wb", // Subatom gel
                    //"mans", // Mining stantion
                    //"msgb", // Message beacon
                    //"nptp", // Traiding post
                    //"mms", // Miners shop
                    //"scs", // Mods workshop
                    //"task_board", // Location quests

                    if (obj.Type == DiscoveryObjectType.Mothership)
                    {
                        doc["ms_info"] = new JsonObject
                        {
                            ["ore_cons"] = 12,
                            ["oper_ore"] = 23,
                            ["oper_ore_max"] = 34,
                            ["ore"] = 56,
                            ["ore_max"] = 67,
                            ["sup"] = 78,
                            ["sup_max"] = 89,
                            ["planets"] = 5,
                            ["systems"] = 1,
                            ["building_progress"] = 0,
                        };

                        doc["warp_planets"] = new JsonArray();

                        locs.Add(new JsonObject { ["type"] = "mstp" });
                        locs.Add(new JsonObject { ["type"] = "mspc" });
                        locs.Add(new JsonObject { ["type"] = "mshr" });
                        //locs.Add(new JsonObject { ["type"] = "task_board" });
                    }
                    else if (obj.Type == DiscoveryObjectType.QuickTravelGate)
                    {
                        doc["warp_systems"] =
                            character.Progress?.WarpSystems?
                            .Select(s => new JsonObject { ["starsystem"] = s })
                            .ToJsonArray() ?? new();

                        locs.Add(new JsonObject
                        {
                            ["type"] = "wb_warp",
                            ["service_price"] = SfaDatabase.GetWarpingCost(obj.System?.Info?.Level ?? 0)
                        });
                    }
                    else if (obj is Planet planet)
                    {
                        doc["colony_info"] = new JsonObject
                        {
                            ["pop"] = 0,
                            ["pop_max"] = 0,
                            ["pop_growth"] = 0,
                            ["prov"] = 0,
                        };
                        doc["warp_planets"] = new JsonArray();
                        doc["planet_info"] = new JsonObject
                        {
                            ["provision"] = 1,
                            ["provision_rich"] = 1,
                            ["black_metals"] = 1,
                            ["black_metals_rich"] = 1,
                            ["oil"] = 1,
                            ["oil_rich"] = 1,
                            ["minerals"] = 1,
                            ["minerals_rich"] = 1,
                            ["heavy_metals"] = 1,
                            ["heavy_metals_rich"] = 1,
                            ["acids"] = 1,
                            ["acids_rich"] = 1,
                            ["super_conductors"] = planet.SuperConductors,
                            ["super_conductors_rich"] = 1,
                            ["radiactive_metals"] = planet.RadiactiveMetals,
                            ["radiactive_metals_rich"] = 1,
                            ["noble_gases"] = planet.NoubleGases,
                            ["noble_gases_rich"] = 1,
                        };
                    }
                    else if (obj is RepairStation repairStation)
                    {
                        doc["upgrade_group"] = new JsonObject
                        {
                            ["grpname"] = "",
                            ["tech"] = new JsonArray { },
                        };
                    }
                    else if (obj is ScienceStation scienceStation)
                    {
                        doc["upgrade_group"] = new JsonObject
                        {
                            ["grpname"] = "",
                            ["tech"] = new JsonArray { },
                        };
                    }
                    else if (obj is FuelStation fuelStation)
                    {
                        locs.Add(new JsonObject
                        {
                            ["type"] = "fd",
                            ["refuel_price"] = SfaDatabase.GetRefuelCost(obj.System?.Info?.Level ?? 0)
                        });
                    }

                    if (Server?.Realm?.QuestsDatabase
                        .GetTaskBoardQuests((byte)obj.Type, obj.Id)?
                        .ToList()
                        .ExceptBy(character.Progress?.CompletedQuests ?? new(), q => q.Id)
                        .ToList() is List<DiscoveryQuest> questsInfo)
                    {
                        var activeQuests = character.ActiveQuests ?? new();
                        var quests = new JsonArray();


                        if (obj is DiscoveryMothership mothership)
                            questsInfo = questsInfo.Where(i => activeQuests.Any(q => q?.Id == i.Id)).ToList();

                        foreach (var item in activeQuests)
                        {
                            if (item is null ||
                                questsInfo.Any(i => i.Id == item.Id))
                                continue;

                            var binding = item.GetBindings().FirstOrDefault(
                                b => b?.CanBeFinished == true &&
                                b.SystemId == obj.System?.Id &&
                                b.ObjectType == obj.Type &&
                                b.ObjectId == obj.Id);

                            if (binding is not null)
                                questsInfo.Add(item.Info);
                        }

                        foreach (var info in questsInfo)
                        {
                            var revardItems = new JsonArray();

                            foreach (var item in info.Reward.Items ?? new())
                                revardItems.Add(new JsonObject { ["item"] = item.Id, ["count"] = item.Count });

                            quests.Add(new JsonObject
                            {
                                ["entity_id"] = info.Id,
                                ["quest_logic"] = info.LogicId,
                                ["level"] = info.Level,
                                ["reward"] = new JsonObject
                                {
                                    ["igc"] = info.Reward.IGC,
                                    ["xp"] = info.Reward.Xp,
                                    ["house_currency"] = info.Reward.HouseCurrency,
                                    ["items"] = revardItems
                                },
                            });
                        }

                        if (quests.Count > 0)
                        {
                            locs.Add(new JsonObject
                            {
                                ["type"] = "task_board",
                                ["quests"] = quests,
                            });
                        }
                    }

                    if (Server?.Realm?.ShopsMap?.GetObjectShops(obj.Id, obj.Type) is ObjectShops shopsInfo)
                    {
                        foreach (var item in shopsInfo.Shops)
                        {
                            locs.Add(new JsonObject
                            {
                                ["type"] = item.ShopName
                            });
                        }
                    }

                    foreach (var quest in CurrentCharacter?.ActiveQuests ?? new())
                    {
                        var questLocations = quest.GetLocations();

                        foreach (var loc in questLocations)
                        {
                            if (loc is null ||
                                loc.SystemId != obj.System?.Id ||
                                loc.ObjectType != obj.Type ||
                                loc.ObjectId != obj.Id)
                                continue;

                            locs.Add(new JsonObject
                            {
                                ["type"] = $"quest_loc_{loc.QuestId}_{loc.ConditionId}",
                                ["quest_location_type"] = loc.Type,
                                ["quest_entity_id"] = loc.QuestId,
                                ["condition_id"] = loc.ConditionId,
                            });
                        }
                    }

                    doc["loc"] = locs;

                    writer.WriteInt32(obj.Id);
                    writer.WriteByte((byte)obj.Type);
                    writer.WriteShortString(doc.ToJsonString(false), -1, true, Encoding.UTF8);
                });
        }

        public virtual void SyncPlanetInfo(int systemId, int planetId)
        {
            if (Galaxy.GetActiveSystem(systemId, true)?.GetObject(planetId, DiscoveryObjectType.Planet) is Planet planet)
            {
                SendSyncMessage(
                    planet,
                    writer =>
                    {
                        writer.WriteShortString(planet.Name, -1, true, Encoding.UTF8); // PlanetName
                        writer.WriteShortString("", -1, true, Encoding.UTF8); // RenamedByUsername
                        writer.WriteShortString("", -1, true, Encoding.UTF8); // ColonizedByUsername
                        writer.WriteBoolean(false); // HasSupplyColony
                        writer.WriteBoolean(false); // HasMiningColony
                        writer.WriteInt32(int.MaxValue); // Population
                    });
            }
        }

        public virtual void SyncMothership(int systemId, int mothershipId)
        {
            if (Galaxy.GetActiveSystem(systemId, true).GetObject(mothershipId) is DiscoveryMothership mothership)
            {
                SendSyncMessage(
                    mothership,
                    writer =>
                    {
                        writer.WriteByte(6); // DevelopmentState
                        writer.WriteHex(mothership.Hex); // Position
                        writer.WriteSingle(0); // TimeToBuild
                        writer.WriteSingle(0); // TimeUntilBuildFinished
                        writer.WriteBoolean(true); // IsOperational
                        writer.WriteBoolean(true); // IsBuilding
                        writer.WriteByte((byte)Faction.None); // AttackingFaction
                        writer.WriteSingle(0); // AttackEventTimeLeft
                        writer.WriteInt32(0); // AttackEventHexDone
                        writer.WriteInt32(0); // AttackEventHexRequired
                    });
            }
        }

        public virtual void SyncPiratesStation(int systemId, int stationId)
        {
            if (Galaxy
                .GetActiveSystem(systemId, true)
                .GetObject(stationId, DiscoveryObjectType.PiratesStation) is PiratesStation station)
            {
                SendSyncMessage(
                    station,
                    writer =>
                    {
                        writer.WriteHex(station.Hex); // Position
                    });
            }
        }

        public virtual void SyncPiratesOutpost(int systemId, int stationId)
        {
            if (Galaxy
                .GetActiveSystem(systemId, true)
                .GetObject(stationId, DiscoveryObjectType.PiratesOutpost) is PiratesOutpost outpost)
            {
                SendSyncMessage(
                    outpost,
                    writer =>
                    {
                        writer.WriteInt32(outpost.Level); // Level
                        writer.WriteHex(outpost.Hex); // Position
                    });
            }
        }

        public virtual void SyncMinerMothership(int systemId, int mothershipId)
        {
            if (Galaxy
                .GetActiveSystem(systemId, true)
                .GetObject(mothershipId, DiscoveryObjectType.MinerMothership) is MinerMothership mothership)
            {
                SendSyncMessage(
                    mothership,
                    writer =>
                    {
                        writer.WriteInt32(mothership.Id); // Id
                        writer.WriteHex(mothership.Hex); // Position
                        writer.WriteInt32(mothership.Level); // Level
                    });
            }
        }
        public virtual void SyncRichAsteroid(int systemId, int asteroidId)
        {
            if (Galaxy
                .GetActiveSystem(systemId, true)
                .GetObject(asteroidId, DiscoveryObjectType.RichAsteroids) is StarSystemRichAsteroid asteroid)
            {
                SendSyncMessage(
                    asteroid,
                    writer =>
                    {
                        writer.WriteHex(asteroid.Hex); // Position

                        var ores = asteroid.Ores ?? new();

                        writer.WriteUInt16((ushort)ores.Count); // Count

                        foreach (var item in ores)
                        {
                            writer.WriteByte((byte)InventoryItemType.DiscoveryItem); // ItemType
                            writer.WriteInt32(item); // Id
                            writer.WriteInt32(100); // Count
                            writer.WriteInt32(1); // IGCPrice
                            writer.WriteInt32(1); // BGCPrice
                            writer.WriteShortString("", -1, true, Encoding.UTF8); // UniqueData
                        }
                    });
            }
        }

        public virtual void SyncStarSystemOnject(StarSystemObject systemObject)
        {
            SendSyncMessage(systemObject);
        }

        public virtual void SyncFleetData(DiscoveryFleet fleet)
        {
            SendSyncMessage(
                fleet,
                writer =>
                {
                    writer.WriteInt32(fleet.MobId); // MobId
                    writer.WriteInt32(fleet.Hull); // SyncHullId

                    writer.WriteUInt16(6); // SkinParamsCount
                    writer.WriteInt32(fleet.Skin);
                    writer.WriteInt32(fleet.SkinColor1);
                    writer.WriteInt32(fleet.SkinColor2);
                    writer.WriteInt32(fleet.SkinColor3);
                    writer.WriteInt32(fleet.Decal);
                    writer.WriteInt32(fleet.DecalColor);

                    writer.WriteShortString(fleet.Name ?? string.Empty, -1, true, Encoding.ASCII); // FleetName
                    writer.WriteVector2(fleet.Location);
                    writer.WriteVector2(fleet.TargetLocation);
                    writer.WriteSingle(fleet.Speed * 0.5f);
                    writer.WriteInt32(fleet.State is FleetState.InBattle ? 1 : 0); // FleetBattleState
                    writer.WriteInt32(fleet.Level); // Level
                    writer.WriteInt32(fleet.Vision); // Vision
                    writer.WriteInt32(fleet.NebulaVision); // NebulaVision

                    var effects = fleet.GetEffects();

                    writer.WriteUInt16((ushort)effects.Length); // EffectsCount

                    foreach (var effect in effects)
                    {
                        writer.WriteInt32((int)effect.Logic); // EffectLogic
                        writer.WriteSingle(effect.Duration); // EffectCurrentActiveTime
                    }

                    writer.WriteByte((byte)fleet.DockObjectType); // DockObjectType
                    writer.WriteInt32(fleet.DockObjectId); // DockObjectId
                });
        }

        public virtual void SyncMove(DiscoveryFleet fleet)
        {
            SendDiscoveryMessage(
                fleet,
                DiscoveryServerAction.Moved,
                writer =>
                {
                    writer.WriteVector2(fleet.Route.TargetWaypointLocation);
                    writer.WriteVector2(fleet.Location);
                });
        }

        public virtual void SyncRoute(DiscoveryFleet fleet)
        {
            SendDiscoveryMessage(
                fleet,
                DiscoveryServerAction.FleetRouteSync,
                writer =>
                {
                    if (fleet.Route.Path.ToArray() is Vector2[] path && path.Length > 0)
                    {
                        writer.WriteUInt16(1);
                        writer.WriteVector2(path.LastOrDefault());
                    }
                    else
                    {
                        writer.WriteUInt16(0);
                    }
                });
        }

        public virtual void SyncSharedVision(DiscoveryFleet fleet)
        {
            SendDiscoveryMessage(
                fleet,
                DiscoveryServerAction.SharedFleetVision,
                writer =>
                {
                    var vision = fleet.GetSharedVision();

                    writer.WriteUInt16((ushort)vision.Length); // Size

                    foreach (var item in vision)
                        writer.WriteInt32(item?.Id ?? 0); // FleetId
                });
        }

        public virtual void SyncAbilities(DiscoveryFleet fleet)
        {
            var abilities = CurrentCharacter?.GetAbilitiesCooldown() ?? Array.Empty<KeyValuePair<int, float>>();

            SendDiscoveryMessage(
                fleet,
                DiscoveryServerAction.SyncAbility,
                writer =>
                {
                    writer.WriteUInt16((ushort)abilities.Length); // Size

                    foreach(var item in abilities)
                    {
                        writer.WriteInt32(item.Key); // ID
                        writer.WriteSingle(item.Value); // Cooldown
                    }
                });
        }

        public void SyncDiscoveryObject(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            switch (objectType)
            {
                case DiscoveryObjectType.None:
                    break;
                case DiscoveryObjectType.UserFleet:
                    break;
                case DiscoveryObjectType.Planet:
                    SyncPlanetInfo(systemId, objectId);
                    break;
                case DiscoveryObjectType.Repairstation:
                    break;
                case DiscoveryObjectType.Fuelstation:
                    break;
                case DiscoveryObjectType.AiFleet:
                    break;
                case DiscoveryObjectType.Mothership:
                    SyncMothership(systemId, objectId);
                    break;
                case DiscoveryObjectType.Trash:
                    break;
                case DiscoveryObjectType.Asteroid:
                    break;
                case DiscoveryObjectType.Blackmarket:
                    break;
                case DiscoveryObjectType.AttackEventInstance:
                    break;
                case DiscoveryObjectType.PiratesStation:
                    break;
                case DiscoveryObjectType.InstanceBattle:
                    break;
                case DiscoveryObjectType.RichAsteroids:
                    break;
                case DiscoveryObjectType.Nebula:
                    break;
                case DiscoveryObjectType.UserPhantom:
                    break;
                case DiscoveryObjectType.WarpBeacon:
                    break;
                case DiscoveryObjectType.MiningStation:
                    break;
                case DiscoveryObjectType.MessageBeacon:
                    break;
                case DiscoveryObjectType.MinerMothership:
                    break;
                case DiscoveryObjectType.ScienceStation:
                    break;
                case DiscoveryObjectType.QuickTravelGate:
                    break;
                case DiscoveryObjectType.PiratesOutpost:
                    break;
                case DiscoveryObjectType.SecretObject:
                    break;
                case DiscoveryObjectType.CustomInstance:
                    break;
                case DiscoveryObjectType.Tradestation:
                    break;
                case DiscoveryObjectType.HouseActionHolder:
                    break;
                default:
                    break;
            }
        }

        public void SendSyncMessage(StarSystemObject obj, Action<SfWriter> writeAction = null)
        {
            SendDiscoveryMessage(
                obj, DiscoveryServerAction.Sync, writer =>
                {
                    writer.WriteByte((byte)obj.Faction);
                    writer.WriteInt32(obj.FactionGroup);

                    var completedQuests = CurrentCharacter?.Progress?.CompletedQuests ?? new();

                    if (Server?.Realm?.QuestsDatabase
                        .GetTaskBoardQuests((byte)obj.Type, obj.Id)
                        .Select(q => q.Id)?
                        .ToList()
                        .Except(completedQuests)?.ToList() is List<int> quests)
                    {
                        writer.WriteUInt16((ushort)quests.Count);

                        foreach (var quest in quests)
                            writer.WriteInt32(quest);
                    }
                    else
                    {
                        writer.WriteUInt16(0);
                    }

                    writeAction?.Invoke(writer);
                });
        }
    }
}
