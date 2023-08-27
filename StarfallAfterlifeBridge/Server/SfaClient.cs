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

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClient : SfaClientBase
    {
        protected SfaGame Game { get; set; }

        protected SfaGameProfile Profile => Game?.GameProfile;

        protected DiscoveryChannel DiscoveryChannel => Game?.DiscoveryChannel;

        protected GalacticChannel GalacticChannel => Game?.GalacticChannel;

        protected BattleGroundChannel BattleGroundChannel => Game?.BattleGroundChannel;

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
            }
        }

        protected override void OnTextInput(string text, SfaServerAction action)
        {
            base.OnTextInput(text, action);

            switch (action)
            {
                case SfaServerAction.GlobalChat:
                    ReciveGlobalChat(text);
                    break;

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

        public Task<bool> Auth(SfaGameProfile profile)
        {
            return SendRequest(SfaServerAction.Auth, new JObject
            {
                ["action"] = "server_auth",
                ["profile_id"] = Guid.Empty,
                ["profile_name"] = profile.Nickname,
            }
            ).ContinueWith(t =>
            {
                if (t.Result is SfaClientResponse response &&
                    response.IsSuccess == true &&
                    response.Action == SfaServerAction.Auth &&
                    JsonHelpers.ParseNodeUnbuffered(response.Text) is JObject doc &&
                    (bool?)doc["auth_success"] == true)
                {
                    var realmId = (string)doc["realm_id"];

                    if (realmId is null)
                        return false;

                    Game.Profile.Use(p =>
                    {
                        var realm = p.GetProfileRealm(realmId);

                        if (realm is null)
                        {
                            realm = p.AddNewRealm(new SfaRealm
                            {
                                Id = realmId,
                                Database = p.Database,
                            });

                            realm.Save();
                        }

                        p.SelectRealm(realm);
                        p.CurrentRealm.LoadDatabase();
                        p.CurrentRealm.LoadProgress();
                    });

                    return true;
                }

                return false;
            });
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
            var chars = playerData?["chars"]?.AsArraySelf();
            var profile = Profile.DiscoveryModeProfile;

            if (chars is not null)
            {
                foreach (var charData in chars)
                {
                    if (profile.GetCharById((int?)charData["id"] ?? -1) is Character c)
                    {
                        c.Database = Game.Profile.Database;
                        c.UniqueId = (int?)charData["unique_id"] ?? -1;
                        c.UniqueName = (string)charData["unique_name"];
                    }
                }
            }
        }

        public Task<bool> LoadGalaxyMap()
        {
            return SendRequest(SfaServerAction.LoadGalaxyMap).ContinueWith((t, game)=>
            {
                var doc = JsonHelpers.ParseNodeUnbuffered(t.Result.Text);
                if (t.Result is SfaClientResponse response &&
                    response.IsSuccess == true &&
                    response.Action == SfaServerAction.LoadGalaxyMap &&
                    doc is JObject)
                {
                    (game as SfaGame).Profile.Use((p, doc) =>
                    {
                        var cache = (string)doc["map"] ?? string.Empty;
                        p.CurrentRealm.Realm.GalaxyMapCache = cache;
                        p.CurrentRealm.Realm.GalaxyMapHash = (string)doc["hash"] ?? string.Empty;
                        p.CurrentRealm?.Save();
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    }, doc);
                    return true;
                }

                return false;
            }, Game);
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

                    if (session is not null)
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

                    doc["char_data"] = JsonHelpers.ParseNode(character?.CreateCharacterResponse(UserDataFlag.All)?.ToJsonString());
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

        private void ReciveGlobalChat(string text)
        {
            Game?.VanguardChatChannel?.SendMessage("SFA", text, 85);
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

                if (doc["systems"]?.AsArray() is JArray systems)
                {
                    foreach (var system in systems)
                    {
                        if (system is JObject &&
                            (int?)system["system_id"] is int systemId &&
                            (string)system["progress"] is string map)
                        {
                            progress.SetSystemProgress(systemId, new SystemHexMap(map));

                            if (system["new_objects"]?.AsArray() is JArray newObjects)
                            {
                                foreach (var item in newObjects)
                                {
                                    if ((byte?)item["type"] is byte type &&
                                        (int?)item["id"] is int id)
                                    {
                                        progress.AddObject((DiscoveryObjectType)type, id, systemId);
                                    }
                                }
                            }
                        }
                    }
                }

                profile.SaveCharacterProgress();
            }
        }

        private void ProcessCharacterInventory(JNode doc, SfaClientRequest request)
        {
            var respomce = new JObject();
            var inventoryUpdated = false;

            Game?.Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character &&
                    character.Inventory is InventoryStorage inventory)
                {
                    switch ((string)doc?["action"])
                    {
                        case "get":
                            {
                                if ((int?)doc?["id"] is int id &&
                                    inventory[id] is InventoryItem item)
                                    respomce["item"] = JsonHelpers.ParseNode(item);
                            }
                            break;

                        case "add":
                            {
                                if ((int?)doc?["id"] is int id &&
                                    (InventoryItemType?)(byte?)doc?["type"] is InventoryItemType type &&
                                    (int?)doc?["count"] is int count)
                                {
                                    var item = new InventoryItem
                                    {
                                        Id = id,
                                        Type = type,
                                        Count = count
                                    };

                                    if (inventory.Add(item) is InventoryItem newItem)
                                    {
                                        respomce["result"] = Math.Max(0, newItem.Count);
                                        inventoryUpdated = true;
                                    }
                                }
                            }
                            break;


                        case "remove":
                            {
                                if ((int?)doc?["id"] is int id &&
                                    (int?)doc?["count"] is int count)
                                {
                                    respomce["result"] = inventory.Remove(id, count);
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

            request?.SendResponce(respomce, SfaServerAction.CharacterInventory);
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
                        character.CreateDiscoveryCharacterDataResponse()?.ToJsonString(),
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
                        .Where(i => i is not null)
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

                    default:
                        break;
                }
            }
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
    }
}
