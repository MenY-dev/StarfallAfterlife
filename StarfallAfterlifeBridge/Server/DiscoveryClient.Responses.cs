using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Inventory;
using StarfallAfterlife.Bridge.Server.Quests;
using StarfallAfterlife.Bridge.Server.Quests.Conditions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public virtual void SendEnterToStarSystem(int systemId, Vector2 location)
        {
            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.InitialWarpedToMothership,
                writer =>
                {
                    writer.WriteInt32(systemId);
                    writer.WriteVector2(location);
                });

            SfaDebug.Print($"EnterToStarSystem (SystemId = {systemId}, Location = {location})", "DiscoveryServerClient");
        }

        public void SendDisconnectObject(StarSystemObject obj)
        {
            SendDiscoveryMessage(
                obj,
                DiscoveryServerAction.DisconnectObject);

            SfaDebug.Print($"DisconnectObject (SystemId = {obj?.System?.Id}, Id = {obj?.Id}, Type = {obj?.Type})", "DiscoveryServerClient");
        }

        public virtual void SendFleetWarpedGateway()
        {
            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.FleetWarpedGateway);

            SfaDebug.Print($"FleetWarpedGateway", "DiscoveryServerClient");
        }

        public virtual void SendFleetWarpedMothership()
        {
            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.FleetWarpedMothership);

            SfaDebug.Print($"FleetWarpedMothership", "DiscoveryServerClient");
        }


        //public virtual void SendCargoListForObject(int objectId, DiscoveryObjectType objectType, string stockName)
        //{
        //    SendDiscoveryMessage(
        //        CurrentCharacter.Fleet,
        //        DiscoveryServerAction.CargoListForObject, writer =>
        //        {
        //            writer.WriteUInt16(1); // Count

        //            writer.WriteByte(3); // ItemType
        //            writer.WriteInt32(451815740); // Id
        //            writer.WriteInt32(100); // Count
        //            writer.WriteInt32(100); // IGCPrice
        //            writer.WriteInt32(0); // ReputationPrice
        //            writer.WriteShortString(stockName, -1, true, Encoding.UTF8); // UniqueData

        //            writer.WriteInt32(-1); // ObjectId
        //            writer.WriteByte((byte)DiscoveryObjectType.None); // ObjectType
        //            writer.WriteShortString(stockName, -1, true, Encoding.UTF8); // ???
        //            writer.WriteUInt32(0); // ???
        //            writer.WriteUInt32(0); // ???
        //            writer.WriteByte(0); // StockUpdateType
        //        });

        //    SFDebug.Print($"ObjectCargoList", GetType().Name);
        //}

        public virtual void SendObjectStock(StarSystemObject obj, ICollection<InventoryItem> stock, string name)
        {
            if (stock is null)
                return;

            SendDiscoveryMessage(
                obj,
                DiscoveryServerAction.ObjectStockUpdated,
                writer =>
                {
                    writer.WriteUInt16((ushort)stock.Count); // Count

                    foreach (var item in stock)
                    {
                        writer.WriteByte((byte)item.Type); // ItemType
                        writer.WriteInt32(item.Id); // Id
                        writer.WriteInt32(item.Count); // Count
                        writer.WriteInt32(item.IGCPrice); // IGCPrice
                        writer.WriteInt32(item.BGCPrice); // BGCPrice
                        writer.WriteShortString("", -1, true, Encoding.UTF8); // UniqueData
                    }

                    writer.WriteShortString(name, -1, true, Encoding.UTF8); // Stock
                });

            SfaDebug.Print($"ObjectStockUpdated (Stock = {name}, Count = {stock.Count}", GetType().Name);
        }

        public virtual void SendFleetCargo()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet is UserFleet fleet &&
                character.Ships is List<ShipConstructionInfo> ships)
            {
                foreach (var item in ships)
                {
                    SendObjectStock(fleet, item.Cargo, "cargo_ship_" + item.Id);
                }
            }

            SfaDebug.Print($"FleetCargoUpdated", GetType().Name);
        }

        public virtual void SendObjectStockOld(StarSystemObject obj, string stockName)
        {
            var stoc = (obj as DockableObject)?.Storages.FirstOrDefault(s => s.Name == stockName);

            if (stoc is null)
                return;

            SendDiscoveryMessage(
                obj,
                DiscoveryServerAction.ObjectStockUpdated,
                writer =>
                {
                    writer.WriteUInt16((ushort)stoc.Count); // Count

                    foreach (var item in stoc)
                    {
                        writer.WriteByte((byte)item.Key.ItemType); // ItemType
                        writer.WriteInt32(item.Key.Id); // Id
                        writer.WriteInt32(item.Value); // Count
                        writer.WriteInt32(item.Key.IGC); // IGCPrice
                        writer.WriteInt32(item.Key.BGC); // BGCPrice
                        writer.WriteShortString("", -1, true, Encoding.UTF8); // UniqueData
                    }

                    writer.WriteShortString(stockName, -1, true, Encoding.UTF8); // Stock
                });

            SfaDebug.Print($"ObjectStockUpdated (Stock = {stockName}, Count = {stoc.Count}, System = {obj.System?.Id}, Id = {obj.Id}, Type = {obj.Type})", GetType().Name);
        }

        public virtual void SendInfoWidgetData(SelectionInfo selection)
        {
            var info = new JsonArray();
            var process = new JsonObject();

            if (selection is not null)
            {
                foreach (var item in selection.Objects)
                {
                    if (item is null)
                        continue;

                    var obj = item.Target;
                    var objInfo = new JsonArray();
                    var actions = new JsonArray();

                    if (obj is Planet planet)
                    {
                        int phase = 0;

                        objInfo.Add(new JsonObject
                        {
                            ["info_type"] = "planet_base",
                            ["phase"] = phase,
                            ["planet_name"] = planet.Name,
                            ["planet_type"] = (int)planet.PlanetType,
                        });

                        if (item.Scanned == true)
                        {
                            phase++;
                            objInfo.Add(new JsonObject
                            {
                                ["info_type"] = "planet_advance",
                                ["phase"] = phase,
                                ["atmosphere"] = planet.Atmosphere,
                                ["gravity"] = planet.Gravitation,
                                ["size"] = planet.Size,
                                ["temperature"] = planet.Temperature,
                            });

                            if (CurrentCharacter.ActiveQuests
                                .SelectMany(q => q?.Conditions ?? new())
                                .Select(c => c as ScanUnknownPlanetConditionListener)
                                .FirstOrDefault(
                                    c => c is not null &&
                                    c.SystemId == obj.System?.Id &&
                                    c.ObjectId == obj.Id)
                                is ScanUnknownPlanetConditionListener condition)
                            {
                                phase++;
                                objInfo.Add(new JsonObject
                                {
                                    ["info_type"] = "find_secret_loc_quest",
                                    ["phase"] = phase,
                                    ["hidden_loc_type"] = "factory",
                                });
                            }
                        }

                        actions.Add("scan");
                    }
                    else if (obj is StarSystemRichAsteroid asteroid)
                    {
                        objInfo.Add(new JsonObject
                        {
                            ["info_type"] = "rich_asteroids_base",
                            ["phase"] = 0,
                        });


                        if (item.Scanned == true)
                        {
                            var asteroidInfo = new JsonObject
                            {
                                ["info_type"] = "rich_asteroids_content",
                                ["phase"] = 1,
                            };

                            foreach (var ore in asteroid.Ores ?? new())
                            {
                                switch (ore)
                                {
                                    case 1300860703: asteroidInfo["arkonit"] = 99; break;
                                    case 1937431386: asteroidInfo["adamantane"] = 99; break;
                                    case 684682126: asteroidInfo["neitherium"] = 99; break;
                                    case 1310293910: asteroidInfo["ro"] = 99; break;
                                    default: break;
                                }
                            }

                            objInfo.Add(asteroidInfo);
                        }

                        actions.Add("scan");
                    }
                    else if (obj is DiscoveryFleet fleet)
                    {
                        objInfo.Add(new JsonObject
                        {
                            ["info_type"] = "base_fleet",
                            ["faction"] = (int)fleet.Faction,
                            ["group_id"] = fleet.FactionGroup,
                        });
                    }

                    if (item.ScanningStarted == true)
                    {
                        process["process_type"] = "scanning";
                        process["time_left"] = item.ScanningTime;
                    }

                    info.Add(new JsonObject
                    {
                        ["id"] = obj.Id,
                        ["type"] = (int)obj.Type,
                        ["actions"] = actions,
                        ["obj_info"] = objInfo,
                    });
                }
            }

            var doc = new JsonObject
            {
                ["hexx"] = selection?.Hex.X ?? 0,
                ["hexy"] = selection?.Hex.Y ?? 0,
                ["info"] = info,
            };

            if (string.IsNullOrEmpty((string)process["process_type"]) == false)
                doc["process"] = process;

            SendDiscoveryMessage(
                CurrentCharacter?.Fleet, DiscoveryServerAction.InfoWidgetData,
                writer => writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8));

            SfaDebug.Print($"InfoWidgetData (SystemId = {selection?.SystemId}, Location = {selection?.Hex}", GetType().Name);
        }

        public virtual void SendInfoWidgetData(int systemId, DiscoveryObjectType objectType, int objectId, SystemHex hex)
        {
            var system = Galaxy.GetActiveSystem(systemId);

            var objects = Enumerable.Empty<StarSystemObject>();

            if (objectType == DiscoveryObjectType.None)
            {
                objects = system?.GetObjectsAt(hex) ?? Enumerable.Empty<StarSystemObject>();
            }
            else if (objectType == DiscoveryObjectType.AiFleet || objectType == DiscoveryObjectType.UserFleet)
            {
                objects = Enumerable.Repeat(system?.Fleets.FirstOrDefault(f => f.Id == objectId), 1);
            }

            JsonArray info = new JsonArray();

            //foreach (var planet in planets)
            //{
            //    info.Add(new JsonObject
            //    {
            //        ["id"] = planet.Id,
            //        ["type"] = (int)planet.Type,
            //        ["obj_info"] = new JsonArray
            //        {
            //            new JsonObject
            //            {
            //                ["info_type"] = "planet_base",
            //                ["planet_name"] = planet.Name,
            //                ["planet_type"] = (int)planet.Type
            //            },
            //            //new JsonObject
            //            //{
            //            //    ["info_type"] = "planet_advance",
            //            //    ["atmosphere"] = planet.Atmosphere,
            //            //    ["gravity"] = planet.Gravitation,
            //            //    ["size"] = planet.Size,
            //            //    ["temperature"] = planet.Temperature,
            //            //},
            //        }
            //    });
            //}

            foreach (var obj in objects)
            {
                var objInfo = new JsonArray();

                if (obj is Planet planet)
                {
                    objInfo.Add(new JsonObject
                    {
                        ["info_type"] = "planet_base",
                        ["phase"] = 2,
                        ["planet_name"] = planet.Name,
                        ["planet_type"] = (int)planet.PlanetType,
                    });
                }
                else if (obj is DiscoveryFleet fleet)
                {
                    objInfo.Add(new JsonObject
                    {
                        ["info_type"] = "base_fleet",
                        ["faction"] = (int)fleet.Faction,
                        ["group_id"] = fleet.FactionGroup,
                    });
                }

                info.Add(new JsonObject
                {
                    ["id"] = obj.Id,
                    ["type"] = (int)obj.Type,
                    ["actions"] = new JsonArray { "scan" },
                    ["obj_info"] = objInfo,
                });
            }

            JsonNode process = new JsonObject
            {
                ["process_type"] = "scanning",
                ["time_left"] = 5,
            };

            JsonNode doc = new JsonObject
            {
                ["hexx"] = hex.X,
                ["hexy"] = hex.Y,
                ["info"] = info,
                //["process"] = process,
                //["scan_finished"] = 0,
            };

            SendDiscoveryMessage(
                systemId, DiscoveryObjectType.UserFleet, CurrentCharacter?.Fleet?.Id ?? -1, DiscoveryServerAction.InfoWidgetData,
                writer => writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8));

            SfaDebug.Print($"InfoWidgetData (SystemId = {systemId}, Location = {hex}, Type = {objectType}, Id = {objectId})", "DiscoveryServerClient");
        }

        public void SendQuestDialog(int entityId, QuestLogicInfo logic)
        {
            var fleet = CurrentCharacter?.Fleet;
            var questInfo = Server.Realm.QuestsDatabase.GetQuest(entityId);

            if (questInfo is null)
                return;

            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestDataUpdate,
                writer =>
                {
                    var bindings = JsonHelpers.ParseNodeUnbuffered(questInfo.CreateBindings());

                    var doc = new JsonObject()
                    {
                        ["quest_dialog"] = new JsonObject
                        {
                            ["id"] = questInfo.Id,
                            ["entity"] = questInfo.Id,
                            ["level"] = questInfo.Level,
                            ["faction"] = (byte)questInfo.ObjectFaction,
                            ["is_quest_dialog"] = 1,
                            ["state"] = (byte)QuestState.InProgress,
                            ["quest_logic"] = questInfo.LogicId,
                            ["quest_params"] = new JsonObject
                            {
                                ["condition_params"] = questInfo.Conditions.Clone() ?? new JsonArray(),
                            },
                            ["bindings"] = bindings,
                        },
                        ["reward"] = new JsonObject
                        {
                            ["igc"] = (int)questInfo.Type,
                            ["house_currency"] = questInfo.Id,
                            ["xp"] = questInfo.LogicId,
                            ["items"] = new JsonArray()
                        },
                    };

                    writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8);
                });

            SfaDebug.Print($"SendQuestDialog (QuestId = {entityId})", "DiscoveryServerClient");
        }

        public void SendQuestDataUpdate()
        {
            if (CurrentCharacter is ServerCharacter character)
            {
                SendGalaxyMessage(
                    DiscoveryServerGalaxyAction.QuestDataUpdate,
                    writer =>
                    {
                        var quests = new JsonArray();

                        foreach (var listener in character.ActiveQuests)
                        {
                            var conditions = listener.Info.Conditions?.Clone();
                            var progress = new JsonArray();
                            var bindings = new JsonArray();

                            foreach (var condition in listener.Conditions)
                            {
                                progress.Add(new JsonObject
                                {
                                    ["identity"] = condition.Identity,
                                    ["progress_done"] = condition.Progress,
                                });
                            }

                            foreach (var binding in listener.GetBindings())
                            {
                                bindings.Add(new JsonObject
                                {
                                    ["starsystem"] = binding.SystemId,
                                    ["object_id"] = binding.ObjectId,
                                    ["object_type"] = (byte)binding.ObjectType,
                                    ["can_be_accepted"] = binding.CanBeAccepted,
                                    ["can_be_finished"] = binding.CanBeFinished,
                                });
                            }

                            quests.Add(new JsonObject
                            {
                                ["id"] = listener.Info.Id,
                                ["entity"] = listener.Info.Id,
                                ["level"] = listener.Info.Level,
                                ["faction"] = (byte)listener.Info.ObjectFaction,
                                ["state"] = (byte)listener.State,
                                ["quest_logic"] = listener.Info.LogicId,
                                ["minutes_remain"] = 0,
                                ["progress"] = new JsonObject
                                {
                                    ["conditions"] = progress,
                                },
                                ["quest_params"] = new JsonObject
                                {
                                    ["condition_params"] = conditions,
                                },
                                ["reward"] = new JsonObject
                                {
                                    ["igc"] = (int)listener.Info.Type,
                                    ["house_currency"] = listener.Info.Id,
                                    ["xp"] = listener.Info.LogicId,
                                    ["items"] = new JsonArray()
                                },
                                ["bindings"] = bindings,
                            });
                        }

                        var doc = new JsonObject()
                        {
                            ["quests"] = quests
                        };

                        writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8);

                        SfaDebug.Print($"QuestDataUpdate ({quests.ToJsonString()}", "DiscoveryServerClient");
                    });
            }
        }

        public void SendQuestCompleteData(QuestListener quest)
        {
            if (CurrentCharacter?.Fleet is UserFleet fleet)
            {
                SendGalaxyMessage(
                    DiscoveryServerGalaxyAction.QuestCompleteData,
                    writer =>
                    {
                        writer.WriteInt32(quest.Id);
                        writer.WriteInt32((byte)quest.State);
                    });

                SfaDebug.Print($"QuestStateUpdate (UserFleet = {fleet.Id}, Quest = {quest.Id}, State = {quest.State})", "DiscoveryServerClient");
            }
        }


        public void SendFleetAttacked(int objectId, DiscoveryObjectType type, SystemHex hex)
        {
            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.Attacked,
                writer =>
                {
                    writer.WriteInt32(objectId);
                    writer.WriteByte((byte)type);
                    writer.WriteHex(hex);
                });
        }


        public void SendConnectToInstance(string address, int port, string auth)
        {
            if (address is null || auth is null || port < 0 || port > ushort.MaxValue)
                return;

            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.Instance,
                writer =>
                {
                    writer.WriteShortString(address, -1, true, Encoding.ASCII);
                    writer.WriteUInt16((ushort)port);
                    writer.WriteShortString(auth, -1, true, Encoding.ASCII);
                });
        }

        public void SendOnScreenNotification(SfaNotification notification)
        {
            if (notification is null)
                return;

            var format = notification.Format ?? new();

            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.AddOnScreenNotification,
                writer =>
                {
                    writer.WriteShortString(notification.Id ?? string.Empty, -1, true, Encoding.UTF8); // Id
                    writer.WriteShortString(notification.Header ?? string.Empty, -1, true, Encoding.UTF8); // TitleId
                    writer.WriteShortString(notification.Text ?? string.Empty, -1, true, Encoding.UTF8); // MessageId
                    writer.WriteSingle(notification.LifeTime); // LifeTime
                    writer.WriteByte((byte)notification.Type); // Type


                    writer.WriteUInt16((ushort)format.Count);

                    foreach (var item in format.Keys)
                        writer.WriteShortString(item ?? string.Empty, -1, true, Encoding.UTF8); // FormatTag

                    writer.WriteUInt16((ushort)format.Count);

                    foreach (var item in format.Values)
                        writer.WriteShortString(item ?? string.Empty, -1, true, Encoding.UTF8); // FormatValue
                });
        }


        public void SendTalkingHead(SfaNotification notification)
        {
            if (notification is null)
                return;

            var format = notification.Format ?? new();

            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.ShowTalkingHead,
                writer =>
                {
                    writer.WriteShortString(notification.Text ?? string.Empty, -1, true, Encoding.UTF8); // Id

                    writer.WriteUInt16((ushort)format.Count);

                    foreach (var item in format.Keys)
                        writer.WriteShortString(item ?? string.Empty, -1, true, Encoding.UTF8); // FormatTag

                    writer.WriteUInt16((ushort)format.Count);

                    foreach (var item in format.Values)
                        writer.WriteShortString(item ?? string.Empty, -1, true, Encoding.UTF8); // FormatValue
                });
        }

        public void SendSessionDropDone()
        {
            Client?.Send(new JsonObject { }, SfaServerAction.DropSession);
            SendGalaxyMessage(DiscoveryServerGalaxyAction.SessionDropDone);
        }

        public void SendFleetRecallStateUpdate(FleetRecallState state, float time, int slotId)
        {
            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.FleetRecallStateUpdate,
                writer =>
                {
                    writer.WriteInt32((int)state);
                    writer.WriteSingle(time);
                    writer.WriteInt32(slotId);
                });
        }

        public void SendDiscoverySessionEnded(DiscoveryFleet fleet)
        {
            Client?.Send(new JsonObject { }, SfaServerAction.EndSession);
            SendDiscoveryMessage(fleet, DiscoveryServerAction.SessionEnded);
        }

        public void SendBattleGroundMatchFinded()
        {
            SendBattleGroundMessage(BattleGroundServerAction.InstanceDone);
        }

        public void SendBattleGroundState(
            MatchMakingStage stage = MatchMakingStage.Nothing,
            MatchMakingResetReason resetReason = MatchMakingResetReason.Unknown)
        {
            SendBattleGroundMessage(
                BattleGroundServerAction.StageUpdated,
                writer =>
            {
                writer.WriteByte((byte)stage);
                writer.WriteByte((byte)resetReason);
            });
        }


        public void SendBattleGroundInstanceReady(string address, int port, string auth)
        {
            SendBattleGroundMessage(
                BattleGroundServerAction.InstanceReady,
                writer =>
                {
                    writer.WriteShortString(address, -1, true, Encoding.ASCII);
                    writer.WriteUInt16((ushort)port);
                    writer.WriteShortString(auth, -1, true, Encoding.ASCII);
                });
        }

        public virtual void RequestDiscoveryObjectSync(StarSystemObject obj)
        {
            if (obj is null)
                return;

            RequestDiscoveryObjectSync(obj.System?.Id ?? -1, obj.Type, obj.Id);
        }

        public virtual void RequestDiscoveryObjectSync(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            SendToDiscoveryChannel(writer =>
            {
                writer.WriteInt32(-1);
                writer.WriteInt32(systemId);
                writer.WriteByte((byte)objectType);
                writer.WriteInt32(objectId);
            });
        }
    }
}
