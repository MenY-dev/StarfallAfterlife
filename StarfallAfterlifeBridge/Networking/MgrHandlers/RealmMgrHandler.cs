using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.MgrHandlers
{
    public class RealmMgrHandler : MgrHandler
    {
        protected Character Character => Profile?.CurrentCharacter;

        public RealmMgrHandler(SfaGameProfile profile) : base(profile)
        {
        }

        #region Crafting

        public JsonNode ReceiveStartCrafting(SfaHttpQuery query)
        {
            int entity = (int?)query["project_entity"] ?? -1;

            if (Character is null || entity < 0)
                return Character.CreateCraftingResponse(new int[0], new int[0]);

            IEnumerable<int> newCrafts = new int[0];
            CraftingInfo craftingInfo = Character.AddCraftingItem(entity);

            if ((craftingInfo is null) == false)
            {
                newCrafts = new[] { craftingInfo.CraftingId };
            }

            return Character.CreateCraftingResponse(new int[0], newCrafts);
        }

        public JsonNode ReceiveAcquireCraftedItem(SfaHttpQuery query)
        {
            int craftingId = (int?)query["craftingid"] ?? -1;

            if (Character is null || craftingId < 0)
                return new JsonObject();

            CraftingInfo craftingInfo = Character.GetCraftingItem(craftingId);
            IEnumerable<int> newShips = new int[0];

            if ((craftingInfo is null) == false)
            {
                Character.DeleteCraftingItem(craftingInfo);
                FleetShipInfo newShip = Character.AddShip(craftingInfo.ProjectEntity);

                if ((newShip is null) == false)
                    newShips = new[] { newShip.Id };
            }

            return Character.CreateCraftingResponse(newShips, new int[0]);
        }

        public JsonNode ReceiveAcquireAllCraftedItems(SfaHttpQuery query)
        {
            if (Character is null)
                return new JsonObject();

            List<int> newShips = new List<int>();

            foreach (var item in Character.Crafting)
            {
                if (item is null)
                    continue;

                FleetShipInfo newShip = Character.AddShip(item.ProjectEntity);

                if ((newShip is null) == false)
                    newShips.Add(newShip.Id);
            }

            Character.Crafting.Clear();

            return Character.CreateCraftingResponse(newShips, new int[0]);
        }

        #endregion

        #region Battlegrounds

        public JsonNode ReceiveBuyBattleGroundShopItem(SfaHttpQuery query)
        {
            int id = (int?)query["item_id"] ?? -1;

            if (Character is Character character &&
                Profile?.Database?.GetItem(id) is SfaItem item)
                character.AddInventoryItem(item, 1);

            return null;
        }

        #endregion

        #region Detachment

        public JsonNode ReceiveMenuCurrentDetachment(SfaHttpQuery query)
        {
            int id = (int?)query["detachmentid"] ?? -1;

            if (Character is null || id < 0)
                return new JsonObject { ["ok"] = 0 };

            Character.CurrentDetachment = id;

            return new JsonObject { ["ok"] = 1 };
        }

        public JsonNode ReceiveDetachmentSave(SfaHttpQuery query)
        {
            int id = (int?)query["detachmentid"] ?? -1;
            Detachment detachment = Character?.Detachments?[id];

            if (detachment is null)
                return new JsonObject { ["ok"] = 0 };

            detachment.Slots.Clear();

            foreach (var item in query.StartsWith("slot", true, StringComparison.InvariantCultureIgnoreCase))
                if (int.TryParse(item.Key, out int slotNumber))
                    detachment.Slots[slotNumber] = ((int?)item ?? -1) - Character.IndexSpace;

            return new JsonObject { ["ok"] = 1 };
        }

        public JsonNode ReceiveDetachmentAbilitySave(SfaHttpQuery query)
        {
            int id = (int?)query["detachmentid"] ?? -1;
            DetachmentAbilities abilities = Character?.Detachments?[id]?.Abilities;

            if (abilities is null)
                return new JsonObject { ["ok"] = 0 };

            abilities.Clear();

            foreach (var item in query.StartsWith("cell", true, StringComparison.InvariantCultureIgnoreCase))
                if (int.TryParse(item.Key, out int cell))
                    abilities[cell] = (int?)item ?? -1;

            return new JsonObject { ["ok"] = 1 };
        }

        #endregion

        #region Ship Editor

        public JsonNode ReceiveSaveShip(SfaHttpQuery query)
        {
            var result = new JsonObject { ["ok"] = 0 };
            JsonNode request = JsonNode.Parse((string)query["data"] ?? string.Empty);

            if (request is null || Character is null)
                return result;

            int shipId = (int?)request["elid"] ?? -1;

            if (shipId < Character.IndexSpace)
                return result;

            var ship = Character.GetShip(shipId - Character.IndexSpace);

            if (ship is null)
                return result;

            var shipData = ship.Data ??= new();

            shipData.ShipSkin = (int?)request["ship_skin"] ?? 0;
            shipData.SkinColor1 = (int?)request["skin_color_1"] ?? 0;
            shipData.SkinColor2 = (int?)request["skin_color_2"] ?? 0;
            shipData.SkinColor3 = (int?)request["skin_color_3"] ?? 0;
            shipData.ShipDecal = (int?)request["shipdecal"] ?? 0;

            JsonArray hardpoints = request["hplist"]?.AsArray();

            if (hardpoints != null)
            {
                shipData.HardpointList.Clear();

                foreach (var item in hardpoints)
                {
                    JsonArray eqlist = item["eqlist"]?.AsArray();

                    if (eqlist is null)
                        continue;

                    List<ShipHardpointEquipment> equipments = new List<ShipHardpointEquipment>();

                    foreach (var eqItem in eqlist)
                    {
                        equipments.Add(new ShipHardpointEquipment
                        {
                            Equipment = (int?)eqItem["eq"] ?? -1,
                            X = (int?)eqItem["x"] ?? 0,
                            Y = (int?)eqItem["y"] ?? 0
                        });
                    }

                    shipData.HardpointList.Add(new ShipHardpoint
                    {
                        Hardpoint = (string)item["hp"] ?? string.Empty,
                        EquipmentList = equipments
                    });
                }
            }

            JsonArray progression = request["progression"]?.AsArray();

            if (progression != null)
            {
                shipData.Progression.Clear();

                foreach (var item in progression)
                {
                    shipData.Progression.Add(new ShipProgression
                    {
                        Id = (int?)item["id"] ?? -1,
                        Points = (int?)item["points"] ?? 0,
                    });
                }
            }

            result = new JsonObject { ["ok"] = 1 };
            
            return result;
        }

        public JsonNode ReceiveShipDelete(SfaHttpQuery query)
        {
            var result = new JsonObject { ["ok"] = 0 };
            int id = (int?)query["elid"] ?? -1;

            if (Character is null || id < Character.IndexSpace)
                return result;

            Character.DeleteShip(id - Character.IndexSpace);
            result = new JsonObject { ["ok"] = 1 };

            return result;
        }

        public JsonNode ReceiveFavoriteShip(SfaHttpQuery query)
        {
            var result = new JsonObject { ["ok"] = 0 };
            int id = (int?)query["ship_id"] ?? -1;

            if (Character is null || id < Character.IndexSpace)
                return result;

            var ship = Character?.GetShip(id - Character.IndexSpace);

            if (ship is null)
                return result;

            ship.IsFavorite = (int?)query["is_favorite"] == 1 ? 1 : 0;
            result = new JsonObject { ["ok"] = 1 };

            return result;
        }

        #endregion
    }
}
