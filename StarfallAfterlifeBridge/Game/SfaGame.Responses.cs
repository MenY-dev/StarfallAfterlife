using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Server.Matchmakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame
    {
        public JsonNode CreateCharacterDataResponse(UserDataFlag flags = UserDataFlag.All)
        {
            return new JsonObject
            {
                ["data_result"] = new JsonObject
                {
                    ["$"] = CreateCharacterResponse(GameProfile.CurrentCharacter, flags)?.ToJsonString(),
                },
                ["session_start_inventory"] = new JsonObject
                {
                    ["inventory"] = CreateCharacterSessionStartInventoryResponse(),
                },
                ["shop_data"] = new JsonObject()
                {
                    ["listdata"] = new JsonObject()
                    {
                        ["items"] = new JsonArray()
                        {
                            new JsonObject() { ["id"] = SValue.Create(232046526) },
                            new JsonObject() { ["id"] = SValue.Create(251198060) },
                            new JsonObject() { ["id"] = SValue.Create(308815052) },
                            new JsonObject() { ["id"] = SValue.Create(1239432978) },
                            new JsonObject() { ["id"] = SValue.Create(232644897) },
                            new JsonObject() { ["id"] = SValue.Create(1688154619) },
                        }
                    }
                }
            };
        }

        public JsonNode CreateCharacterResponse(Character character, UserDataFlag flags = UserDataFlag.All)
        {
            var doc = new JsonObject();

            if (character is null)
                return doc;

            var progress = Profile?.CurrentRealm?.Progress?.FirstOrDefault(p => p?.CharacterId == character.Id);

            flags |= UserDataFlag.CharacterInfo | UserDataFlag.Boosters | UserDataFlag.DiscoveryBattleInfo;

            if (flags.HasFlag(UserDataFlag.CharacterInfo))
            {
                doc["charactname"] = character.CurrentName;
                doc["faction"] = character.Faction;
                doc["level"] = character.Level;
                doc["igc"] = character.IGC;
                doc["xp"] = character.Xp;
                doc["currentdetachment"] = character.CurrentDetachment;
                doc["rank"] = character.Rank;
                doc["reputation"] = character.Reputation;
                doc["access_level"] = character.AccessLevel;
                doc["ability_cells"] = character.AbilityCells;
                doc["has_active_session"] = Profile.CurrentSession is not null;
                doc["char_for_tutorial"] = character.CharForTutorial;
                doc["ship_slots"] = character.ShipSlots;
                doc["selfservice"] = character.SelfService;
                doc["production_points"] = character.ProductionPoints;
                doc["production_income"] = character.ProductionIncome;
                doc["production_cap"] = character.ProductionCap;
                doc["bonus_xp"] = character.BonusXp;
                doc["bonus_xp_income_minute_elapsed"] = character.BonusXpIncomeMinuteElapsed;
                doc["has_session_results"] = (character.HasSessionResults && character.LastSession is not null) ? 1 : 0;
                doc["end_session_time"] = character.EndSessionTime;
                doc["bgc"] = character.BGC;
            }

            if (flags.HasFlag(UserDataFlag.Boosters))
            {
                doc["c_has_premium"] = character.HasPremium;
                doc["c_xp_boost"] = character.XpBoost;
                doc["c_igc_boost"] = character.IgcBoost;
                doc["c_craft_boost"] = character.CraftBoost;
                doc["c_premium_minutes_left"] = character.PremiumMinutesLeft;
                doc["c_xp_minutes_left"] = character.XpMinutesLeft;
                doc["c_igc_minutes_left"] = character.IgcMinutesLeft;
                doc["c_craft_minutes_left"] = character.CraftMinutesLeft;
            }

            if (character.HasSessionResults == true &&
                character.LastSession is DiscoverySession lastSession)
            {
                doc["session_start_xp"] = lastSession.SessionStartXp;
                doc["session_start_igc"] = lastSession.SessionStartIGC;
                doc["session_start_reputation"] = lastSession.SessionStartBGC;
                doc["session_start_access_level"] = lastSession.SessionStartAccessLevel;
            }

            if (flags.HasFlag(UserDataFlag.WeeklyQuests))
            {

            }

            if (flags.HasFlag(UserDataFlag.DiscoveryBattleInfo))
            {
                doc["indiscoverybattle"] = character.InDiscoveryBattle;
            }

            if (flags.HasFlag(UserDataFlag.Ships))
            {
                JsonArray ships = new JsonArray();
                JsonArray groups = new JsonArray();


                foreach (var item in character.Ships ?? new())
                    ships.Add(CreateShipResponse(item));

                foreach (var item in character.ShipGroups ?? new())
                    groups.Add(CreateShipGroupResponse(character, item));

                doc["ships"] = ships;
                doc["ship_groups"] = groups;
            }

            if (flags.HasFlag(UserDataFlag.Inventory))
            {
                JsonArray inventory = new JsonArray();

                if (character.Inventory.Count == 0)
                {
                    foreach (var item in Profile.Database.Equipments.Values)
                    {
                        character.AddInventoryItem(item, 999);
                    }
                }

                foreach (var item in character.Inventory)
                    inventory.Add(JsonHelpers.ParseNodeUnbuffered(character.CreateInventoryItemResponse(item)?.ToJsonString()));

                doc["inventory"] = inventory;
            }

            if (flags.HasFlag(UserDataFlag.Crafting))
            {
                JsonArray crafting = new JsonArray();

                foreach (var item in character.Crafting)
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
                doc["checked_events"] = JsonHelpers.ParseNodeUnbuffered(character.CreateCheckedEventsResponse()?.ToJsonString());
            }

            if (flags.HasFlag(UserDataFlag.Achievements))
            {

            }

            if (flags.HasFlag(UserDataFlag.Detachments))
            {
                JsonArray detachments = new JsonArray();

                if (Profile?.CurrentSession is DiscoverySession session)
                {
                    var activeDetachments = character.Detachments.FirstOrDefault(d => d.Key == character.CurrentDetachment);
                    var slots = new JsonObject();

                    foreach (var ship in session?.Ships ?? new())
                        if (ship is not null)
                            slots[ship.Slot.ToString()] = ship.Id < 1 ? 0 : ship.Id + character.IndexSpace;

                    detachments.Add(new JsonObject()
                    {
                        ["id"] = activeDetachments.Key,
                        ["xp"] = activeDetachments.Value.Xp,
                        ["slots"] = slots,
                        ["abilities"] = JsonHelpers.ParseNodeUnbuffered(character.CreateDetachmentAbilitiesResponse(activeDetachments.Value)?.ToJsonString()),
                    });
                }
                else
                {
                    foreach (var detachment in character.Detachments)
                    {
                        detachments.Add(new JsonObject()
                        {
                            ["id"] = detachment.Key,
                            ["xp"] = detachment.Value.Xp,
                            ["slots"] = JsonHelpers.ParseNodeUnbuffered(character.CreateDetachmentSlotsResponse(detachment.Value)?.ToJsonString()),
                            ["abilities"] = JsonHelpers.ParseNodeUnbuffered(character.CreateDetachmentAbilitiesResponse(detachment.Value)?.ToJsonString()),
                        });
                    }
                }

                //foreach (var detachment in character.Detachments)
                //{
                //    detachments.Add(new JsonObject()
                //    {
                //        ["id"] = detachment.Key,
                //        ["xp"] = detachment.Value.Xp,
                //        ["slots"] = JsonHelpers.ParseNodeUnbuffered(character.CreateDetachmentSlotsResponse(detachment.Value)?.ToJsonString()),
                //        ["abilities"] = JsonHelpers.ParseNodeUnbuffered(character.CreateDetachmentAbilitiesResponse(detachment.Value)?.ToJsonString()),
                //    });
                //}

                doc["detachments"] = detachments;
            }

            if (flags.HasFlag(UserDataFlag.ProjectsResearch))
            {
                JsonArray projects = new JsonArray();

                foreach (var proj in character.ProjectResearch)
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
                var ships = new JsonArray();

                var options = new JsonSerializerOptions();
                options.Converters.Add(new InventoryAsCargoJsonConverter());

                foreach (var ship in Profile?.CurrentSession?.Ships ?? new())
                {
                    ships.Add(new JsonObject()
                    {
                        ["id"] = ship.Id,
                        ["in_galaxy"] = 1,
                        ["destroyed"] = 0,
                        ["data"] = JsonHelpers.ParseNodeUnbuffered(ship, options)?.ToJsonStringUnbuffered(false)
                    });
                }

                doc["active_ships"] = ships;
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
                doc["completed_quests"] = new JsonArray(progress?.CompletedQuests
                    .Select(q => new JsonObject { ["entity"] = q })
                    .ToArray());
            }

            if (flags.HasFlag(UserDataFlag.BGShopItems))
            {
                var shop = new JsonArray()
                {
                    new JsonObject
                    {
                        ["id"] = 1045281085,
                        ["type"] = 1,
                        ["bgc_price"] = 1,
                        ["access_level"] = 1,
                        ["faction"] = 2
                    },
                    new JsonObject
                    {
                        ["id"] = 1517501299,
                        ["type"] = 1,
                        ["bgc_price"] = 1,
                        ["access_level"] = 1,
                        ["faction"] = 2
                    },
                };

                doc["battle_ground_shop"] = shop;
            }

            return doc;
        }

        public JsonNode CreateShipResponse(FleetShipInfo ship)
        {
            if (ship is null)
                return new JsonObject();

            var character = Profile?.GameProfile?.CurrentCharacter;
            var indexSpace = character?.IndexSpace ?? 0;

            JsonNode doc = new JsonObject
            {
                ["id"] = ship.Id + indexSpace,
                ["data"] = CreateShipDataResponse(character, ship).ToJsonStringUnbuffered(false),
                ["position"] = ship.Position,
                ["kills"] = ship.Kills,
                ["death"] = ship.Death,
                ["played"] = ship.Played,
                ["woncount"] = ship.WonCount,
                ["lostcount"] = ship.LostCount,
                ["xp"] = ship.Xp,
                ["level"] = ship.Level,
                ["damagedone"] = ship.DamageDone,
                ["damagetaken"] = ship.DamageTaken,
                ["timetoconstruct"] = ship.TimeToConstruct,
                ["timetorepair"] = ship.TimeToRepair,
                ["is_favorite"] = ship.IsFavorite
            };

            if (character is not null &&
                character.HasSessionResults == true &&
                character.LastSession is DiscoverySession lastSession &&
                lastSession.StartShipsXps?.TryGetValue(ship.Id, out int startXp) == true)
            {
                doc["session_start_xp"] = startXp;
            }
            return doc;
        }

        public JsonNode CreateShipDataResponse(Character character, FleetShipInfo ship)
        {
            if (ship is null)
                return new JsonObject();

            var indexSpace = character?.IndexSpace ?? 0;
            var shipData = ship?.Data ?? new();

            JsonNode doc = new JsonObject
            {
                ["hull"] = shipData.Hull,
                ["elid"] = shipData.Id + indexSpace,
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
                ["cargo_hold_size"] = SfaDatabase.Instance.GetShipCargo(shipData.Hull),
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

        public JsonNode CreateShipGroupResponse(Character character, ShipsGroup group)
        {
            return new JsonObject
            {
                ["group_num"] = group.Number,
                ["lock_formation"] = group.LockFormation,
                ["ships"] = new JsonArray(group.Ships.Select(s => new JsonObject
                {
                    ["id"] = s.Id + character?.IndexSpace ?? 0,
                    ["x"] = s.X,
                    ["y"] = s.Y,
                    ["rot"] = s.Rotation,
                }).ToArray()),
            };
        }

        private JsonNode CreateCharacterSessionStartInventoryResponse()
        {
            var doc = new JsonArray();

            Profile.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    character.HasSessionResults == true &&
                    character.LastSession is DiscoverySession session)
                {
                    foreach (var item in session.SessionStartInventory ?? new())
                    {
                        if (item.IsEmpty)
                            continue;

                        doc.Add(new JsonObject
                        {
                            ["itemtype"] = SValue.Create((byte)item.Type),
                            ["id"] = SValue.Create(item.Id),
                            ["count"] = SValue.Create(item.Count),
                            ["unique_data"] = SValue.Create(item.UniqueData ?? ""),
                        });
                    }
                }
            });

            return doc;
        }

        protected JsonNode CreateCraftingResponce(params CraftingInfo[] crafting)
        {
            var doc = new JsonArray();

            if (crafting is null || crafting.Length < 1)
                return doc;

            foreach (var item in crafting)
                doc.Add(JsonHelpers.ParseNodeUnbuffered(item));

            return doc;
        }

        protected JsonNode CreateInventoryResponce(params InventoryItem[] inventory)
        {
            var doc = new JsonArray();

            if (inventory is null || inventory.Length < 1)
                return doc;

            foreach (var item in inventory)
            {
                doc.Add(new JsonObject
                {
                    ["itemtype"] = (byte)item.Type,
                    ["id"] = item.Id,
                    ["count"] = item.Count,
                    ["unique_data"] = item.UniqueData ?? "",
                });
            }

            return doc;
        }

        protected JsonNode CreateShipsResponce(params FleetShipInfo[] ships)
        {
            var doc = new JsonArray();

            if (ships is null || ships.Length < 1)
                return doc;

            Profile?.Use(p =>
            {
                if (p.GameProfile.CurrentCharacter is Character character)
                {
                    foreach (var item in ships)
                    {
                        doc.Add(new JsonObject
                        {
                            ["id"] = item.Id + character.IndexSpace,
                            ["data"] = character.CreateShipDataResponse(item).ToJsonString(),
                            ["position"] = item.Position,
                            ["kills"] = item.Kills,
                            ["death"] = item.Death,
                            ["played"] = item.Played,
                            ["woncount"] = item.WonCount,
                            ["lostcount"] = item.LostCount,
                            ["xp"] = item.Xp,
                            ["level"] = item.Level,
                            ["damagedone"] = item.DamageDone,
                            ["damagetaken"] = item.DamageTaken,
                            ["timetoconstruct"] = item.TimeToConstruct,
                            ["timetorepair"] = item.TimeToRepair,
                            ["is_favorite"] = item.IsFavorite
                        });
                    }
                }
            });

            return doc;
        }

        public JsonNode CreateGalaxyMapResponse(bool onlyVariableMap)
        {
            var realm = Profile?.CurrentRealm.Realm;

            if (realm is null)
                return new JsonObject();

            JsonArray exploredSystems = null;
            JsonArray exploredPlanets = null;
            JsonArray exploredPortals = null;
            JsonArray exploredMotherships = null;
            JsonArray exploredRepairStations = null;
            JsonArray exploredFuelStations = null;
            JsonArray exploredTradeStations = null;
            JsonArray exploredMMS = null;
            JsonArray exploredSCS = null;
            JsonArray exploredPiratesStations = null;
            JsonArray exploredQuickTravelGates = null;
            JsonArray exploredSecretLocs = null;

            if (Profile?.CurrentProgress is CharacterProgress progress)
            {
                exploredSystems = new();

                if (progress.Systems is not null)
                {
                    foreach (var item in progress.Systems)
                    {
                        exploredSystems.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(item.Key),
                            ["mask"] = SValue.Create(item.Value?.ToBase64String()),
                        });
                    }

                    exploredPlanets = new JsonArray(progress.Planets.Select(SValue.Create).ToArray());
                    exploredPortals = new JsonArray(progress.Portals.Select(SValue.Create).ToArray());
                    exploredMotherships = new JsonArray(progress.Motherships.Select(SValue.Create).ToArray());
                    exploredRepairStations = new JsonArray(progress.RepairStations.Select(SValue.Create).ToArray());
                    exploredFuelStations = new JsonArray(progress.Fuelstations.Select(SValue.Create).ToArray());
                    exploredTradeStations = new JsonArray(progress.TradeStations.Select(SValue.Create).ToArray());
                    exploredMMS = new JsonArray(progress.MMS.Select(SValue.Create).ToArray());
                    exploredSCS = new JsonArray(progress.SCS.Select(SValue.Create).ToArray());
                    exploredPiratesStations = new JsonArray(progress.PiratesStations.Select(SValue.Create).ToArray());
                    exploredQuickTravelGates = new JsonArray(progress.QuickTravelGates.Select(SValue.Create).ToArray());
                    exploredSecretLocs = new JsonArray(progress.SecretLocs.Select(SValue.Create).ToArray());
                }
            }

            string galaxyMap = null;

            if (onlyVariableMap == false)
                galaxyMap = (realm.GalaxyMapCache ??= JsonHelpers.SerializeUnbuffered(realm.GalaxyMap));

            JsonNode doc = new JsonObject()
            {
                ["galaxymap"] = SValue.Create(galaxyMap),

                ["variablemap"] = new JsonObject
                {
                    ["renamedsystems"] = new JsonArray(),
                    ["renamedplanets"] = new JsonArray(),
                    ["faction_event"] = new JsonArray(),
                },

                ["charactmap"] = new JsonObject
                {
                    ["exploredsystems"] = exploredSystems ?? new(),
                    ["exploredneutralplanets"] = exploredPlanets ?? new(),
                    ["exploredportals"] = exploredPortals ?? new(),
                    ["exploredmotherships"] = exploredMotherships ?? new(),
                    ["exploredrepairstations"] = exploredRepairStations ?? new(),
                    ["exploredfuelstations"] = exploredFuelStations ?? new(),
                    ["exploredtradestations"] = exploredTradeStations ?? new(),
                    ["exploredmms"] = exploredMMS ?? new(),
                    ["exploredscs"] = exploredSCS ?? new(),
                    ["exploredpiratesstations"] = exploredPiratesStations ?? new(),
                    ["exploredquicktravelgate"] = exploredQuickTravelGates ?? new(),
                    ["exploredsecretloc"] = exploredSecretLocs ?? new(),
                },
            };

            if (onlyVariableMap == true)
                doc["ok"] = 1;

            return doc;
        }
    }
}
