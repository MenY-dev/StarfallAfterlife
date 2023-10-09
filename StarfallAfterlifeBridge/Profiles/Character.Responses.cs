using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class Character
    {
        public JsonNode CreateCharacterDataResponse(UserDataFlag flags = UserDataFlag.All)
        {
            return new JsonObject
            {
                ["data_result"] = new JsonObject
                {
                    ["$"] = CreateCharacterResponse(flags).ToJsonString()
                }
            };
        }

        public JsonNode CreateCharacterResponse(UserDataFlag flags = UserDataFlag.All)
        {
            flags |= UserDataFlag.CharacterInfo | UserDataFlag.Boosters | UserDataFlag.DiscoveryBattleInfo;

            JsonNode doc = new JsonObject();

            if (flags.HasFlag(UserDataFlag.CharacterInfo))
            {
                doc["charactname"] = CurrentName;
                doc["faction"] = Faction;
                doc["level"] = Level;
                doc["igc"] = IGC;
                doc["xp"] = Xp;
                doc["currentdetachment"] = CurrentDetachment;
                doc["rank"] = Rank;
                doc["reputation"] = Reputation;
                doc["access_level"] = AccessLevel;
                doc["ability_cells"] = AbilityCells;
                doc["has_active_session"] = HasActiveSession == true ? 1 : 0;
                doc["char_for_tutorial"] = CharForTutorial;
                doc["ship_slots"] = ShipSlots;
                doc["selfservice"] = SelfService;
                doc["production_points"] = ProductionPoints;
                doc["production_income"] = ProductionIncome;
                doc["production_cap"] = ProductionCap;
                doc["bonus_xp"] = BonusXp;
                doc["bonus_xp_income_minute_elapsed"] = BonusXpIncomeMinuteElapsed;
                doc["has_session_results"] = HasSessionResults;
                doc["end_session_time"] = EndSessionTime;
                doc["bgc"] = BGC;
            }

            if (flags.HasFlag(UserDataFlag.Boosters))
            {
                doc["c_has_premium"] = HasPremium;
                doc["c_xp_boost"] = XpBoost;
                doc["c_igc_boost"] = IgcBoost;
                doc["c_craft_boost"] = CraftBoost;
                doc["c_premium_minutes_left"] = PremiumMinutesLeft;
                doc["c_xp_minutes_left"] = XpMinutesLeft;
                doc["c_igc_minutes_left"] = IgcMinutesLeft;
                doc["c_craft_minutes_left"] = CraftMinutesLeft;
            }

            if (flags.HasFlag(UserDataFlag.WeeklyQuests))
            {

            }

            //if (flags.HasFlag(UserDataFlag.WeeklyQuestsProgress))
            //{

            //}

            if (flags.HasFlag(UserDataFlag.DiscoveryBattleInfo))
            {
                doc["indiscoverybattle"] = InDiscoveryBattle;
            }

            if (flags.HasFlag(UserDataFlag.Ships))
            {
                JsonArray ships = new JsonArray();

                foreach (var item in Ships)
                    ships.Add(CreateShipResponse(item));

                doc["ships"] = ships;
            }

            if (flags.HasFlag(UserDataFlag.Inventory))
            {
                JsonArray inventory = new JsonArray();

                if (Inventory.Count == 0)
                {
                    foreach (var item in Database.Equipments.Values)
                    {
                        AddInventoryItem(item, 999);
                    }
                }

                foreach (var item in Inventory)
                    inventory.Add(CreateInventoryItemResponse(item));

                doc["inventory"] = inventory;
            }

            if (flags.HasFlag(UserDataFlag.Crafting))
            {
                JsonArray crafting = new JsonArray();

                foreach (var item in Crafting)
                {
                    crafting.Add(new JsonObject
                    {
                        ["id"] = item.CraftingId,
                        ["project_entity"] = item.ProjectEntity,
                        ["queue_position"] = item.QueuePosition,
                        ["production_points_spent"] = item.ProductionPointsSpent,
                    });
                }

                doc["crafting"] = crafting;
            }

            if (flags.HasFlag(UserDataFlag.CheckedEvents))
            {
                doc["checked_events"] = CreateCheckedEventsResponse();
            }

            if (flags.HasFlag(UserDataFlag.Achievements))
            {

            }

            if (flags.HasFlag(UserDataFlag.Detachments))
            {
                JsonArray detachments = new JsonArray();

                foreach (var detachment in Detachments)
                {
                    detachments.Add(new JsonObject()
                    {
                        ["id"] = detachment.Key,
                        ["xp"] = detachment.Value.Xp,
                        ["slots"] = CreateDetachmentSlotsResponse(detachment.Value),
                        ["abilities"] = CreateDetachmentAbilitiesResponse(detachment.Value)
                    });
                }

                doc["detachments"] = detachments;
            }

            if (flags.HasFlag(UserDataFlag.ProjectsResearch))
            {
                JsonArray projects = new JsonArray();

                foreach (var proj in ProjectResearch)
                {
                    projects.Add(new JsonObject()
                    {
                        ["entity"] = proj.Entity,
                        ["xp"] = proj.Xp,
                        ["is_opened"] = proj.IsOpened,
                    });
                }

                doc["project_research"] = projects;
            }

            if (flags.HasFlag(UserDataFlag.FactionShop))
            {

            }

            if (flags.HasFlag(UserDataFlag.ActiveShips))
            {
                doc["active_ships"] = ActiveShips?.AsArray() ?? new JsonArray();
            }

            if (flags.HasFlag(UserDataFlag.SpecOps))
            {

            }

            if (flags.HasFlag(UserDataFlag.SpecOpsRewards))
            {

            }

            if (flags.HasFlag(UserDataFlag.CharactRewards))
            {

            }

            if (flags.HasFlag(UserDataFlag.CharactRewardQueue))
            {

            }

            if (flags.HasFlag(UserDataFlag.CompletedQuests))
            {

            }

            if (flags.HasFlag(UserDataFlag.BGShopItems))
            {

            }

            return doc;
        }

        public JsonNode CreateDiscoveryCharacterDataResponse()
        {
            JsonNode doc = new JsonObject
            {
                ["charactname"] = CurrentName,
                ["faction"] = Faction,
                ["xp_factor"] = XpBoost,
                ["bonus_xp"] = BonusXp,
                ["access_level"] = AccessLevel,
                ["level"] = Level,
                ["house_tag"] = "",
            };

            JsonArray ships = new JsonArray();
            JsonArray groups = new JsonArray();

            foreach (var item in Ships)
                ships.Add(CreateShipResponse(item));

            doc["ships_list"] = ships;
            doc["ship_groups"] = groups;

            return doc;
        }

        public JsonNode CreateCraftingResponse(IEnumerable<int> newShips = null, IEnumerable<int> newCrafts = null)
        {
            JsonNode doc = new JsonObject();
            JsonArray ships = new JsonArray();
            JsonArray inventory = new JsonArray();
            JsonArray crafting = new JsonArray();

            if (newShips is null)
            {
                foreach (var item in Ships)
                    ships.Add(CreateShipResponse(item));
            }
            else
            {
                List<int> shipFilter = new List<int>(newShips);

                foreach (var item in Ships)
                    if (shipFilter.Contains(item.Id))
                        ships.Add(CreateShipResponse(item));
            }

            foreach (var item in Inventory)
                inventory.Add(CreateInventoryItemResponse(item));

            if (newCrafts is null)
            {
                foreach (var item in Crafting)
                {
                    crafting.Add(new JsonObject
                    {
                        ["id"] = item.CraftingId,
                        ["project_entity"] = item.ProjectEntity,
                        ["queue_position"] = item.QueuePosition,
                        ["production_points_spent"] = 99999,
                    });
                }
            }
            else
            {
                List<int> craftingFilter = new List<int>(newCrafts);

                foreach (var item in Crafting)
                {
                    if (craftingFilter.Contains(item.CraftingId))
                        crafting.Add(new JsonObject
                        {
                            ["id"] = item.CraftingId,
                            ["project_entity"] = item.ProjectEntity,
                            ["queue_position"] = item.QueuePosition,
                            ["production_points_spent"] = 99999,
                        });
                }
            }

            doc["ships"] = ships;
            doc["inventory"] = inventory;
            doc["crafting"] = crafting;

            return new JsonObject
            {
                ["data_result"] = new JsonObject { ["$"] = doc.ToJsonString() }
            };
        }

        public JsonNode CreateShipResponse(FleetShipInfo ship)
        {
            if (ship is null)
                return new JsonObject();

            JsonNode doc = new JsonObject
            {
                ["id"] = ship.Id + IndexSpace,
                ["data"] = CreateShipDataResponse(ship).ToJsonString(),
                ["position"] = ship.Position,
                ["kills"] = ship.Kills,
                ["death"] = ship.Death,
                ["played"] = ship.Played,
                ["woncount"] = ship.WonCount,
                ["lostcount"] = ship.LostCount,
                ["xp"] = ship.Xp,
                ["level"] = 30,
                ["damagedone"] = ship.DamageDone,
                ["damagetaken"] = ship.DamageTaken,
                ["timetoconstruct"] = ship.TimeToConstruct,
                ["timetorepair"] = ship.TimeToRepair,
                ["is_favorite"] = ship.IsFavorite
            };

            return doc;
        }

        public JsonNode CreateShipDataResponse(FleetShipInfo ship)
        {
            if (ship is null)
                return new JsonObject();

            var shipData = ship?.Data ?? new();

            JsonNode doc = new JsonObject
            {
                ["hull"] = shipData.Hull,
                ["elid"] = shipData.Id + IndexSpace,
                ["xp"] = shipData.Xp,
                ["armor"] = shipData.ArmorDelta,
                ["structure"] = shipData.StructureDelta,
                ["detachment"] = shipData.Detachment,
                ["slot"] = shipData.Slot,
                ["destroyed"] = 0,
                ["ship_skin"] = shipData.ShipSkin,
                ["skin_color_1"] = shipData.SkinColor1,
                ["skin_color_2"] = shipData.SkinColor2,
                ["skin_color_3"] = shipData.SkinColor3,
                ["shipdecal"] = shipData.ShipDecal,
                ["cargo_hold_size"] = Database?.GetShipCargo(shipData.Hull) ?? 0,
            };

            JsonArray hardpoints = new JsonArray();
            JsonArray progression = new JsonArray();

            foreach (var item in shipData.HardpointList)
            {
                JsonArray equipments = new JsonArray();

                foreach (var eq in item.EquipmentList)
                {
                    equipments.Add(new JsonObject
                    {
                        ["eq"] = eq.Equipment,
                        ["x"] = eq.X,
                        ["y"] = eq.Y,
                    });
                }

                hardpoints.Add(new JsonObject
                {
                    ["hp"] = item.Hardpoint,
                    ["eqlist"] = equipments
                });
            }

            foreach (var item in shipData.Progression)
            {
                progression.Add(new JsonObject
                {
                    ["id"] = item.Id,
                    ["points"] = item.Points
                });
            }

            doc["hplist"] = hardpoints;
            doc["progression"] = progression;

            return doc;
        }

        public JsonNode CreateInventoryItemResponse(InventoryItem item)
        {
            if (item is null)
                return new JsonObject();


            JsonNode doc = new JsonObject
            {
                ["itemtype"] = (byte)item.Type,
                ["id"] = item.Id,
                ["count"] = item.Count,
                ["unique_data"] = item.UniqueData ?? ""
            };

            return doc;
        }

        //public JsonArray CreateDetachmentSlotsResponse(Detachment detachment)
        //{
        //    if (detachment is null)
        //        return new JsonArray();

        //    JsonArray slots = new();

        //    foreach (var slot in detachment.Slots)
        //    {
        //        slots.Add(new JsonObject()
        //        {
        //            [slot.Key.ToString()] = (slot.Value < 1 ? 0 : slot.Value + IndexSpace)
        //        });
        //    }

        //    return slots;
        //}

        public JsonObject CreateDetachmentSlotsResponse(Detachment detachment)
        {
            if (detachment is null)
                return new JsonObject();

            JsonObject slots = new();

            foreach (var slot in detachment.Slots)
            {
                slots[slot.Key.ToString()] = (slot.Value < 1 ? int.MaxValue : slot.Value + IndexSpace);
            }

            return slots;
        }

        public JsonArray CreateDetachmentAbilitiesResponse(Detachment detachment)
        {
            if (detachment?.Abilities is null)
                return new JsonArray();

            JsonArray abilities = new();

            foreach (var ability in detachment.Abilities)
            {
                abilities.Add(new JsonObject()
                {
                    [ability.Key.ToString()] = ability.Value < 0 ? 0 : ability.Value
                });
            }

            return abilities;
        }

        public JsonArray CreateActiveShipsResponse()
        {
            Detachment currentDetachment = Detachments[CurrentDetachment];
            JsonArray ships = new();

            foreach (var slot in currentDetachment.Slots)
            {
                if (slot.Value > -1)
                {
                    ships.Add(new JsonObject
                    {
                        ["id"] = slot.Value,
                        ["in_galaxy"] = 1,
                        ["destroyed"] = 0,
                        ["data"] = CreateShipDataResponse(GetShip(slot.Value)).ToJsonString(),
                    });
                }
            }

            return ships;
        }

        public JsonNode CreateCheckedEventsResponse()
        {
            return new JsonArray()
            {
                new JsonObject { ["event_id"] = "DiscoveryIntroTutorial" },
                new JsonObject { ["event_id"] = "DiscoveryMothershipTutorial" },
                new JsonObject { ["event_id"] = "DiscoveryCaravanTutorial" },
                new JsonObject { ["event_id"] = "DiscoveryPostCaravanTutorial" },
                new JsonObject { ["event_id"] = "DiscoveryQuestsTutorial" },
                new JsonObject { ["event_id"] = "DiscoveryAutopilotTutorial" },
                new JsonObject { ["event_id"] = "ShipyardBanDropSessionTutorial" },
                new JsonObject { ["event_id"] = "ShipyardIntroTutorial" },
                new JsonObject { ["event_id"] = "ShipyardShipEditTutorial" },
                new JsonObject { ["event_id"] = "ShipyardCraftingTutorial" },
                new JsonObject { ["event_id"] = "ShipyardShipBuildTutorial" },
                new JsonObject { ["event_id"] = "FirstChoiceTutorial" },
                new JsonObject { ["event_id"] = "DiscoveryRecall" },
                new JsonObject { ["event_id"] = "MiningContextHint" },
            };
        }
    }
}
