﻿using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Server.Quests;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Inventory;
using StarfallAfterlife.Bridge.Server.Characters;
using System.Collections;
using System.Text.Json.Nodes;
using System.Threading.Channels;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public void OnTextReceive(string text, SfaServerAction action)
        {
            switch (action)
            {
                case SfaServerAction.Chat:
                    InputFromChat(text ?? string.Empty);
                    break;
                case SfaServerAction.RegisterChannel:
                    ProcessRegisterChannel(JsonHelpers.ParseNodeUnbuffered(text));
                    break;
                case SfaServerAction.TakeCharactRewardFromQueue:
                    ProcessTakeCharactRewardFromQueue(JsonHelpers.ParseNodeUnbuffered(text));
                    break;
                case SfaServerAction.SyncCharacterData:
                    ProcessSyncCharacterData(JsonHelpers.ParseNodeUnbuffered(text));
                    break;
                default:
                    break;
            }
        }

        internal void OnRequestReceive(SfaClientRequest request)
        {
            switch (request.Action)
            {
                case SfaServerAction.SyncCharacterSelect:
                    ProcessCharacterSelect(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;
                case SfaServerAction.GetFullGalaxySessionData:
                    ProcessGetFullGalaxySesionData(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;
                default:
                    break;
            }
        }

        public void InputFromDiscoveryChannel(SfReader reader)
        {
            var systemId = reader.ReadInt32();
            var objectType = (DiscoveryObjectType)reader.ReadByte();
            var objectId = reader.ReadInt32();
            var action = (DiscoveryClientAction)reader.ReadInt32();

            SfaDebug.Print($"InputFromDiscoveryChannel (SystemId = {systemId}, ObjectType = {objectType}, ObjectId = {objectId}, Action = {action})", "DiscoveryServerClient");

            switch (action)
            {
                case DiscoveryClientAction.EnterToGalaxy:
                    HandleEnterToGalaxy(); break;

                case DiscoveryClientAction.SwitchViewToStarSystem:
                    HandleSwitchViewToStarSystem(reader); break;

                case DiscoveryClientAction.RequestSync:
                    SyncDiscoveryObject(systemId, objectType, objectId); break;

                case DiscoveryClientAction.AbilitySync:
                    HandleAbilitySync(); break;

                case DiscoveryClientAction.MoveFleet:
                    HandleMoveFleet(reader, systemId); break;

                case DiscoveryClientAction.MoveFleetToTarget:
                    HandleMoveFleetToTarget(reader, systemId); break;

                case DiscoveryClientAction.SetSelection:
                    HandleSelection(reader, systemId); break;

                case DiscoveryClientAction.WarpGatewayFleet:
                    HandleWarpGatewayFleet(systemId); break;

                case DiscoveryClientAction.WarpToStarSystem:
                    HandleWarpToStarSystem(reader, systemId); break;

                case DiscoveryClientAction.SendInventoryToMotherhsip:
                    HandleSendInventoryToMotherhsip(reader, systemId); break;

                case DiscoveryClientAction.ProcessLocationAction:
                    HandleProcessLocationAction(reader, systemId); break;

                case DiscoveryClientAction.DockToObject:
                    HandleDockToObject(reader); break;

                case DiscoveryClientAction.UndockFromObject:
                    HandleUndockFromObject(reader); break;

                case DiscoveryClientAction.RequestStockContent:
                    HandleRequestStockContent(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.ItemsToStock:
                    HandleSendItemsToStock(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.RequestQuestDialog:
                    HandleRequestQuestDialog(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.AcceptQuest:
                    HandleAcceptQuest(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.AbandoneQuest:
                    HandleAbandoneQuest(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.DeliverQuestItems:
                    HandleDeliverQuestItems(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.QuestAction:
                    HandleQuestAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.FinishQuest:
                    HandleFinishQuest(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.Explore:
                    HandleExploreSystemHex(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SetTarget:
                    HandleSetTarget(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SessionEnd:
                    HandleSessionEnd(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.DropSession:
                    HandleSessionDrop(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.RepairAndRefuel:
                    HandleRepairAndRefuel(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.FleetRecall:
                    HandleFleetRecall(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.StartScan:
                    HandleStartScan(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.ActivateAbility:
                    HandleActivateAbility(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SecretObjectLooted:
                    HandleSecretObjectLooted(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.RenamePlanet:
                    HandleRenamePlanet(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.RenameStar:
                    HandleRenameStar(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.ReportPlanetName:
                    HandleReportPlanetName(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.ReportStarName:
                    HandleReportStarName(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SetTutorialStage:
                    HandleSetTutorialStage(reader, systemId, objectType, objectId); break;

                default:
                    InputFromDiscoveryHousesChannel(reader, action, systemId, objectType, objectId);
                    break;
            }
        }

        private void ProcessRegisterChannel(JsonNode doc)
        {
            var channelName = (string)doc?["name"];

            if ("Galactic".Equals(channelName, StringComparison.InvariantCultureIgnoreCase) == true)
                SendQuestDataUpdate();
        }

        public void InputFromBattleGroundChannel(SfReader reader)
        {
            var action = (BattleGroundAction)reader.ReadByte();

            SfaDebug.Print($"InputFromBattleGroundChannel (Action = {action})", "DiscoveryServerClient");

            switch (action)
            {
                case BattleGroundAction.FindMatch:
                    HandleBattleGroundFindMatch(); break;

                case BattleGroundAction.ReadyToPlay:
                    HandleBattleGroundReadyToPlay(); break;

                case BattleGroundAction.Cancel:
                    HandleBattleGroundCancel(); break;
            }
        }

        public void InputFromGalacticChannel(SfReader reader)
        {
            byte[] data = reader.ReadToEnd();
            SfaDebug.Print(BitConverter.ToString(data).Replace("-", ""), "InputFromGalacticChannel");
        }

        public void InputFromChat(string text)
        {
            var doc = JsonHelpers.ParseNode(text);

            if (doc is not JsonObject)
                return;

            Invoke(() =>
            {
                var channel = (string)doc["channel"];
                var isGeneralChat = channel == "GeneralTextChat";
                var msg = (string)doc["msg"];
                var isPrivate = (bool?)doc["is_private"];
                var receiver = (string)doc["receiver"];
                var sender = isGeneralChat ?
                    Client.UniqueName ?? "" :
                    CurrentCharacter?.UniqueName ?? "";

                if (msg?.StartsWith('\\') == true)
                {
                    Client?.HandleDebugConsoleInput(channel, msg[1..]);
                    return;
                }

                if (isPrivate == false)
                {
                    Server.UseClients(clients =>
                    {
                        if (channel.StartsWith("Star System") == true &&
                            CurrentCharacter?.Fleet?.System is StarSystem system)
                        {
                            foreach (var item in clients)
                            {
                                if (item.IsPlayer == true &&
                                    item.DiscoveryClient.CurrentCharacter?.Fleet?.System == system)
                                {
                                    item.SendToChat(channel, sender, msg, false);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in clients)
                                if (item.IsPlayer == true)
                                    item.SendToChat(channel, sender, msg, false);
                        }
                    });
                }
                else if (receiver is not null and { Length: > 0})
                {

                    if (isGeneralChat)
                    {
                        Server?.GetPlayer(receiver)?
                            .SendToChat("GeneralTextChat", sender, msg, true);
                    }
                    else if (Server?.GetCharacter(receiver) is ServerCharacter character)
                    {
                        var currentChannel = character.Faction switch
                        {
                            Faction.Deprived => "Deprived Chat",
                            Faction.Eclipse => "Eclipse Chat",
                            Faction.Vanguard => "Vanguard Chat",
                            _ => "GeneralTextChat",
                        };

                        character?.DiscoveryClient?.Client?
                            .SendToChat(currentChannel, sender, msg, true);
                    }
                }
            });
        }

        protected virtual void HandleEnterToGalaxy()
        {
            EnterToGalaxy();
        }

        protected virtual void HandleSwitchViewToStarSystem(SfReader reader)
        {
            int systemId = reader.ReadInt32();
            SfaDebug.Print($"HandleSwitchViewToStarSystem (SystemId = {systemId})");

            if (CurrentCharacter?.GetSystemCustomInstances(systemId) is CustomInstance[] customInstances)
            {
                foreach (var instance in customInstances)
                {
                    Invoke(c =>
                    {
                        c.RequestDiscoveryObjectSync(systemId, DiscoveryObjectType.CustomInstance, instance.Id);
                        c.SyncCustomInstance(instance);
                    });
                }
            }

            Galaxy.BeginPreUpdateAction(g =>
            {
                if (CurrentCharacter?.Fleet is DiscoveryFleet fleet)
                {
                    if (g.GetActiveSystem(systemId, true) is StarSystem system)
                    {
                        foreach (var item in system.Fleets)
                        {
                            if (item is null || item.State == FleetState.Destroyed)
                                continue;

                            Invoke(() =>
                            {
                                RequestDiscoveryObjectSync(item);
                                SyncFleetData(item);
                            });
                        }

                        foreach (var obj in system.GetAllObjects(false))
                        {
                            if (obj is StarSystemDungeon dungeon &&
                                dungeon.IsDungeonVisible == false)
                                continue;

                            Invoke(() =>
                            {
                                if (obj is SecretObject secret &&
                                    CurrentCharacter?.Progress?.SecretLocs?.Contains(secret.Id) == true)
                                    return;

                                RequestDiscoveryObjectSync(obj);
                                SyncDiscoveryObject(systemId, obj.Type, obj.Id);
                            });
                        }
                    }
                }
            });
#if DEBUG
            if (Galaxy?.Map.GetSystem(systemId) is GalaxyMapStarSystem systemInfo)
            {
                string text = $"Factiom: {(Faction)systemInfo.Faction}\n";

                text += $"FactionGroup: {systemInfo.FactionGroup}\n";
                //text += $"Convexhull: {systemInfo.Convexhull}\n";
                //text += $"Out: {systemInfo.Out}\n";

                //if (systemInfo.PiratesStations is not null && systemInfo.PiratesStations.Count > 0)
                //{
                //    text += $"PiratesStations:\n";
                //    text += "[\n";
                //    foreach (var item in systemInfo.PiratesStations)
                //    {
                //        text += "  {\n";
                //        text += $"     Id: {item.Id}\n";
                //        text += $"     Hex: {item.Hex}\n";
                //        text += $"     Level: {item.Level}\n";
                //        text += $"     Factiom: {(Faction)systemInfo.Faction}\n";
                //        text += $"     FactionGroup: {systemInfo.FactionGroup}\n";
                //        text += "  }\n";
                //    }
                //    text += "]\n";
                //}

                //if (systemInfo.Planets is not null && systemInfo.Planets.Count > 0)
                //{
                //    text += $"Planets:\n";
                //    text += "[\n";
                //    foreach (var item in systemInfo.Planets)
                //    {
                //        text += $"   Name: {item.Name}\n";
                //        text += $"   Faction: {item.Faction}\n\n";
                //    }
                //    text += "]\n";
                //}

                //if (systemInfo.Portals is not null && systemInfo.Portals.Count > 0)
                //{
                //    text += $"Portals:\n";
                //    text += "[\n";
                //    foreach (var item in systemInfo.Portals)
                //    {
                //        text += $"   Destination: {item.Destination}\n";
                //    }
                //    text += "]\n";
                //}

                if (Galaxy?.GetActiveSystem(systemId)?.SecretObjects is List<SecretObject> secrets &&
                    secrets.Count > 0)
                {
                    text += $"Secret Objects:\n";
                    text += "[\n";
                    foreach (var item in secrets)
                        text += $"   {item.SecretType}:{item.Id}\n";
                    text += "]\n";
                }

                try
                {
                    text += $"Fleets:\n";
                    text += "[\n";

                    foreach (var item in CurrentCharacter?.Fleet?.System?.Fleets ?? new())
                    {
                        text += $"   {item.Name} [{item.Id}]\n";
                    }

                    text += "]\n";
                }
                catch { }

                SendOnScreenNotification(new SfaNotification
                {
                    Id = "SystemInfo",
                    Header = $"System Info ({systemInfo.Id})",
                    Text = text,
                    LifeTime = 6000
                });
            }
#endif
        }

        protected virtual void HandleAbilitySync()
        {
            SyncAbilities(CurrentCharacter?.Fleet);
        }


        private void HandleMoveFleet(SfReader reader, int systemId)
        {
            SystemHex hex = reader.ReadHex();
            Vector2 location = SystemHexMap.HexToSystemPoint(hex);

            Galaxy.BeginPreUpdateAction(g =>
            {
                if (CurrentCharacter?.Fleet is UserFleet fleet &&
                    fleet.System is StarSystem system &&
                    system.Id == systemId &&
                    system.ObstacleMap?[hex] == false)
                    fleet.MoveTo(location);
            });
        }

        protected void HandleMoveFleetToTarget(SfReader reader, int systemId)
        {
            Vector2 location = reader.ReadVector2();
            int val1 = reader.ReadInt32();
            SystemHex hex = SystemHexMap.SystemPointToHex(location);

            Galaxy.BeginPreUpdateAction(g =>
            {
                if (CurrentCharacter?.Fleet is UserFleet fleet &&
                    fleet.System is StarSystem system &&
                    system.Id == systemId &&
                    system.ObstacleMap?[hex] == false)
                    fleet.MoveTo(location);
            });
        }

        private void HandleSelection(SfReader reader, int systenId)
        {
            SystemHex hex = reader.ReadHex();
            DiscoveryObjectType type = (DiscoveryObjectType)reader.ReadByte();
            int objectId = reader.ReadInt32();

            Galaxy.BeginPreUpdateAction(g =>
            {
                SelectionInfo info = null;

                if (objectId > -1 && type != DiscoveryObjectType.None &&
                    CurrentCharacter?.Fleet?.System?.GetObject(objectId, type) is StarSystemObject selectedObject)
                {
                    info = new SelectionInfo()
                    {
                        Hex = hex,
                        SystemId = systenId,
                    };

                    info.Objects.Add(new() { Target = selectedObject });
                }
                else if (Galaxy?.GetActiveSystem(systenId) is StarSystem system)
                {
                    info = new SelectionInfo()
                    {
                        Hex = hex,
                        SystemId = systenId,
                    };

                    if (system.GetObjectsAt(hex) is IEnumerable<StarSystemObject> objects)
                    {
                        foreach (var item in objects)
                        {
                            if (item is SecretObject secret &&
                                CurrentCharacter?.Progress?.SecretLocs?.Contains(secret.Id) == true)
                                continue;

                            info.Objects.Add(new() { Target = item });
                        }
                    }

                    if (system.IsStarHex(hex) == true)
                    {
                        info.Star = system.Info ?? Map?.GetSystem(systenId);
                    }
                }

                Invoke(() =>
                {
                    if (CurrentCharacter is ServerCharacter character)
                        character.Selection = info;

                    SendInfoWidgetData(info);
                });
            });
        }

        private void HandleWarpGatewayFleet(int systemId)
        {
            var character = CurrentCharacter;
            var fleet = character?.Fleet;

            if (character is null || fleet is null)
                return;

            SyncExploration(new[]{ fleet.System?.Id ?? -1 });

            Galaxy?.BeginPreUpdateAction(g =>
            {
                var fleetHex = SystemHexMap.SystemPointToHex(fleet.Location);
                var currentSystem = fleet.System;

                if (currentSystem is null ||
                    currentSystem.Id != systemId ||
                    fleet.GetBattle() is not null)
                    return;

                var portal = currentSystem.GetObjectAt(fleet.Hex, DiscoveryObjectType.WarpBeacon) as WarpBeacon;

                if (portal is null)
                    return;

                if (portal is not null &&
                    Map?.GetSystem(portal.Destination) is GalaxyMapStarSystem mapSystem)
                {
                    if (mapSystem.Level > character.AccessLevel)
                    {
                        Invoke(() => SendOnScreenNotification(new SfaNotification
                        {
                            Id = $"error_system_lvl_{mapSystem.Level}",
                            Header = "AccessDeniedLowAccessLevel",
                            Type = SfaNotificationType.Error
                        }));
                    }
                    else if (g.ActivateStarSystem(portal.Destination) is StarSystem newSystem)
                    {
                        var newLocation = SystemHexMap.HexToSystemPoint(portal.Hex).GetNegative();

                        fleet.AddEffect(new() { Duration = 6, Logic = GameplayEffectType.Immortal });
                        currentSystem.RemoveFleet(fleet);
                        Invoke(() =>
                        {
                            SendFleetWarpedGateway(currentSystem.Id, fleet.Type, fleet.Id);
                            character.AddNewStats(new() { { "Stat.FleetActions.WarpOnVortex", 1 } });
                        });
                        newSystem.AddFleet(fleet, newLocation);

                    }
                }
            });
        }

        private void HandleWarpToStarSystem(SfReader reader, int systemId)
        {
            int warpingSourceId = reader.ReadInt32();
            int targetSystem = reader.ReadInt32();
            var character = CurrentCharacter;
            var fleet = character?.Fleet;

            if (character is null || fleet is null)
                return;

            SyncExploration(new[] { fleet.System?.Id ?? -1 });

            Galaxy?.BeginPreUpdateAction(g =>
            {
                var currentSystem = fleet.System;

                if (currentSystem is null ||
                    currentSystem.Id != systemId ||
                    fleet.GetBattle() is not null)
                    return;

                if (Map?.GetSystem(targetSystem) is GalaxyMapStarSystem mapSystem &&
                    mapSystem.Level > character.AccessLevel)
                {
                    Invoke(() => SendOnScreenNotification(new SfaNotification
                    {
                        Id = $"error_system_lvl_{mapSystem.Level}",
                        Header = "AccessDeniedLowAccessLevel",
                        Type = SfaNotificationType.Error
                    }));

                    return;
                }

                var newSystem = g.ActivateStarSystem(targetSystem);

                if (newSystem is null || currentSystem is null)
                    return;

                var gate = newSystem.QuickTravelGates?.FirstOrDefault();

                if (gate is null)
                    return;

                currentSystem.RemoveFleet(fleet);

                Invoke(() =>
                {
                    var systemInfo = g?.Map?.GetSystem(systemId);
                    var cost = systemInfo?.Faction == fleet.Faction ?
                        0 : SfaDatabase.GetWarpingCost(systemInfo?.Level ?? 0);

                    CurrentCharacter?.AddCharacterCurrencies(igc: -cost);
                    SendFleetWarpedMothership(currentSystem.Id, fleet.Type, fleet.Id);
                    character.AddNewStats(new() { { "Stat.FleetActions.WarpOnGateway", 1 } });
                });

                g.BeginPostUpdateAction(g => newSystem.AddFleet(fleet, gate.Location));
            });
        }

        private void HandleSendInventoryToMotherhsip(SfReader reader, int systemId)
        {
            Invoke(() =>
            {
                if (CurrentCharacter is ServerCharacter character)
                {
                    var ships = character.Ships;
                    var system = Galaxy?.Map?.GetSystem(systemId);
                    var cost = SfaDatabase.GetWarpingCost(system?.Level ?? 0);
                    var dst = CargoTransactionEndPoint.CreateForCharacterInventory(character);
                    var result = 0;

                    if (ships is null || dst is null)
                        return;

                    foreach (var ship in ships.Where(s => s is not null).ToArray())
                    {
                        var cargo = ship?.Cargo;

                        if (cargo is null)
                            continue;

                        var src = CargoTransactionEndPoint.CreateForCharacterStoc(
                            character, character.CreateShipStocName(ship.Id));

                        if (src is null)
                            continue;

                        foreach (var item in cargo.Where(i => i.IsEmpty == false).ToArray())
                        {
                            result += src.SendItemTo(dst, item.Id, item.Count, item.UniqueData);
                        }
                    }

                    if (result > 0)
                    {
                        if (character.Faction != system?.Faction)
                            character.AddCharacterCurrencies(igc: -cost);

                        SyncSessionFleetInfo();
                        SendFleetCargo();
                        character.AddNewStats(new() { { "Stat.FleetActions.SendCargoToMotherhsip", 1 } });
                    }
                }
            });
        }


        private void HandleProcessLocationAction(SfReader reader, int systemId)
        {
            var locName = reader.ReadShortString(Encoding.UTF8);
            var data = reader.ReadShortString(Encoding.UTF8);

            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet is DiscoveryFleet fleet &&
                fleet.System is StarSystem system)
            {
                Invoke(() =>
                {
                    if (locName == "fd")
                    {
                        var cost = SfaDatabase.GetRefuelCost(Galaxy?.Map?.GetSystem(systemId)?.Level ?? 0);
                        character.AddCharacterCurrencies(igc: -cost);

                        Galaxy?.BeginPreUpdateAction(g =>
                        {
                            fleet.AddEffect(new FleetEffectInfo()
                            {
                                Duration = 600,
                                Logic = GameplayEffectType.FuelStationBoost,
                                EngineBoost = 1.5f,
                            });
                        });
                    }
                });
            }
        }


        private void HandleDockToObject(SfReader reader)
        {
            int id = reader.ReadInt32();
            DiscoveryObjectType type = (DiscoveryObjectType)reader.ReadByte();

            var fleet = CurrentCharacter?.Fleet;

            if (fleet is null)
                return;

            Galaxy?.BeginPreUpdateAction(g =>
            {
                if (fleet?.System is StarSystem system &&
                    system.GetObject(id, type) is StarSystemObject obj &&
                    fleet.GetBattle() is null)
                {
                    fleet.DockObjectId = id;
                    fleet.DockObjectType = type;

                    Invoke(() =>
                    {
                        RequestDiscoveryObjectSync(fleet);
                        SyncFleetData(fleet);
                        SyncObjectInfo(obj);

                        SfaDebug.Print($"DockToObject (Id = {id}, Type = {type})", GetType().Name);
                    });
                }
            });
        }


        private void HandleUndockFromObject(SfReader reader)
        {
            var fleet = CurrentCharacter?.Fleet;

            if (fleet is null)
                return;

            Galaxy?.BeginPreUpdateAction(g =>
            {
                fleet.DockObjectId = -1;
                fleet.DockObjectType = DiscoveryObjectType.None;
                fleet.AddEffect(new() { Logic = GameplayEffectType.Immortal, Duration = 5 });
                Invoke(() => SyncFleetData(fleet));
            });
        }

        private void HandleRequestStockContent(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            string stockName = reader.ReadShortString(Encoding.UTF8);
            int otherObjectId = reader.ReadInt32();
            DiscoveryObjectType otherObjectType = (DiscoveryObjectType)reader.ReadByte();
            string otherStockName = reader.ReadShortString(Encoding.UTF8);

            if (objectType == DiscoveryObjectType.UserFleet)
            {
                if (CurrentCharacter is ServerCharacter character &&
                    character.Fleet is UserFleet fleet &&
                    fleet.Id == objectId &&
                    fleet.System is not null)
                {

                    if (character.GetShipCargoByStockName(stockName) is InventoryStorage cargo)
                    {
                        SendObjectStock(fleet, cargo, stockName);
                    }
                }
            }
            else
            {
                if (Galaxy?.ActivateStarSystem(systemId).GetObject(objectId, objectType) is StarSystemObject obj &&
                    Server.GetObjectShops(objectId, (GalaxyMapObjectType)objectType) is ObjectShops shopsInfo &&
                    shopsInfo.Shops.FirstOrDefault(s => s.StocName == stockName) is ShopInfo shop)
                {
                    SendObjectStock(obj, shop.Items, stockName);
                }
            }
        }

        private void HandleSendItemsToStock(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            string stockName = reader.ReadShortString(Encoding.UTF8);
            int itemsCount = reader.ReadInt16();
            List<InventoryItem> items = new();

            for (int i = 0; i < itemsCount; i++)
            {
                items.Add(new()
                {
                    Type = (InventoryItemType)reader.ReadByte(),
                    Id = reader.ReadInt32(),
                    Count = reader.ReadInt32(),
                    IGCPrice = reader.ReadInt32(),
                    BGCPrice = reader.ReadInt32(),
                    UniqueData = reader.ReadShortString(Encoding.UTF8) is string data && data.Length > 0 ? data : null,
                });
            }

            int srcObjectId = reader.ReadInt32();
            DiscoveryObjectType srcObjectType = (DiscoveryObjectType)reader.ReadByte();
            string srcStockName = reader.ReadShortString(Encoding.UTF8);

            Invoke(() =>
            {
                if (CurrentCharacter is ServerCharacter character &&
                character.Fleet is UserFleet fleet &&
                Server?.Realm.Database is SfaDatabase database)
                {
                    if (objectType == DiscoveryObjectType.UserFleet &&
                        objectId == fleet.Id)
                    {
                        if (srcObjectType != DiscoveryObjectType.UserFleet &&
                            Galaxy?.GetActiveSystem(systemId)?.GetObject(srcObjectId, srcObjectType) is StarSystemObject obj &&
                            Server.GetObjectShops(obj.Id, (GalaxyMapObjectType)obj.Type) is ObjectShops shopsInfo &&
                            shopsInfo?.Shops?.FirstOrDefault(s => s.StocName == srcStockName) is ShopInfo shop)
                        {
                            var dst = CargoTransactionEndPoint.CreateForCharacterFleet(character, stockName);
                            var currentIGC = character.IGC;
                            var totalPrice = 0;

                            foreach (var item in items)
                            {
                                if (currentIGC < item.IGCPrice)
                                    continue;

                                var possibleToBuy = currentIGC / item.IGCPrice;
                                var result = dst.Receive(item, Math.Min(item.Count, possibleToBuy));
                                var operationPrice = item.IGCPrice * result;
                                totalPrice += operationPrice;
                                currentIGC -= operationPrice;
                            }

                            character.AddCharacterCurrencies(igc: -totalPrice);
                            character.RaseStockUpdated();
                            SendFleetCargo();
                            SendObjectStock(obj, shop.Items, srcStockName);
                        }
                        else if (srcObjectId == objectId)
                        {
                            var src = CargoTransactionEndPoint.CreateForCharacterStoc(character, srcStockName);
                            var dst = srcStockName == "inventory" ?
                                CargoTransactionEndPoint.CreateForCharacterFleet(character, stockName) :
                                CargoTransactionEndPoint.CreateForCharacterStoc(character, stockName);

                            foreach (var item in items)
                            {
                                var result = src.SendItemTo(dst, item.Id, item.Count, item.UniqueData);
                            }

                            character.RaseStockUpdated();
                            SendFleetCargo();
                        }
                    }
                    else if (objectType != DiscoveryObjectType.UserFleet &&
                        srcObjectType == DiscoveryObjectType.UserFleet &&
                        srcObjectId == fleet.Id &&
                        Galaxy?.GetActiveSystem(systemId)?.GetObject(objectId, objectType) is StarSystemObject obj &&
                        Server?.GetObjectShops(obj.Id, (GalaxyMapObjectType)obj.Type) is ObjectShops shopsInfo &&
                        shopsInfo?.Shops?.FirstOrDefault(s => s.StocName == stockName) is ShopInfo shop)
                    {
                        var src = CargoTransactionEndPoint.CreateForCharacterStoc(character, srcStockName);
                        var dst = CargoTransactionEndPoint.CreateForShop(shop);
                        var totalPrice = 0;

                        foreach (var item in items)
                        {
                            var result = src.SendItemTo(dst, item.Id, item.Count, item.UniqueData);
                            var itemPrice = item.IGCPrice;

                            if (item.IGCPrice == 0 &&
                                database.GetItem(item.Id) is SfaItem blueprint)
                                itemPrice = blueprint.IGC;

                            totalPrice += Math.Max(0, itemPrice * result);
                        }

                        character.AddCharacterCurrencies(igc: totalPrice);
                        character.RaseStockUpdated();
                        SendFleetCargo();
                        SendObjectStock(obj, shop.Items, srcStockName);
                    }

                    SyncSessionFleetInfo();
                }
            });
        }

        private void HandleRequestQuestDialog(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            Invoke(c =>
            {
                if (Server?.Realm is SfaRealm realm &&
                    (realm.Database ?? SfaDatabase.Instance) is SfaDatabase realmDatabase &&
                    realm.QuestsDatabase is DiscoveryQuestsDatabase questsDatabase)
                {
                    var quest = questsDatabase.GetQuest(entityId) ??
                                CurrentCharacter?.Progress?.ActiveQuests?.GetValueOrDefault(entityId)?.QuestData;

                    if (quest is not null)
                        SendQuestDialog(quest);
                }
            });
        }


        private void HandleAcceptQuest(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            if (CurrentCharacter is ServerCharacter character)
            {
                character.AcceptQuest(entityId, true);
                SendQuestDataUpdate();
                character.UpdateQuestLines();
                character.UpdateDailyQuests();
            }
        }

        private void HandleAbandoneQuest(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            if (CurrentCharacter is ServerCharacter character)
            {
                character.AbandoneQuest(entityId);
                character.UpdateQuestLines();
                character.UpdateDailyQuests();
            }
        }

        private void HandleDeliverQuestItems(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            if (CurrentCharacter is ServerCharacter character)
            {
                Invoke(() =>
                {
                    character.ActiveQuests?
                       .FirstOrDefault(q => q.Id == entityId)?
                       .DeliverQuestItems();

                    SyncSessionFleetInfo();
                });
            }
        }

        private void HandleQuestAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var questId = reader.ReadInt32();
            var identity = reader.ReadShortString(Encoding.ASCII);
            var data = reader.ReadShortString(Encoding.ASCII);

            Invoke(() =>
            {
                foreach (var item in CurrentCharacter?.ActiveQuests ?? new())
                {
                    if (item?.Id == questId)
                        item.RaiseAction(identity, data);
                }
            });
        }

        private void HandleFinishQuest(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            if (CurrentCharacter is ServerCharacter character)
            {
                Invoke(() =>
                {
                    character.FinishQuest(entityId);
                    character.UpdateQuestLines();
                    character.UpdateDailyQuests();
                });
            }
        }


        private void HandleExploreSystemHex(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var hex = reader.ReadHex();

            Galaxy.BeginPreUpdateAction(g =>
            { 
                if (CurrentCharacter is ServerCharacter character &&
                    character.Fleet is UserFleet fleet &&
                    fleet.System is StarSystem system &&
                    fleet.Id == objectId &&
                    system.Id == systemId)
                {
                    var objectsInHex = system.GetObjectsAt(hex, false);

                    if (character.GetSystemCustomInstances(systemId).FirstOrDefault(i => i.Hex == hex) is CustomInstance instance)
                    {

                    }
                    else if (objectsInHex.FirstOrDefault(o => o is SecretObject) is SecretObject secret &&
                        character.Progress?.SecretLocs?.Contains(secret.Id) is null or false)
                    {
                        fleet.ExploreSecretObject(secret);
                    }
                    else
                    {
                        fleet.ExploreCurrentHex();
                    }
                }
            });

            SfaDebug.Print($"HandleExploreSystemHex (System = {systemId}, ObjType = {objectType}, ObjId = {objectId})", GetType().Name);
        }

        private void HandleSetTarget(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var targetObjectId = reader.ReadInt32();
            var targetObjectType = (DiscoveryObjectType)reader.ReadByte();

            if (targetObjectType == DiscoveryObjectType.CustomInstance &&
                CurrentCharacter is ServerCharacter character &&
                CurrentCharacter.GetCustomInstance(targetObjectId) is CustomInstance instance)
            {
                Galaxy.BeginPreUpdateAction(g =>
                {
                    var fleet = CurrentCharacter?.Fleet;

                    if (fleet is null ||
                        fleet.System is null ||
                        fleet.System.Id != systemId ||
                        fleet.Id != objectId)
                        return;

                    fleet.MoveTo(instance.Hex);
                });

                return;
            }

            Galaxy.BeginPreUpdateAction(g =>
            {
                var fleet = CurrentCharacter?.Fleet;

                if (fleet is null ||
                    fleet.System is null ||
                    fleet.System.Id != systemId ||
                    fleet.Id != objectId)
                    return;

                var targetObject = fleet.System.GetObject(targetObjectId, targetObjectType);
                fleet.SetAttackTarget(targetObject);
            });
        }

        private void HandleSessionEnd(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int mothershipId = reader.ReadInt32();
            EndGalaxySession();
        }

        private void HandleSessionDrop(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            DropGalaxySession();
        }

        private void HandleRepairAndRefuel(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var character = CurrentCharacter;

            if (character is not null)
                Invoke(() => character?.RepairAndRefuelFleet());
        }

        private void HandleFleetRecall(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var unusedVar = reader.ReadInt32();
            var slotId = reader.ReadInt32();
            var shipId = reader.ReadInt32();

            CurrentCharacter?.ReacallShip(shipId, slotId);
        }


        private void HandleStartScan(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var pos = reader.ReadHex();
            var type = (DiscoveryObjectType)reader.ReadByte();
            var id = reader.ReadInt32();

            Galaxy?.BeginPreUpdateAction(g =>
            {
                if (id > -1 &&
                    type is not (DiscoveryObjectType.None or DiscoveryObjectType.SecretObject) &&
                    CurrentCharacter?.Selection?.GetObjectInfo(id, type) is ObjectSelectionInfo info)
                {
                    CurrentCharacter?.Fleet?.ScanObject(id, type);
                }
                else
                {
                    CurrentCharacter?.Fleet?.ScanSector(pos);
                }
            });

            SfaDebug.Print($"StartScan (Id = {id}, Type = {type}), Pos = {pos})", GetType().Name);
        }

        public void HandleBattleGroundFindMatch()
        {
            Server?.Matchmaker?.MothershipAssaultGameMode?.AddToQueue(CurrentCharacter);
        }

        public void HandleBattleGroundReadyToPlay()
        {
            Server?.Matchmaker?.MothershipAssaultGameMode?.AcceptMatch(CurrentCharacter);
        }

        public void HandleBattleGroundCancel()
        {
            Server?.Matchmaker?.MothershipAssaultGameMode?.RemoveFromQueue(CurrentCharacter);
        }

        private void HandleActivateAbility(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var actionId = reader.ReadInt32();
            var hex = reader.ReadHex();
            var actionSystemId = reader.ReadInt32();

            CurrentCharacter?.UseAbility(actionId, actionSystemId, hex);
        }

        private void ProcessTakeCharactRewardFromQueue(JsonNode doc)
        {
            Invoke(() =>
            {
                if (doc is not null &&
                (int?)doc["char_id"] is int charId &&
                (int?)doc["reward_id"] is int rewardId)
                    Characters?.FirstOrDefault(c => c?.UniqueId == charId)?
                        .AddReward(rewardId);
            });
        }

        private void ProcessSyncCharacterData(JsonNode doc)
        {
            Invoke(() =>
            {
                if (doc is not null &&
                    (int?)doc["char_id"] is int charId &&
                    Characters?.FirstOrDefault(c => c?.UniqueId == charId) is ServerCharacter character)
                {
                    if (doc["detachments"]?.AsArraySelf() is JsonArray detachments)
                    {
                        character.SyncAbilities(detachments);
                    }
                }
            });
        }

        private void HandleSecretObjectLooted(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Invoke(() =>
            {

            });
        }

        private void HandleRenamePlanet(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var newName = reader.ReadShortString(Encoding.UTF8);
            var charName = CurrentCharacter?.Name;

            if (string.IsNullOrWhiteSpace(newName) ||
                string.IsNullOrWhiteSpace(charName))
                return;

            if (string.IsNullOrWhiteSpace(newName) == false &&
                Server?.RenamePlanet(objectId, newName, charName) == true)
            {
                CurrentCharacter?.AddCharacterCurrencies(igc: -5000);
            }
            else
            {
                SendDiscoveryMessage(systemId, objectType, objectId, DiscoveryServerAction.PlanetNamingFailed);
                RequestDiscoveryObjectSync(systemId, objectType, objectId);
                SyncDiscoveryObject(systemId, objectType, objectId);
            }
        }

        private void HandleRenameStar(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var newName = reader.ReadShortString(Encoding.UTF8);
            var charName = CurrentCharacter?.Name;

            if (string.IsNullOrWhiteSpace(newName) ||
                string.IsNullOrWhiteSpace(charName))
                return;

            if (string.IsNullOrWhiteSpace(newName) == false &&
                Server?.RenameSystem(systemId, newName, charName) == true)
            {
                CurrentCharacter?.AddCharacterCurrencies(igc: -10000);
            }
            else
            {
                SendDiscoveryMessage(systemId, objectType, objectId, DiscoveryServerAction.NamingFailed);
            }
        }

        private void HandleReportPlanetName(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Server?.ReportPlanetName(objectId, Client);
        }

        private void HandleReportStarName(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Server?.ReportSystemName(systemId, Client);
        }

        private void HandleSetTutorialStage(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var stage = (TutorialStage)reader.ReadByte();

            if (CurrentCharacter is ServerCharacter character)
                Invoke(_ => character.SetTutorialStage(systemId, stage));
        }
    }
}
