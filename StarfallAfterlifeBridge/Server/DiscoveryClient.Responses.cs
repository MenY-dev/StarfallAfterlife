using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
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

        public virtual void SendFleetWarpedGateway(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            SendDiscoveryMessage(
                systemId, objectType, objectId,
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


        public virtual void SendFleetWarpedMothership(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            SendDiscoveryMessage(
                systemId, objectType, objectId,
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
                        writer.WriteShortString(item.UniqueData ?? "", -1, true, Encoding.UTF8); // UniqueData
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
            var scanned = false;

            if (selection is not null)
            {
                scanned = selection.Scanned;

                foreach (var item in selection.Objects)
                {
                    if (item is null)
                        continue;

                    var obj = item.Target;
                    var objInfo = new JsonArray();
                    var actions = new JsonArray();

                    scanned |= item.Scanned;

                    if (obj is Planet planet)
                    {
                        int phase = 0;

                        objInfo.Add(new JsonObject
                        {
                            ["info_type"] = "planet_base",
                            ["phase"] = phase,
                            ["planet_name"] = Server.Variable?.RenamedPlanets?.GetValueOrDefault(planet.Id)?.Name ?? planet.Name,
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

                            foreach (var ore in asteroid.CurrentOres ?? new())
                            {
                                var count = ore.Value;

                                switch (ore.Key)
                                {
                                    case 1300860703: asteroidInfo["arkonit"] = count; break;
                                    case 1937431386: asteroidInfo["adamantane"] = count; break;
                                    case 684682126: asteroidInfo["neitherium"] = count; break;
                                    case 1310293910: asteroidInfo["ro"] = count; break;
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
                    else if (obj is SecretObject secret)
                    {
                        if (selection.Scanned == true)
                        {
                            objInfo.Add(new JsonObject
                            {
                                ["info_type"] = "secret_stash",
                                ["phase"] = 0,
                            });
                        }
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

            if (selection.Star is GalaxyMapStarSystem star)
            {
                var starInfo = new JsonObject
                {
                    ["info_type"] = "star",
                    ["name"] = star.Name,
                    ["star_id"] = star.Id,
                    ["phase"] = 0,
                };

                var objInfo = new JsonObject
                {
                    ["id"] = star.Id,
                    ["type"] = (int)DiscoveryObjectType.None,
                    ["obj_info"] = new JsonArray(starInfo),
                };

                var renameInfo = Server?.Realm?.Variable?.RenamedSystems?.GetValueOrDefault(star.Id);

                if (renameInfo is not null)
                {
                    starInfo["name"] = renameInfo.Name;
                    starInfo["renamed_by_user_id"] = 123;
                    starInfo["renamed_by_user_name"] = renameInfo.Char;
                    objInfo["actions"] = new JsonArray { "report_star_name" };
                }
                else
                {
                    objInfo["actions"] = new JsonArray { "rename_star" };
                }

                info.Add(objInfo);
            }

            if (selection.ScanningStarted == true)
            {
                process["process_type"] = "scaning_sector";
                process["time_left"] = selection.ScanningTime;
            }

            var doc = new JsonObject
            {
                ["hexx"] = selection.Hex.X,
                ["hexy"] = selection.Hex.Y,
                ["info"] = info
            };

            if (scanned == true)
                doc["scan_finished"] = 1;

            if (string.IsNullOrEmpty((string)process["process_type"]) == false)
                doc["process"] = process;

            SendDiscoveryMessage(
                CurrentCharacter?.Fleet, DiscoveryServerAction.InfoWidgetData,
                writer => writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8));

            SfaDebug.Print($"InfoWidgetData (SystemId = {selection?.SystemId}, Location = {selection?.Hex}", GetType().Name);
        }

        public void SendSecretObjectRevealed(StarSystemObject obj)
        {
            SendDiscoveryMessage(obj, DiscoveryServerAction.SecretObjectRevealed);
        }

        public void SendQuestDialog(DiscoveryQuest quest)
        {
            if (quest is null)
                return;

            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestDataUpdate,
                writer =>
                {
                    var bindings = JsonHelpers.ParseNodeUnbuffered(quest.CreateBindings());
                    var reward = quest.Reward;

                    var doc = new JsonObject()
                    {
                        ["quest_dialog"] = new JsonObject
                        {
                            ["id"] = quest.Id,
                            ["entity"] = quest.Id,
                            ["level"] = quest.Level,
                            ["faction"] = (byte)quest.ObjectFaction,
                            ["is_quest_dialog"] = 1,
                            ["state"] = (byte)QuestState.InProgress,
                            ["quest_logic"] = quest.LogicId,
                            ["quest_params"] = new JsonObject
                            {
                                ["condition_params"] = quest.Conditions.Clone() ?? new JsonArray(),
                                ["reward"] = new JsonObject
                                {
                                    ["igc"] = reward.IGC,
                                    ["house_currency"] = reward.HouseCurrency,
                                    ["xp"] = reward.Xp,
                                    ["items"] = new JsonArray(reward.Items?.Select(i => new JsonObject
                                    {
                                        ["item"] = i.Id,
                                        ["count"] = i.Count
                                    })?.ToArray() ?? Array.Empty<JsonNode>())
                                },
                            },
                            ["bindings"] = bindings,
                        },
                    };

                    writer.WriteShortString(doc.ToJsonString(), -1, true, Encoding.UTF8);
                });

            SfaDebug.Print($"SendQuestDialog (QuestId = {quest.Id})", "DiscoveryServerClient");
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
                            var reward = listener.Info.Reward;
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
                                    ["reward"] = new JsonObject
                                    {
                                        ["igc"] = reward.IGC,
                                        ["house_currency"] = reward.HouseCurrency,
                                        ["xp"] = reward.Xp,
                                        ["items"] = new JsonArray(reward.Items?.Select(i => new JsonObject
                                        {
                                            ["item"] = i.Id,
                                            ["count"] = i.Count
                                        })?.ToArray() ?? Array.Empty<JsonNode>())
                                    },
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
            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.QuestCompleteData,
                writer =>
                {
                    writer.WriteInt32(quest.Id);
                    writer.WriteInt32((byte)quest.State);
                });

            SfaDebug.Print($"QuestStateUpdate (UserFleet = {CurrentCharacter?.Fleet?.Id}, Quest = {quest.Id}, State = {quest.State})", "DiscoveryServerClient");
        }

        public void SendUpdateInventory()
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.UpdateInventory);
            SfaDebug.Print("UpdateInventory");
        }

        public void SendInventoryNewItems(ICollection<InventoryItem> newItems)
        {
            var items = newItems?.ToList() ?? new();

            SendGalaxyMessage(DiscoveryServerGalaxyAction.InventoryNewItems, writer =>
            {
                writer.WriteUInt16((ushort)items.Count); // Count

                foreach (var item in items)
                {
                    writer.WriteByte((byte)item.Type); // ItemType
                    writer.WriteInt32(item.Id); // Id
                    writer.WriteInt32(item.Count); // Count
                    writer.WriteInt32(item.IGCPrice); // IGCPrice
                    writer.WriteInt32(item.BGCPrice); // BGCPrice
                    writer.WriteShortString(item.UniqueData ?? "", -1, true, Encoding.UTF8); // UniqueData
                }
            });

            SfaDebug.Print("InventoryNewItems");
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

        public void SendQuestLimitNotification(int questId = 0)
        {
            SendOnScreenNotification(new SfaNotification
            {
                Id = "accept_quest" + questId,
                Header = "ReachedQuestLimit",
                Format = new()
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


        public void SendShowAiMessage(DiscoveryObjectType senderType, int senderId, string msg, Dictionary<string, string> format = null)
        {
            format ??= new Dictionary<string, string>();

            SendDiscoveryMessage(
                CurrentCharacter?.Fleet,
                DiscoveryServerAction.ShowAiMessage,
                writer =>
                {
                    writer.WriteShortString(msg ?? string.Empty, -1, true, Encoding.UTF8); // msg
                    writer.WriteByte((byte)senderType); // SenderType
                    writer.WriteInt32(senderId); // SenderId

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

        public void SendStarRenamed(int starId, string newName, string charName)
        {
            SendGalaxyMessage(
                DiscoveryServerGalaxyAction.StarRenamed,
                writer =>
                {
                    var name = newName ?? Galaxy?.Map?.GetSystem(starId)?.Name ?? string.Empty;
                    writer.WriteInt32(starId);
                    writer.WriteShortString(name, -1, true, Encoding.UTF8);
                    writer.WriteShortString(charName ?? string.Empty, -1, true, Encoding.UTF8);
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
