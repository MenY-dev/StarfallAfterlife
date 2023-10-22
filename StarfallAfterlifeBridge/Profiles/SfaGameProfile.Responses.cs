using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class SfaGameProfile
    {
        #region Main

        public JsonNode CreateAllMyPropertyResponse()
        {
            JsonNode doc = new JsonObject
            {
                ["u_bm"] = BM,
                ["u_sfc"] = SFC,
                ["u_ban"] = Ban,
                ["u_charactslotlimit"] = CharacterSlotlimit,
                ["drop_ship_progression_param_sfc"] = DropShipProgressionSFC,
                ["drop_ship_progression_param_igc"] = DropShipProgressionIGC,
                ["production_points_cost_60_sfc"] = ProductionPointsCost60SFC,
                ["production_points_cost_60_igc"] = ProductionPointsCost60IGC,
                ["rush_open_weekly_reward"] = RushOpenWeeklyReward
            };

            JsonArray shopItems = new();
            JsonArray availableSkinColors = new();
            JsonArray availableShipDecals = new();
            JsonArray availableShipSkins = new();

            if ((Database is null) == false)
            {
                int id = -1;

                foreach (var item in Database.SkinColors)
                {
                    id++;

                    shopItems.Add(new JsonObject
                    {
                        ["id"] = id,
                        ["name"] = item.Value,
                        ["description"] = "Color",
                        ["itemtype"] = 0,
                        ["igcprice"] = 100,
                        ["sfcprice"] = -1,
                        ["bmprice"] = -1,
                        ["realmoneyprice"] = -1,
                        ["itemtypespecificjson"] = new JsonObject()
                        {
                            ["skincolor_id"] = item.Key
                        }.ToJsonString()
                    });

                    availableSkinColors.Add(new JsonObject
                    {
                        ["id"] = item.Key
                    });
                }

                foreach (var item in Database.Skins)
                {
                    availableShipSkins.Add(new JsonObject
                    {
                        ["id"] = item.Key
                    });
                }

                foreach (var item in Database.Decals.Values)
                {
                    id++;

                    shopItems.Add(new JsonObject
                    {
                        ["id"] = id,
                        ["name"] = item.Name,
                        ["description"] = "Decal",
                        ["itemtype"] = 2,
                        ["igcprice"] = 100,
                        ["sfcprice"] = -1,
                        ["bmprice"] = -1,
                        ["realmoneyprice"] = -1,
                        ["itemtypespecificjson"] = new JsonObject()
                        {
                            ["decal_id"] = item.Id
                        }.ToJsonString()
                    });

                    availableShipDecals.Add(new JsonObject
                    {
                        ["id"] = item.Id
                    });
                }
            }

            doc["shop_items"] = shopItems;
            doc["available_skincolors"] = availableSkinColors;
            doc["available_decals"] = availableShipDecals;
            doc["available_shipskins"] = availableShipSkins;

            return doc;
        }

        public JsonNode CreateRealmsResponse()
        {
            JsonNode response = new JsonObject();
            JsonArray chars = new JsonArray();

            foreach (var item in DiscoveryModeProfile.Chars)
            {
                chars.Add(CreateCharacterInfoResponse(item));
            }

            JsonArray realms = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = new JsonObject
                    {
                        ["$"] = 0
                    },
                    ["name"] = new JsonObject
                    {
                        ["$"] = "NewRealm"
                    },
                    ["chars"] = chars
                }
            };

            response["elem"] = realms;

            return response;
        }

        public JsonNode CreateCharacterInfoResponse() =>
            CreateCharacterInfoResponse(CurrentCharacter);

        public JsonNode CreateCharacterInfoResponse(Character character)
        {
            return new JsonObject
            {
                ["id"] = new JsonObject
                {
                    ["$"] = character?.CurrentId ?? -1
                },
                ["name"] = new JsonObject
                {
                    ["$"] = character?.CurrentName ?? string.Empty
                },
                ["faction"] = new JsonObject
                {
                    ["$"] = character?.Faction ?? 0
                },
                ["level"] = new JsonObject
                {
                    ["$"] = character?.Level ?? 100
                }
            };
        }

        public JsonNode CreateCharactEditResponse() =>
            CreateCharactEditResponse(CurrentCharacter);

        public JsonNode CreateCharactEditResponse(Character character)
        {
            return new JsonObject
            {
                ["charactid"] = new JsonObject
                {
                    ["$"] = character?.CurrentId ?? 0
                }
            };
        }

        public JsonNode CreateCharactSelectResponse(Uri server) =>
            CreateCharactSelectResponse(CurrentCharacter, server);

        public JsonNode CreateCharactSelectResponse(Character character, Uri server)
        {
            return new JsonObject
            {
                ["charactername"] = SValue.Create(character?.CurrentName ?? string.Empty),
                ["realmmgrurl"] = new JsonObject
                {
                    ["$"] = server.ToString()
                },
                ["temporarypass"] = new JsonObject
                {
                    ["$"] = TemporaryPass ?? string.Empty
                },
                ["chatacterfaction"] = new JsonObject
                {
                    ["$"] = character?.Faction ?? 0
                },
            };
        }

        #endregion
        #region Ranked Mode

        public JsonNode CreateDraftFleetsResponse()
        {
            JsonArray fleets = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = new JsonObject
                    {
                        ["$"] = 0
                    },
                    ["name"] = new JsonObject
                    {
                        ["$"] = "0"
                    },
                    ["type"] = new JsonObject
                    {
                        ["$"] = "111"
                    },
                    ["maxships"] = new JsonObject
                    {
                        ["$"] = 20
                    },
                    ["ships"] = new JsonArray()
                }
            };

            return new JsonObject
            {
                ["fleets"] = fleets,
                ["userbm"] = new JsonObject
                {
                    ["$"] = 1
                }
            };
        }

        #endregion
    }
}
