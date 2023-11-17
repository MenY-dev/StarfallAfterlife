using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JNode = System.Text.Json.Nodes.JsonNode;
using JObject = System.Text.Json.Nodes.JsonObject;
using JArray = System.Text.Json.Nodes.JsonArray;
using JValue = System.Text.Json.Nodes.JsonValue;
using StarfallAfterlife.Bridge.Serialization;
using System.Diagnostics;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Database;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO.Compression;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClient : SfaClientBase
    {
        public string ServerAuth { get; protected set; }

        public string RealmId { get; protected set; }

        public string GalaxyHash { get; protected set; }

        public string MobsMapHash { get; protected set; }

        public string AsteroidsMapHash { get; protected set; }

        protected SfaGame Game { get; set; }

        protected SfaGameProfile Profile => Game?.GameProfile;

        protected DiscoveryChannel DiscoveryChannel => Game?.DiscoveryChannel;

        protected GalacticChannel GalacticChannel => Game?.GalacticChannel;

        protected QuickMatchChannel QuickMatchChannel => Game?.QuickMatchChannel;

        protected BattleGroundChannel BattleGroundChannel => Game?.BattleGroundChannel;

        protected FriendChannel CharacterFriendsChannel => Game?.CharacterFriendsChannel;

        protected FriendChannel UserFriendsChannel => Game?.UserFriendsChannel;

        public SfaClient(SfaGame game)
        {
            Game = game;

            if (Game is null)
                return;
        }

        protected override void OnBinaryInput(SfReader reader, SfaServerAction action)
        {
            base.OnBinaryInput(reader, action);

            switch (action)
            {
                case SfaServerAction.DiscoveryChannel:
                    DiscoveryChannel?.Send(reader.ReadToEnd());
                    break;
                case SfaServerAction.GalacticChannel:
                    GalacticChannel?.Send(reader.ReadToEnd());
                    break;
                case SfaServerAction.BattleGroundChannel:
                    BattleGroundChannel?.Send(reader.ReadToEnd());
                    break;
                case SfaServerAction.QuickMatchChannel:
                    QuickMatchChannel?.Send(reader.ReadToEnd());
                    break;
                case SfaServerAction.CharacterFriendChannel:
                    CharacterFriendsChannel?.Send(reader.ReadToEnd());
                    break;
                case SfaServerAction.UserFriendChannel:
                    UserFriendsChannel?.Send(reader.ReadToEnd());
                    break;
            }
        }

        protected override void OnTextInput(string text, SfaServerAction action)
        {
            base.OnTextInput(text, action);

            switch (action)
            {
                case SfaServerAction.SyncProgress:
                    SyncProgress(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.SyncGalaxySessionData:
                    SyncSessionData(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.EndSession:
                    SyncSessionEnd(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.DropSession:
                    SyncSessionDrop(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.StartBattle:
                    ProcessStartBattle(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.AddCharacterCurrencies:
                    ProcessAddCharacterCurrencies(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.SaveShipsGroup:
                    ProcessSaveShipsGroup(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.Chat:
                    ProcessNewChatMessage(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.AddNewCharacterStats:
                    ProcessAddNewCharacterStats(JsonHelpers.ParseNodeUnbuffered(text));
                    break;
            }
        }

        protected override void OnRequestInput(SfaClientRequest request)
        {
            base.OnRequestInput(request);

            switch (request.Action)
            {
                case SfaServerAction.StartSession:
                    ProcessStartSession(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;

                case SfaServerAction.CharacterInventory:
                    ProcessCharacterInventory(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;

                case SfaServerAction.RequestCharacterDiscoveryData:
                    ProcessRequestCharacterDiscoveryData(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;

                case SfaServerAction.RequestCharacterShipData:
                    ProcessRequrstCharacterShipData(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;

                case SfaServerAction.RequestItemResearch:
                    ProcessRequestItemResearch(JsonHelpers.ParseNodeUnbuffered(request.Text), request);
                    break;
            }
        }

        public Task<bool> Auth(SfaGameProfile profile, string password = null, string lastAuth = null)
        {
            var request = new JObject();

            if (lastAuth is not null)
            {
                request["action"] = "restore_session";
                request["password"] = password;
                request["auth"] = lastAuth;
            }
            else
            {
                request["action"] = "server_auth";
                request["password"] = password;
                request["profile_id"] = Profile?.Id ?? Guid.Empty;
                request["profile_name"] = profile.Nickname;
            }

            return SendRequest(SfaServerAction.Auth, request).ContinueWith(t =>
            {
                if (t.Result is SfaClientResponse response &&
                    response.IsSuccess == true &&
                    response.Action == SfaServerAction.Auth &&
                    JsonHelpers.ParseNodeUnbuffered(response.Text) is JObject doc &&
                    (bool?)doc["auth_success"] == true)
                {
                    ServerAuth = (string)doc["auth"];
                    var realmId = RealmId = (string)doc["realm_id"];

                    if (realmId is null)
                        return false;

                    Game.Profile.Use(p =>
                    {
                        var realmInfo = p.GetProfileRealm(realmId);

                        if (realmInfo is null || realmInfo.Realm is null)
                        {
                            realmInfo = p.AddNewRealm(new SfaRealm
                            {
                                Id = realmId,
                                Database = p.Database
                            });

                            realmInfo.Save();
                        }

                        realmInfo.Realm.LastAuth = ServerAuth;
                        realmInfo.SaveInfo();

                        GalaxyHash = (string)doc["galaxy_hash"];
                        MobsMapHash = (string)doc["mobs_map_hash"];
                        AsteroidsMapHash = (string)doc["asteroids_map_hash"];

                        p.SelectRealm(realmInfo);
                        p.CurrentRealm.LoadDatabase();
                        p.CurrentRealm.LoadProgress();
                    });

                    return true;
                }

                return false;
            });
        }


        public void SendChannelRegister(string channelName)
        {
            Send(new JObject()
            {
                ["name"] = channelName,
            }, SfaServerAction.RegisterChannel);
        }

        public Task<bool> SyncPlayerData()
        {
            return SendRequest(
                SfaServerAction.RegisterPlayer,
                Profile.CreatePlayerDataRequest().ToJsonString()
            ).ContinueWith(t =>
            {
                if (t.Result is SfaClientResponse response &&
                    response.IsSuccess == true &&
                    response.Action == SfaServerAction.RegisterPlayer)
                {
                    ProcessPlayerData(JsonHelpers.ParseNodeUnbuffered(response.Text));
                    return true;
                }

                return false;
            });
        }

        public void ProcessPlayerData(JNode playerData)
        {
            if (playerData is not JObject)
                return;

            Game?.Profile?.Use(p =>
            {
                if (p.GameProfile is SfaGameProfile gameProfile)
                {
                    gameProfile.Seasons = playerData["seasons"]?.DeserializeUnbuffered<WeeklyQuestsInfo>() ?? new();

                    var chars = playerData["chars"]?.AsArraySelf();
                    var discoveryProfile = gameProfile.DiscoveryModeProfile;

                    if (chars is not null && discoveryProfile is not null)
                    {
                        foreach (var charData in chars)
                        {
                            if (discoveryProfile.GetCharById((int?)charData["id"] ?? -1) is Character character)
                            {
                                character.Database = Game.Profile.Database;
                                character.UniqueId = (int?)charData["unique_id"] ?? -1;
                                character.UniqueName = (string)charData["unique_name"];
                            }
                        }
                    }
                }
            });
        }

        public Task<bool> LoadGalaxyMap()
        {
            return SendBinaryRequest(SfaServerAction.LoadGalaxyMap, default).ContinueWith(t =>
            {
                if (t.Result is SfaClientResponse response &&
                    response.IsSuccess == true &&
                    response.Action == SfaServerAction.LoadGalaxyMap &&
                    Game?.Profile is SfaProfile profile)
                {
                    var result = true;

                    profile.Use((p, data) =>
                    {
                        try
                        {
                            using var input = new ReadOnlyMemoryStream(data);
                            using var output = new MemoryStream();
                            using var deflate = new DeflateStream(input, CompressionMode.Decompress, true);
                            deflate.CopyTo(output);
                            deflate.Flush();

                            var map = Encoding.UTF8.GetString(output.GetBuffer(), 0, (int)output.Length);
                            var realmInfo = p.CurrentRealm;

                            if (realmInfo is null)
                            {
                                result = false;
                                return;
                            }

                            if (map is null)
                            {
                                result = false;
                                return;
                            }

                            p.CurrentRealm.Realm.GalaxyMapCache = map;
                            p.CurrentRealm.Realm.GalaxyMapHash = GalaxyHash;
                            p.MapsCache.Save(GalaxyHash, map);
                        }
                        catch
                        {
                            result = false;
                        }
                    }, response.Data);

                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                    return result;
                }

                return false;
            });
        }

        public void ProcessStartSession(JNode entryData, SfaClientRequest request)
        {
            var charId = (int?)entryData["character_id"] ?? -1;

            Game.Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    character.UniqueId == charId)
                {
                    var doc = new JObject();
                    var session = p.CurrentSession;

                    if (session is not null && session.Ships is not null and { Count: > 0 })
                    {
                        var ships = JsonHelpers.ParseNode(session.Ships ?? new())?.AsArray() ?? new();

                        if (character.IndexSpace > 0)
                        {
                            foreach (var ship in ships)
                                if (ship is not null)
                                    ship["elid"] = ((int?)ship["elid"] ?? 0) + character.IndexSpace;
                        }

                        doc["active_session"] = new JObject
                        {
                            ["system"] = session.SystemId,
                            ["location"] = JsonHelpers.ParseNode(session.Location),
                            ["ships"] = ships,
                        };
                    }
                    else
                    {
                        session = p.StartNewSessionForCurrentChar();
                        session?.Save();
                    }

                    doc["char_data"] = JsonHelpers.ParseNode(Game.CreateCharacterResponse(character, UserDataFlag.All)?.ToJsonString());
                    request.SendResponce(doc.ToJsonString(), SfaServerAction.StartSession);
                }
            });
        }

        public Task SendCharacterSelect()
        {
            Task result;

            Game?.Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character)
                {
                    var doc = new JObject
                    {
                        ["id"] = character.Id,
                        ["unique_id"] = character.UniqueId,
                        ["progress"] = JsonHelpers.ParseNodeUnbuffered(Game?.Profile.CurrentProgress.ToJsonString(false))
                    };

                    result = SendRequest(SfaServerAction.SyncCharacterSelect, doc);
                }
            });

            return Task.CompletedTask;
        }

        public Task<JObject> RequestFullGalaxySesionData()
        {
            return SendRequest(SfaServerAction.GetFullGalaxySessionData, new JObject
            {

            }, 10000).ContinueWith<JObject>(t =>
            {
                if (t.Result is SfaClientResponse response &&
                    response?.IsSuccess == true &&
                    response.Action == SfaServerAction.GetFullGalaxySessionData &&
                    JsonHelpers.ParseNodeUnbuffered(response.Text) is JObject doc)
                        return doc;

                return new();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Game = null;
        }

        protected void SyncProgress(JNode doc)
        {
            if (Game?.Profile is SfaProfile profile &&
                profile.CurrentProgress is CharacterProgress progress &&
                doc is JObject)
            {
                if (doc["new_active_quests"]?.AsArray() is JArray activeQuests)
                {
                    foreach (var item in activeQuests)
                    {
                        if (item is JObject quest &&
                            (int?)quest["id"] is int id)
                        {
                            var questProgress = quest["progress"]?
                                .DeserializeUnbuffered<QuestProgress>() ??
                                new QuestProgress();

                            if (progress.ActiveQuests.TryGetValue(id, out var currentQuest) == false ||
                                currentQuest is null)
                                progress.ActiveQuests[id] = questProgress;
                        }
                    }
                }

                if (doc["updated_quests"]?.AsArray() is JArray updatedQuests)
                {
                    foreach (var item in updatedQuests)
                    {
                        if (item is JObject quest &&
                            (int?)quest["id"] is int id)
                        {
                            var questProgress = quest["progress"]?
                                .DeserializeUnbuffered<QuestProgress>();

                            if (questProgress is not null &&
                                progress.ActiveQuests.ContainsKey(id) == true)
                                progress.ActiveQuests[id] = questProgress;
                        }
                    }
                }

                if (doc["new_completed_quests"]?.AsArray() is JArray completedQuests)
                {
                    foreach (var item in completedQuests)
                    {
                        if ((int?)item is int quest)
                        {
                            progress.ActiveQuests.Remove(quest);
                            progress.CompletedQuests.Add(quest);
                        }
                    }
                }

                if (doc["new_rewards"]?.AsArray() is JArray rewards)
                {
                    foreach (var item in rewards)
                    {
                        if ((int?)item is int rewardsId)
                        {
                            progress.TakenRewards.Add(rewardsId);
                        }
                    }
                }

                if (doc["new_secrets"]?.AsArray() is JArray secrets)
                {
                    foreach (var item in secrets)
                    {
                        if ((int?)item is int secretId)
                        {
                            progress.SecretLocs.Add(secretId);
                        }
                    }
                }

                if (doc["systems"]?.AsArray() is JArray systems)
                {
                    foreach (var system in systems)
                    {
                        if (system is JObject &&
                            (int?)system["system_id"] is int systemId &&
                            (string)system["progress"] is string map)
                        {
                            progress.SetSystemProgress(systemId, new SystemHexMap(map));
                        }
                    }
                }

                if (doc["new_objects"]?.AsArray() is JArray objs)
                {
                    foreach (var obj in objs)
                    {
                        if ((int?)obj["type"] is int type &&
                            (int?)obj["id"] is int id)
                        {
                            progress.AddObject((DiscoveryObjectType)type, id);
                        }
                    }
                }

                if (doc["new_warp_systems"]?.AsArray() is JArray warpSystems)
                {
                    foreach (var item in warpSystems)
                    {
                        if ((int?)item is int systemId)
                            progress.AddWarpSystem(systemId);
                    }
                }

                if (doc["new_season_rewards"]?.AsArray() is JArray newSeasonRevards)
                {
                    foreach (var item in newSeasonRevards)
                    {
                        if ((int?)item is int rewardId)
                            progress.AddSeasonReward(rewardId);
                    }
                }

                if (doc["new_season_progress"] is JObject newSeasonProgress)
                {
                    if ((int?)newSeasonProgress["id"] is int id &&
                        (int?)newSeasonProgress["xp"] is int xp)
                        progress.AddSeasonProgress(id, xp);
                }

                profile.SaveCharacterProgress();
            }
        }

        private void ProcessCharacterInventory(JNode doc, SfaClientRequest request)
        {
            var responce = new JObject();
            var inventoryUpdated = false;

            Game?.Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    character.Inventory is InventoryStorage inventory)
                {
                    if (doc is null)
                        return;

                    switch ((string)doc["action"])
                    {
                        case "get":
                            {
                                if ((int?)doc["id"] is int id &&
                                    inventory[id, (string)doc["unique_data"]] is InventoryItem item)
                                    responce["item"] = JsonHelpers.ParseNode(item);
                            }
                            break;

                        case "add":
                            {
                                if ((int?)doc["id"] is int id &&
                                    (InventoryItemType?)(byte?)doc["type"] is InventoryItemType type &&
                                    (int?)doc["count"] is int count)
                                {
                                    var item = new InventoryItem
                                    {
                                        Id = id,
                                        Type = type,
                                        Count = count,
                                        UniqueData = (string)doc["unique_data"]
                                    };

                                    var result = inventory.Add(item, count);

                                    if (result > 0)
                                    {
                                        responce["result"] = Math.Max(0, result);
                                        inventoryUpdated = true;
                                    }
                                }
                            }
                            break;


                        case "remove":
                            {
                                if ((int?)doc["id"] is int id &&
                                    (int?)doc["count"] is int count)
                                {
                                    responce["result"] = inventory.Remove(id, count, (string)doc["unique_data"]);
                                    inventoryUpdated = true;
                                }
                            }
                            break;

                        default:
                            break;
                    }

                    SyncCharacterCurrencies(character);
                }

                if (inventoryUpdated == true)
                    p.SaveGameProfile();
            });


            if (inventoryUpdated == true)
                Game?.DiscoveryChannel?.SendInventory();

            request?.SendResponce(responce, SfaServerAction.CharacterInventory);
        }


        private void ProcessRequestCharacterDiscoveryData(JNode doc, SfaClientRequest request)
        {
            if (doc is not JObject)
                return;

            Game?.Profile.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character)
                {
                    request.SendResponce(
                        character.CreateDiscoveryCharacterDataResponse((byte?)doc["all_ships"] == 1)?.ToJsonString(),
                        SfaServerAction.RequestCharacterDiscoveryData);
                }
            });
        }


        private void ProcessRequrstCharacterShipData(JNode doc, SfaClientRequest request)
        {
            if (doc is not JObject)
                return;

            Game?.Profile.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    (int?)doc["ship_id"] is int shipId &&
                    character.GetShip(shipId - character.IndexSpace) is FleetShipInfo shipInfo &&
                    shipInfo.Data is ShipConstructionInfo ship &&
                    JsonHelpers.ParseNodeUnbuffered(ship) is JObject data)
                {
                    data["elid"] = ((int?)data["elid"] ?? 0) + character.IndexSpace;

                    request.SendResponce(new JObject
                    {
                        ["data"] = data
                    }, SfaServerAction.RequestCharacterShipData);
                }
            });
        }

        private void ProcessRequestItemResearch(JNode doc, SfaClientRequest request)
        {
            if (doc is not JObject)
                return;

            Game?.Profile.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    (int?)doc["entity_id"] is int entityId)
                {
                    var result = character.ProjectResearch?.Any(p => p?.Entity == entityId) == true;

                    request.SendResponce(new JObject
                    {
                        ["explored"] = result,
                    }, SfaServerAction.RequestItemResearch);
                }
            });
        }

        public void SyncSessionData(JNode doc)
        {
            if (doc is not JObject)
                return;

            Game?.Profile.Use(p =>
            {
                if (p.CurrentSession is DiscoverySession session)
                {
                    if ((int?)doc["system"] is int system)
                        session.SystemId = system;

                    if (doc["location"]?.DeserializeUnbuffered<Vector2>() is Vector2 location)
                        session.Location = location;

                    if (doc["ships"]?.DeserializeUnbuffered<List<ShipConstructionInfo>>() is List<ShipConstructionInfo> ships)
                    {
                        if (p.GameProfile.CurrentCharacter is Character character &&
                            character.IndexSpace > 0)
                        {
                            foreach (var ship in ships)
                                if (ship is not null)
                                    ship.Id -= character.IndexSpace;
                        }


                        session.Ships ??= new();
                        session.Ships.Clear();
                        session.Ships.AddRange(ships);

                        if (session.Ships.Count < 1)
                        {
                            DropSession();
                        }
                    }

                    session.Save();
                }
            });
        }

        protected void SyncSessionDrop(JNode doc)
        {
            DropSession();
        }

        protected void DropSession()
        {
            Game?.Profile.Use(p =>
            {
                if (p.CurrentSession is DiscoverySession session &&
                    p.GameProfile.CurrentCharacter is Character character)
                {
                    p.CurrentSession = null;
                    p.Sessions?.Remove(session);
                    session.RemoveSessionFile();
                    character.LastSession = session;
                    character.HasSessionResults = true;
                }

                p.SaveGameProfile();
            });
        }

        protected void SyncSessionEnd(JNode doc)
        {
            Game?.Profile.Use(p =>
            {
                if (p.CurrentSession is DiscoverySession session &&
                    p.GameProfile.CurrentCharacter is Character character)
                {
                    p.CurrentSession = null;
                    p.Sessions?.Remove(session);
                    session.RemoveSessionFile();

                    var newItems = session.Ships?
                        .Where(s => s?.Cargo is not null)
                        .SelectMany(s => s.Cargo)
                        .Where(i => i.IsEmpty == false)
                        .ToList() ?? new();

                    foreach (var item in newItems)
                    {
                        character.AddInventoryItem(item, item.Count);
                    }

                    character.LastSession = session;
                    character.HasSessionResults = true;
                }

                p.SaveGameProfile();
            });
        }

        private void ProcessStartBattle(JNode doc)
        {
            if (doc is JObject &&
                (string)doc["game_mode"] is string gameMode &&
                (string)doc["address"] is string address &&
                (int?)doc["port"] is int port &&
                (string)doc["auth"] is string auth)
            {
                address = RemapInstanceAddress(address);

                switch (gameMode)
                {
                    case "discovery":
                        DiscoveryChannel?.SendConnectToInstance(
                            (int?)doc["system_id"] ?? 0,
                            (int?)doc["char_id"] ?? -1,
                            address,
                            port,
                            auth);
                        break;

                    case "battlegrounds":
                        BattleGroundChannel?.SendInstanceReady(
                            address,
                            port,
                            auth);
                        break;

                    case "quick_match":
                        QuickMatchChannel?.SendInstanceReady(
                            address,
                            port,
                            auth);
                        break;
                    default:
                        break;
                }
            }
        }

        public string RemapInstanceAddress(string address)
        {
            if (IPAddress.TryParse(address, out var ip) == true)
            {
                if (ip.IsIPv4MappedToIPv6 == true)
                    ip = ip.MapToIPv4();

                if (ip.Equals(IPAddress.Any) == true)
                {
                    IPAddress serverIp = null;

                    try
                    {
                        serverIp = Dns.GetHostAddresses(
                            Game?.ServerAddress?.Host ?? "",
                            AddressFamily.InterNetwork)?
                            .FirstOrDefault();
                    }
                    catch { }

                    address = (serverIp ?? IPAddress.Loopback).ToString();
                }
            }

            return address;
        }

        private void ProcessAddCharacterCurrencies(JNode doc)
        {
            Game?.Profile.Use(p =>
            {
                if (doc is JObject &&
                    (int?)doc["char_id"] is int charId &&
                    p.GameProfile?.DiscoveryModeProfile.Chars is List<Character> chars &&
                    chars.FirstOrDefault(c => c.UniqueId == charId) is Character character)
                {
                    if ((int?)doc["igc"] is int igc)
                        character.IGC += igc;

                    if ((int?)doc["bgc"] is int bgc)
                        character.BGC += bgc;

                    if ((int?)doc["xp"] is int xp)
                        character.AddXp(xp);

                    if (doc["ships_xp"]?.DeserializeUnbuffered<Dictionary<int, int>>() is Dictionary<int, int> ships)
                    {
                        foreach (var item in ships)
                        {
                            if (character.GetShip(item.Key - character.IndexSpace) is FleetShipInfo fleetShip &&
                                fleetShip.Data is ShipConstructionInfo ship)
                            {
                                var newXp = Math.Max(fleetShip.Xp, ship.Xp) + item.Value;
                                var newLevel = SfaDatabase.Instance.GetLevelForShipXp(ship.Hull, newXp);
                                fleetShip.Xp = ship.Xp = newXp;
                                fleetShip.Level = Math.Max(fleetShip.Level, newLevel);
                            }
                        }
                    }

                    GalacticChannel?.SendCharacterCurrencyUpdate();
                    GalacticChannel?.SendCharacterXpUpdate();
                    SyncCharacterCurrencies(character);
                }

                p.SaveGameProfile();
            });
        }

        public void SyncCharacterCurrencies(Character character)
        {
            if (character is null)
                return;

            var doc = new JObject
            {
                ["char_id"] = character.UniqueId,
                ["xp"] = character.Xp,
                ["level"] = character.Level,
                ["access_level"] = character.AccessLevel,
                ["igc"] = character.IGC,
                ["bgc"] = character.BGC,
            };

            Send(doc, SfaServerAction.SyncCharacterCurrencies);
        }

        public void SyncCharacterNewResearch(Character character, IEnumerable<int> newItems)
        {
            if (character is null || newItems is null)
                return;

            Send(new JObject
            {
                ["char_id"] = character.UniqueId,
                ["new_items"] = JsonHelpers.ParseNodeUnbuffered(newItems),
            }, SfaServerAction.SyncCharacterNewResearch);
        }


        private void ProcessSaveShipsGroup(JNode doc)
        {
            Game?.Profile.Use(p =>
            {
                if ((int?)doc?["char_id"] is int charId &&
                    p.GameProfile?.DiscoveryModeProfile is DiscoveryProfile profile &&
                    profile.GetCharByUniqueId(charId) is Character character &&
                    JsonHelpers.DeserializeUnbuffered<ShipsGroup>((string)doc?["group"] ?? "") is ShipsGroup newGroup)
                {
                    var groups = character.ShipGroups ??= new();

                    foreach (var item in newGroup.Ships)
                        if (item is not null)
                            item.Id -= character.IndexSpace;

                    groups.RemoveAll(g => g?.Number == newGroup.Number);
                    groups.Add(newGroup);
                    p.SaveGameProfile();
                }
            });
        }

        public void TakeCharactRewardFromQueue(int charId, int rewardId)
        {
            Send(new JObject
            {
                ["char_id"] = charId,
                ["reward_id"] = rewardId,
            }, SfaServerAction.TakeCharactRewardFromQueue);
        }

        public void SendChatMessage(string channel, string msg, bool isPrivate = false, string receiver = null)
        {
            Send(new JObject
            {
                ["channel"] = channel,
                ["receiver"] = receiver,
                ["msg"] = msg,
                ["is_private"] = isPrivate,
            }, SfaServerAction.Chat);
        }

        public void ProcessNewChatMessage(JNode doc)
        {
            if (doc is null)
                return;

            if ((string)doc["channel"] is string channelName &&
                (string)doc["sender"] is string sender &&
                (string)doc["msg"] is string msg &&
                (bool?)doc["is_private"] is bool isPrivate &&
                Game?.GetChannel(channelName) is ChatChannel channel)
            {
                channel.SendMessage(sender, msg, isPrivate);
            }
        }

        private void ProcessAddNewCharacterStats(JNode doc)
        {
            if (doc?.DeserializeUnbuffered<Dictionary<string,float>>() is Dictionary<string, float> newStats)
            {
                Game?.Profile?.Use(p =>
                {
                    if (p.GameProfile?.CurrentCharacter is Character character)
                    {
                        var stats = character.Statistic ??= new();

                        foreach (var item in newStats)
                        {
                            if (item.Key is null)
                                continue;

                            var currentValue = stats.GetValueOrDefault(item.Key, 0);
                            stats[item.Key] = currentValue + item.Value;
                        }
                    }

                    p.SaveGameProfile();
                });
            }
        }
    }
}
