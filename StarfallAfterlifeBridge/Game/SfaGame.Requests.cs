using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
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
            var doc = new JsonObject();

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

                if (eqForDisassemble?["eqlist"] is JsonArray eqlist)
                {
                    var rnd = new Random();

                    foreach (var eq in eqlist)
                    {
                        if ((int?)eq["entity"] is int entity &&
                            (int?)eq["count"] is int count &&
                            count > 0 &&
                            p.GameProfile.CurrentCharacter is Character character &&
                            p.Database is SfaDatabase database &&
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

                                AddProductionPoints(newProductionPoints);
                                Profile.SaveGameProfile();
                                SfaClient.SyncCharacterNewResearch(character, newOpenResearch.ToList());
                            }
                        }
                    }
                }

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
            });
            
            return doc;
        }

        protected JsonNode HandleSellInventory(SfaHttpQuery query)
        {
            UpdateProductionPointsIncome();

            var doc = new JsonObject();

            if (query is null)
                return doc;

            Profile.Use(p =>
            {
                if ((string)query["inventoryforsell"] is string sellQuery &&
                    JsonHelpers.ParseNodeUnbuffered(sellQuery)?["eqlist"] is JsonArray eqList &&
                    p.GameProfile.CurrentCharacter is Character character &&
                    p.Database is SfaDatabase database)
                {
                    foreach (var item in eqList)
                    {
                        if (item is not null &&
                            (int?)item["entity"] is int entity &&
                            (int?)item["count"] is int count &&
                            database.GetItem(entity) is SfaItem info &&
                            character.GetInventoryItem(info) is InventoryItem inventory)
                        {
                            int seilCount = Math.Min(character.GetInventoryItem(info).Count, count);
                            character.DeleteInventoryItem(info, seilCount);
                            character.IGC += Math.Max(0, (int)(info.IGC * 0.9f) * seilCount);
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

            var doc = new JsonObject();

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

                    DistributeProductionPoints();

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

            var doc = new JsonObject();

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

                    doc["inventory"] = CreateInventoryResponce(newItems.Where(i => i is not null).ToArray());
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

            var doc = new JsonObject();

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

            var doc = new JsonObject();

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

        public JsonNode HandleConfirmSessionReward(SfaHttpQuery query)
        {
            var doc = new JsonObject();

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
    }
}
