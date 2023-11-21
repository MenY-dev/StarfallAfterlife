using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame
    {
        protected JsonNode HandleCharecterEditRequest(SfaHttpQuery query)
        {
            var doc = new JsonObject();
            var character = Profile.CreateNewCharacter(
                (string)query["name"],
                (Faction?)(byte?)query["faction"] ?? Faction.None);

            if (character is not null)
            {

                var result = SfaClient.SendRequest(
                    SfaServerAction.RegisterNewCharacters,
                    new JsonObject
                    {
                        ["chars"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["id"] = character.Id,
                                ["name"] = character.Name,
                                ["faction"] = character.Faction,
                            }
                        }
                    }.ToJsonStringUnbuffered(false)
                ).ContinueWith(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        response.Action == SfaServerAction.RegisterNewCharacters)
                    {
                        return ProcessRegisterNewChars(JsonHelpers.ParseNodeUnbuffered(response.Text)?["chars"]?.AsArraySelf());
                    }

                    return false;
                }).Result;

                if (result == true)
                {
                    doc["charactid"] = SValue.Create(character.UniqueId);
                    Profile.SaveGameProfile();
                }
            }


            return doc;
        }

        protected JsonNode HandleCharecterDeleteRequest(SfaHttpQuery query)
        {
            var doc = new JsonObject();

            if ((int?)query["charactid"] is int id &&
                GameProfile?.DiscoveryModeProfile?.GetCharByUniqueId(id) is Character character)
            {
                Profile.RemoveCharacter(character);

                SfaClient.Send(new JsonObject
                {
                    ["char_id"] = id
                }, SfaServerAction.DeleteCharacter);

                // 0 - delete done
                // 1 - delete fail, char heads the house
                doc["delete_result"] = SValue.Create(0);
                Profile.SaveGameProfile();
            }

            return doc;
        }

        protected bool ProcessRegisterNewChars(JsonArray chars)
        {
            if (chars is null || chars.Count == 0)
                return false;

            var profile = Profile.GameProfile.DiscoveryModeProfile;

            if (chars is not null)
            {
                foreach (var charData in chars)
                {
                    if (profile.GetCharById((int?)charData["id"] ?? -1) is Character c)
                    {
                        c.Database = Profile.Database;
                        c.UniqueId = (int?)charData["unique_id"] ?? -1;
                        c.UniqueName = (string)charData["unique_name"];
                    }
                }
            }

            return true;
        }

        protected JsonNode HandleDisassembleItems(SfaHttpQuery query)
        {
            var doc = new JsonObject(){ ["ok"] = 1 };

            if (query is null)
                return doc;

            Profile?.Use(p =>
            {
                bool forProductionPoints = (bool?)query["disassembleforproductionpoints"] ?? false;
                var eqForDisassemble = JsonHelpers.ParseNodeUnbuffered((string)query["eqfordisassemble"]);
                List<(int id, int laxtXp, int newXp, int needXp, int isOpened)> newResearch = new();
                List<int> newOpenResearch = new();
                Dictionary<int, int> newItems = new();
                
                int newProductionPoints = 0;

                if (eqForDisassemble?["eqlist"] is JsonArray eqlist &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    p.Database is SfaDatabase database)
                {
                    var rnd = new Random();

                    foreach (var eq in eqlist)
                    {
                        if ((int?)eq["entity"] is int entity &&
                            (int?)eq["count"] is int count &&
                            count > 0 &&
                            database.GetItem(entity) is SfaItem disassembleItem)
                        {
                            character.DeleteInventoryItem(disassembleItem, count);

                            if (disassembleItem is not null)
                            {
                                if (forProductionPoints == true)
                                    newProductionPoints += disassembleItem.ProductionPointsOnDisassemble * count;
                                else
                                {
                                    if (disassembleItem.ProjectToOpen != 0 &&
                                        disassembleItem.ProjectToOpenXp > 0 &&
                                        p.Database.GetItem(disassembleItem.ProjectToOpen) is SfaItem toOpen &&
                                        toOpen.RequiredProjectToOpenXp > -1)
                                    {
                                        var research = character.ProjectResearch.FirstOrDefault(r => 
                                            r.Entity == toOpen.Id);

                                        if (research is null)
                                        {
                                            research = new ResearchInfo
                                            {
                                                Entity = toOpen.Id,
                                                Xp = 0,
                                                IsOpened = 0
                                            };

                                            character.ProjectResearch.Add(research);
                                        }

                                        if (research.IsOpened < 1)
                                        {
                                            var currentXp = research.Xp;
                                            var newXp = currentXp + disassembleItem.ProjectToOpenXp * count;

                                            if (newXp >= toOpen.RequiredProjectToOpenXp)
                                            {
                                                research.Xp = toOpen.RequiredProjectToOpenXp;
                                                research.IsOpened = 1;
                                                newOpenResearch.Add(toOpen.Id);
                                            }
                                            else
                                            {
                                                research.Xp = newXp;
                                            }

                                            newResearch.Add(
                                            (
                                                toOpen.Id,
                                                currentXp,
                                                research.Xp,
                                                toOpen.RequiredProjectToOpenXp,
                                                research.IsOpened
                                            ));
                                        }
                                    }

                                    if (disassembleItem.DisassembleMaterialsDrop is List<MaterialDropInfo> drop &&
                                        drop.Count > 0)
                                    {
                                        foreach (var dropItem in drop)
                                        {
                                            var dropCount = rnd.Next(dropItem.Min, dropItem.Max + 1);
                                            var newItem = database.GetItem(dropItem.Id);
                                            character.AddInventoryItem(newItem, dropCount);

                                            if (newItems.ContainsKey(dropItem.Id))
                                                newItems[dropItem.Id] += dropCount;
                                            else
                                                newItems.Add(dropItem.Id, dropCount);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    AddProductionPoints(newProductionPoints, false);
                    Profile.SaveGameProfile();
                    SfaClient.SyncCharacterNewResearch(character, newOpenResearch.ToList());
                }

                if (newItems.Count > 0 || newResearch.Count > 0)
                {
                    JsonArray receivedItems = new JsonArray();
                    JsonArray projectResearch = new JsonArray();

                    foreach (var item in newItems)
                    {
                        receivedItems.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(item.Key),
                            ["count"] = SValue.Create(item.Value),
                        });
                    }

                    foreach (var item in newResearch)
                    {
                        projectResearch.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(item.id),
                            ["previous_xp"] = SValue.Create(item.laxtXp),
                            ["current_xp"] = SValue.Create(item.newXp),
                            ["required_project_to_open_xp"] = SValue.Create(item.needXp),
                            ["is_opened"] = SValue.Create(item.isOpened),
                        });
                    }

                    doc["received_items"] = receivedItems;
                    doc["project_research"] = projectResearch;
                }
            });
            
            return doc;
        }

        protected JsonNode HandleSellInventory(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject(){ ["ok"] = 1 };

            if (query is null)
                return doc;

            Profile.Use(p =>
            {
                if ((string)query["inventoryforsell"] is string sellQuery &&
                    JsonHelpers.ParseNodeUnbuffered(sellQuery)?["eqlist"] is JsonArray eqList &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    p.Database is SfaDatabase database)
                {
                    foreach (var eq in eqList)
                    {
                        if (eq is not null &&
                            (int?)eq["entity"] is int entity &&
                            (int?)eq["count"] is int count &&
                            database.GetItem(entity) is SfaItem info &&
                            character.GetInventoryItemVariants(info)?
                            .OrderBy(i => string.IsNullOrWhiteSpace(i.UniqueData) ? -1 : 0)
                            .ToArray() is InventoryItem[] inventory)
                        {
                            int requiredCount = Math.Max(0, count);
                            int totalCount = 0;

                            foreach (var item in inventory)
                            {
                                var toRemove = Math.Min(requiredCount, item.Count);
                                character.DeleteInventoryItem(info, toRemove, item.UniqueData);
                                requiredCount -= toRemove;
                                totalCount += toRemove;

                                if (requiredCount < 1)
                                    break;
                            }


                            character.IGC += Math.Max(0, (int)(info.IGC * 0.9f) * totalCount);
                        }
                    }

                    Profile.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return new JsonObject { };
        }

        protected JsonNode HandleStartCrafting(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject(){ ["ok"] = 1 };

            if (query is null)
                return doc;

            Profile.Use(p =>
            {
                if ((int?)query["project_entity"] is int entity &&
                    (int?)query["project_count"] is int count &&
                    count > 0 &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    p.Database is SfaDatabase database &&
                    database.GetItem(entity) is SfaItem item &&
                    IsProductionPossible(item, count) == true)
                {
                    if (item.Materials is List<MaterialInfo> materials)
                    {
                        foreach (var material in materials)
                            if (database.GetItem(material.Id) is SfaItem materialItem)
                                character.DeleteInventoryItem(materialItem, material.Count * count);
                    }

                    if (item.ItemType is (InventoryItemType.ShipProject or InventoryItemType.ItemProject))
                        character.DeleteInventoryItem(item, count);

                    character.IGC = Math.Max(0, character.IGC - item.IGCToProduce);

                    var newCraftings = Enumerable
                        .Repeat(entity, count)
                        .Select(e => character.AddCraftingItem(entity))
                        .ToArray();

                    UpdateProductionPointsIncome(false);

                    doc["crafting"] = new JsonArray(newCraftings
                        .Select(e => JsonHelpers.ParseNodeUnbuffered(e))
                        .ToArray());

                    Profile.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return new JsonObject
            {
                ["data_result"] = SValue.Create(doc.ToJsonStringUnbuffered(false))
            };
        }

        protected JsonNode HandleAcquireCraftedItem(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject(){ ["ok"] = 1 };

            if (query is null)
                return doc;

            var newItems = new List<InventoryItem>();
            var newShips = new List<FleetShipInfo>();

            Profile.Use(p =>
            {
                if ((int?)query["craftingid"] is int craftingId &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    character.GetCraftingItem(craftingId) is CraftingInfo craftingItem &&
                    p.Database is SfaDatabase database &&
                    database.GetItem(craftingItem.ProjectEntity) is SfaItem item)
                {
                    character.DeleteCraftingItem(craftingItem);

                    if (item.ItemType == InventoryItemType.ShipProject)
                        newShips.Add(character.AddShip(item.Id));
                    else if (item.ItemType == InventoryItemType.ItemProject)
                    {
                        if (database.GetItem(item.ProductItem) is SfaItem productItem)
                            newItems.Add(character.AddInventoryItem(productItem, item.ProductCount));
                    }
                    else
                        newItems.Add(character.AddInventoryItem(item, 1));

                    doc["inventory"] = CreateInventoryResponce(newItems.Where(i => i.IsEmpty == false).ToArray());
                    doc["ships"] = CreateShipsResponce(newShips.Where(i => i is not null).ToArray());
                    doc["crafting"] = CreateCraftingResponce(character.Crafting.ToArray());

                    Profile.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return new JsonObject
            {
                ["data_result"] = SValue.Create(doc.ToJsonStringUnbuffered(false))
            };
        }

        protected JsonNode HandleAcquireAllCraftedItems(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject(){ ["ok"] = 1 };

            if (query is null)
                return doc;

            var newItems = new List<InventoryItem>();
            var newShips = new List<FleetShipInfo>();

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character &&
                    p.Database is SfaDatabase database &&
                    character.Crafting is List<CraftingInfo> crafting &&
                    crafting.Count > 0)
                {
                    var completedCrafts = new Dictionary<CraftingInfo, SfaItem>();

                    foreach (var craft in crafting)
                    {
                        if (craft is not null &&
                            database.GetItem(craft.ProjectEntity) is SfaItem item &&
                            craft.ProductionPointsSpent >= item.ProductionPoints)
                        {
                            completedCrafts.Add(craft, item);
                        }
                    }

                    foreach (var craft in completedCrafts)
                    {
                        character.DeleteCraftingItem(craft.Key);

                        if (craft.Value.ItemType == InventoryItemType.ShipProject)
                            newShips.Add(character.AddShip(craft.Value.Id));
                        else if (craft.Value.ItemType == InventoryItemType.ItemProject)
                        {
                            if (database.GetItem(craft.Value.ProductItem) is SfaItem productItem)
                                newItems.Add(character.AddInventoryItem(productItem, craft.Value.ProductCount));
                        }
                        else
                            newItems.Add(character.AddInventoryItem(craft.Value, 1));
                    }

                    doc["inventory"] = CreateInventoryResponce(newItems.ToArray());
                    doc["ships"] = CreateShipsResponce(newShips.ToArray());
                    doc["crafting"] = CreateCraftingResponce(character.Crafting.ToArray());

                    Profile.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return new JsonObject
            {
                ["data_result"] = SValue.Create(doc.ToJsonStringUnbuffered(false))
            };
        }

        public JsonNode HandleSwapCraftingQueue(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject() { ["ok"] = 1 };

            if (query is null)
                return doc;

            var newItems = new List<InventoryItem>();
            var newShips = new List<FleetShipInfo>();

            Profile.Use(p =>
            {
                if ((int?)query["craftingid1"] is int craftingId1 &&
                    (int?)query["craftingid2"] is int craftingId2 &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    character.GetCraftingItem(craftingId1) is CraftingInfo craftingItem1 &&
                    character.GetCraftingItem(craftingId2) is CraftingInfo craftingItem2)
                {
                    var tmpPos = craftingItem1.QueuePosition;
                    craftingItem1.QueuePosition = craftingItem2.QueuePosition;
                    craftingItem2.QueuePosition = tmpPos;

                    Profile.SaveGameProfile();
                }
            });

            return new JsonObject { };
        }

        public JsonNode HandleRushCraftingItem(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject(){};

            if (query is null)
                return doc;

            var newItems = new List<InventoryItem>();
            var newShips = new List<FleetShipInfo>();

            Profile.Use(p =>
            {
                if ((int?)query["craftingid"] is int craftingId &&
                    (int?)query["currencytype"] == 0 &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    character.GetCraftingItem(craftingId) is CraftingInfo craftingItem &&
                    p.Database?.GetItem(craftingItem.ProjectEntity) is SfaItem itemInfo)
                {
                    UpdateProductionPointsIncome(false);
                    var pp = itemInfo.ProductionPoints - craftingItem.ProductionPointsSpent;
                    var cost = (int)(p.GameProfile.ProductionPointsCost60IGC / 60f * pp);
                    craftingItem.ProductionPointsSpent = itemInfo.ProductionPoints;
                    character.IGC = Math.Max(0, character.IGC - cost);
                    Profile.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                    doc["ok"] = 1;
                }
            });

            return doc;
        }

        public JsonNode HandleConfirmSessionReward(SfaHttpQuery query)
        {
            var doc = new JsonObject(){ ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    character.HasSessionResults = false;
                    character.LastSession = null;

                    Profile.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return doc;
        }

        public JsonNode HandleTakeCharactRewardFromQueue(SfaHttpQuery query)
        {
            var doc = new JsonObject(){ ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    SfaClient?.TakeCharactRewardFromQueue(character.UniqueId, (int?)query["reward_id"] ?? -1);
                    p.SaveGameProfile();
                }
            });

            return doc;
        }

        public JsonNode HandleSaveShip(SfaHttpQuery query)
        {
            var doc = new JsonObject{ ["ok"] = 1 };
            JsonNode request = JsonHelpers.ParseNodeUnbuffered((string)query["data"]);

            Profile.Use(p =>
            {
                if (JsonHelpers.ParseNodeUnbuffered((string)query["data"]) is JsonObject request &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    (int?)request["elid"] is int shipId &&
                    shipId >= character.IndexSpace &&
                    character.GetShip(shipId - character.IndexSpace) is FleetShipInfo shipInfo)
                {
                    var newShip = shipInfo.Data ??= new();
                    var oldShip = newShip.Clone();

                    newShip.ShipSkin = (int?)request["ship_skin"] ?? 0;
                    newShip.SkinColor1 = (int?)request["skin_color_1"] ?? 0;
                    newShip.SkinColor2 = (int?)request["skin_color_2"] ?? 0;
                    newShip.SkinColor3 = (int?)request["skin_color_3"] ?? 0;
                    newShip.ShipDecal = (int?)request["shipdecal"] ?? 0;

                    if (request["hplist"]?.AsArraySelf() is JsonArray newHardpoints)
                    {
                        var shipHardpoints = newShip.HardpointList ??= new();
                        shipHardpoints.Clear();

                        shipHardpoints.AddRange(newHardpoints.Select(hardpoint => new ShipHardpoint
                        {
                            Hardpoint = (string)hardpoint["hp"],
                            EquipmentList = hardpoint["eqlist"]?.AsArraySelf()?.Select(equipment => new ShipHardpointEquipment()
                            {
                                Equipment = (int?)equipment["eq"] ?? -1,
                                X = (int?)equipment["x"] ?? 0,
                                Y = (int?)equipment["y"] ?? 0,
                            }).ToList(),
                        }).Where(h => h.Hardpoint is not null && h.EquipmentList is not null));
                    }

                    if (request["progression"]?.AsArraySelf() is JsonArray newProgress)
                    {
                        var shipProgress = newShip.Progression ??= new();
                        shipProgress.Clear();

                        shipProgress.AddRange(newProgress.Select(item => new ShipProgression
                        {
                            Id = (int?)item["id"] ?? -1,
                            Points = (int?)item["points"] ?? 0,
                        }));
                    }

                    var oldItems = oldShip.GetAllHardpointsEquipment();
                    var newItems = newShip.GetAllHardpointsEquipment();

                    foreach (var item in oldItems)
                        character.AddInventoryItem(item.Key, item.Value);

                    foreach (var item in newItems)
                        character.DeleteInventoryItem(item.Key, item.Value);

                    var igcCost = 0;
                    var sfcCost = 0;

                    if ((int?)query["drop_progression"] == 1)
                    {
                        oldShip.Progression?.Clear();

                        switch ((CurrencyType?)(int?)query["drop_type"])
                        {
                            case CurrencyType.IGC: igcCost += p.GameProfile.DropShipProgressionIGC; break;
                            case CurrencyType.SFC: sfcCost += p.GameProfile.DropShipProgressionSFC; break;
                        }
                    }

                    if ((string)query["eqforbuy"] is string eqForBuy &&
                        JsonHelpers.ParseNodeUnbuffered(eqForBuy)?["eqlist"] is JsonArray eqList)
                    {
                        foreach (var eq in eqList)
                        {
                            if ((int?)eq["entity"] is int entity &&
                                (int?)eq["count"] is int count &&
                                SfaDatabase.Instance.GetItem(entity) is SfaItem item)
                            {
                                character.AddInventoryItem(item, count);
                                igcCost += item.IGC * count;
                            }
                        }
                    }

                    igcCost += ShipConstructionInfo.CalculateCost(oldShip, newShip);
                    character.IGC = Math.Max(0, character.IGC - igcCost);

                    p.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return doc;
        }

        public JsonNode HandleShipDelete(SfaHttpQuery query)
        {
            var doc = new JsonObject{ ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    int id = (int?)query["elid"] ?? -1;

                    if (id < character.IndexSpace)
                        return;

                    var ship = character.GetShip(id - character.IndexSpace);

                    if (ship is null)
                        return;

                    character.DeleteShip(ship);

                    if (ship.Data is not null)
                    {
                        foreach (var item in ship.Data.GetAllHardpointsEquipment())
                            character.AddInventoryItem(item.Key, item.Value);
                    }

                    if (p.Database?.GetShip(ship.Data?.Hull ?? 0) is ShipBlueprint blueprint)
                        character.IGC = Math.Max(0, character.IGC + (int)Math.Round(blueprint.IGCToProduce * 0.33333333));

                    p.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return doc;
        }

        public JsonNode HandleFavoriteShip(SfaHttpQuery query)
        {
            var doc = new JsonObject{ ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    int id = (int?)query["ship_id"] ?? -1;

                    if (id < character.IndexSpace)
                        return;

                    var ship = character.GetShip(id - character.IndexSpace);

                    if (ship is null)
                        return;

                    ship.IsFavorite = (int?)query["is_favorite"] == 1 ? 1 : 0;
                    p.SaveGameProfile();
                }
            });

            return doc;
        }

        public JsonNode HandleBuyBattleGroundShopItem(SfaHttpQuery query)
        {
            var doc = new JsonObject{ ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    int id = (int?)query["item_id"] ?? -1;

                    if (p.Database?.GetItem(id) is SfaItem item)
                    {
                        character.AddInventoryItem(item, 1);
                        character.BGC = Math.Max(0, character.BGC - item.BGC);
                        p.SaveGameProfile();
                        SfaClient?.SyncCharacterCurrencies(character);
                    }
                }
            });

            return doc;
        }

        public JsonNode HandleDetachmentAbilitySave(SfaHttpQuery query)
        {
            var doc = new JsonObject { ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    int id = (int?)query["detachmentid"] ?? -1;
                    DetachmentAbilities abilities = character.Detachments?[id]?.Abilities;

                    if (abilities is null)
                        return;

                    abilities.Clear();

                    foreach (var item in query.StartsWith("cell", true, StringComparison.InvariantCultureIgnoreCase))
                        if (int.TryParse(item.Key, out int cell))
                            abilities[cell] = (int?)item ?? -1;

                    p.SaveGameProfile();
                }
            });

            return doc;
        }

        public JsonNode HandleDetachmentSave(SfaHttpQuery query)
        {
            var doc = new JsonObject { ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    int id = (int?)query["detachmentid"] ?? -1;
                    Detachment detachment = character.Detachments?[id];

                    if (detachment is null)
                        return;

                    detachment.Slots.Clear();

                    foreach (var item in query.StartsWith("slot", true, StringComparison.InvariantCultureIgnoreCase))
                        if (int.TryParse(item.Key, out int slotNumber))
                            detachment.Slots[slotNumber] = ((int?)item ?? -1) - character.IndexSpace;

                    p.SaveGameProfile();
                }
            });

            return doc;
        }

        public JsonNode HandleMenuCurrentDetachment(SfaHttpQuery query)
        {
            var doc = new JsonObject { ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {

                    int id = (int?)query["detachmentid"] ?? -1;
                    Detachment detachment = character.Detachments?[id];

                    if (detachment is null)
                        return;

                    character.CurrentDetachment = id;
                    p.SaveGameProfile();
                }
            });

            return doc;
        }

        public JsonNode HandleRepairShips(SfaHttpQuery query)
        {
            var doc = new JsonObject { ["ok"] = 1 };

            Profile.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character &&
                    (int?)query["ship_id"] is int id)
                {
                    UpdateShipsRepairProgress(false);
                    var ships = new List<FleetShipInfo>();

                    if (id == -1)
                    {
                        if (character.Detachments?[character.CurrentDetachment] is Detachment detachment)
                        {
                            foreach (var item in detachment.Slots.Values ?? Array.Empty<int>())
                            {
                                if (item > 0 &&
                                    character.GetShip(item) is FleetShipInfo ship &&
                                    ship.TimeToRepair > 0)
                                {
                                    ships.Add(ship);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (character.GetShip(id - character.IndexSpace) is FleetShipInfo ship)
                            ships.Add(ship);
                    }

                    int igcPerMinute = 60;
                    int totalCost = 0;

                    foreach (var item in ships)
                    {
                        if (item is null)
                            continue;

                        totalCost += (int)Math.Ceiling(item.TimeToRepair / 60f) * igcPerMinute;
                        item.TimeToRepair = 0;
                    }

                    character.IGC = Math.Max(0, character.IGC - totalCost);
                    p.SaveGameProfile();
                    SfaClient?.SyncCharacterCurrencies(character);
                }
            });

            return doc;
        }
    }
}
