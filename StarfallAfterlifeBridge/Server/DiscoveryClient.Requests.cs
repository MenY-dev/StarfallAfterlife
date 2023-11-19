using StarfallAfterlife.Bridge.IO;
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

                case DiscoveryClientAction.FleetRecall:
                    HandleFleetRecall(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.StartScan:
                    HandleStartScan(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.ActivateAbility:
                    HandleActivateAbility(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SecretObjectLooted:
                    HandleSecretObjectLooted(reader, systemId, objectType, objectId); break;
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
                var msg = (string)doc["msg"];
                var isPrivate = (bool?)doc["is_private"];
                var receiver = (string)doc["receiver"];
                var sender = channel == "GeneralTextChat" ?
                    Client.Name ?? "" :
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
                else
                {
                    Server?.GetCharacter(receiver)?.DiscoveryClient?.Client?.SendToChat(channel, sender, msg, true);
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

                            RequestDiscoveryObjectSync(item);
                            SyncFleetData(item);
                        }

                        foreach (var obj in system.GetAllObjects(false))
                        {
                            if (obj is StarSystemDungeon dungeon &&
                                dungeon.IsDungeonVisible == false)
                                continue;

                            if (obj is SecretObject secret &&
                                CurrentCharacter?.Progress?.SecretLocs?.Contains(secret.Id) == true)
                                continue;

                            RequestDiscoveryObjectSync(obj);
                            SyncDiscoveryObject(systemId, obj.Type, obj.Id);
                        }
                    }
                }
            });

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
                else if (CurrentCharacter?.Fleet?.System?.GetObjectsAt(hex) is IEnumerable<StarSystemObject> objects)
                {
                    info = new SelectionInfo()
                    {
                        Hex = hex,
                        SystemId = systenId,
                    };

                    foreach (var item in objects)
                    {
                        if (item is SecretObject secret &&
                            CurrentCharacter?.Progress?.SecretLocs?.Contains(secret.Id) == true)
                            continue;

                        info.Objects.Add(new() { Target = item });
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
            var fleet = CurrentCharacter?.Fleet;

            if (fleet is null)
                return;

            SyncExploration(new[]{ fleet.System?.Id ?? -1 });

            Galaxy?.BeginPreUpdateAction(g =>
            {
                var fleetHex = SystemHexMap.SystemPointToHex(fleet.Location);
                var currentSystem = fleet.System;

                if (currentSystem is null || currentSystem.Id != systemId)
                    return;

                var portal = currentSystem.GetObjectAt(fleet.Hex, DiscoveryObjectType.WarpBeacon) as WarpBeacon;

                if (portal is not null &&
                    g.ActivateStarSystem(portal.Destination) is StarSystem newSystem)
                {
                    var newLocation = SystemHexMap.HexToSystemPoint(portal.Hex).GetNegative();

                    currentSystem.RemoveFleet(fleet);
                    Invoke(() => SendFleetWarpedGateway(currentSystem.Id, fleet.Type, fleet.Id));
                    newSystem.AddFleet(fleet, newLocation);
                }
            });
        }

        private void HandleWarpToStarSystem(SfReader reader, int systemId)
        {
            int warpingSourceId = reader.ReadInt32();
            int targetSystem = reader.ReadInt32();
            var fleet = CurrentCharacter?.Fleet;

            if (fleet is null)
                return;

            SyncExploration(new[] { fleet.System?.Id ?? -1 });

            Galaxy?.BeginPreUpdateAction(g =>
            {
                if (fleet.System?.Id != systemId)
                    return;

                var currentSystem = fleet.System;
                var newSystem = g.ActivateStarSystem(targetSystem);

                if (newSystem is null || currentSystem is null)
                    return;

                var gate = newSystem.QuickTravelGates?.FirstOrDefault();

                if (gate is not null)
                {
                    Invoke(() =>
                    {
                        var cost = SfaDatabase.GetWarpingCost(g?.Map?.GetSystem(systemId)?.Level ?? 0);
                        CurrentCharacter?.AddCharacterCurrencies(igc: -cost);

                        Galaxy.BeginPostUpdateAction(g =>
                        {
                            currentSystem.RemoveFleet(fleet);
                            newSystem.AddFleet(fleet, gate.Location);
                            Invoke(() => SendFleetWarpedMothership(currentSystem.Id, fleet.Type, fleet.Id));
                            newSystem.AddFleet(fleet);
                        });
                    });
                }
            });
        }

        private void HandleSendInventoryToMotherhsip(SfReader reader, int systemId)
        {
            Invoke(() =>
            {
                if (CurrentCharacter is ServerCharacter character)
                {
                    var ships = character.Ships;
                    var cost = SfaDatabase.GetWarpingCost(Galaxy?.Map?.GetSystem(systemId)?.Level ?? 0);
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
                        character.AddCharacterCurrencies(igc: -cost);

                        SynckSessionFleetInfo();
                        SendFleetCargo();
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
                system.GetObject(id, type) is StarSystemObject obj)
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
                        Server?.Realm?.ShopsMap?.GetObjectShops(obj.Id, obj.Type) is ObjectShops shopsInfo &&
                        shopsInfo?.Shops?.FirstOrDefault(s => s.StocName == stockName) is ShopInfo shop)
                    {
                        var src = CargoTransactionEndPoint.CreateForCharacterStoc(character, srcStockName);
                        var dst = CargoTransactionEndPoint.CreateForShop(shop);
                        var totalPrice = 0;

                        foreach (var item in items)
                        {
                            var result = src.SendItemTo(dst, item.Id, item.Count, item.UniqueData);
                            totalPrice += Math.Max(0, item.IGCPrice * result);
                        }

                        character.AddCharacterCurrencies(igc: totalPrice);
                        character.RaseStockUpdated();
                        SendFleetCargo();
                        SendObjectStock(obj, shop.Items, srcStockName);
                    }

                    SynckSessionFleetInfo();
                }
            });
        }

        private void HandleRequestQuestDialog(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            if (Server?.Realm is SfaRealm realm &&
                realm.Database is SfaDatabase realmDatabase &&
                realm.QuestsDatabase is DiscoveryQuestsDatabase questsDatabase &&
                questsDatabase.GetQuest(entityId) is DiscoveryQuest quests &&
                realmDatabase.GetQuestLogic(quests.LogicId) is QuestLogicInfo logic)
            {
                SendQuestDialog(entityId, logic);
            }
        }


        private void HandleAcceptQuest(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int entityId = reader.ReadInt32();

            if (CurrentCharacter is ServerCharacter character)
            {
                character.AcceptQuest(entityId);
                SendQuestDataUpdate();
                character.UpdateQuestLines();
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

                    SynckSessionFleetInfo();
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
                    if (system.GetObjectsAt<SecretObject>(hex, false).FirstOrDefault() is SecretObject secret &&
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

        private void HandleSecretObjectLooted(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Invoke(() =>
            {

            });
        }
    }
}
