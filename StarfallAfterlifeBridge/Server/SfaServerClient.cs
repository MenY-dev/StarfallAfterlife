using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JNode = System.Text.Json.Nodes.JsonNode;
using JObject = System.Text.Json.Nodes.JsonObject;
using JArray = System.Text.Json.Nodes.JsonArray;
using JValue = System.Text.Json.Nodes.JsonValue;
using StarfallAfterlife.Bridge.Serialization;
using System.Text.Json.Serialization.Metadata;
using StarfallAfterlife.Bridge.Server.Characters;
using System.Net;
using System.Reflection;
using System.Text.Json.Nodes;
using System.IO.Compression;
using StarfallAfterlife.Bridge.Diagnostics;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient : SfaClientBase
    {
        public string Name { get; set; }

        public string UniqueName { get; set; }

        public string Auth { get; set; }

        public int PlayerId { get; set; } = -1;

        public int IndexSpace => PlayerId < 0 ? 0 : PlayerId * 2000;

        public Guid ProfileId { get; set; }

        public SfaClientState State { get; set; } = SfaClientState.PendingLogin;

        public UserInGameStatus UserStatus { get; set; } = UserInGameStatus.None;

        public List<RankedFleetInfo> RankedFleets { get; set; } = new();

        public int SelectedRankedFleet { get; set; } = 0;

        public bool IsSpectator { get; set; }

        public bool IsPlayer { get; set; }

        public SfaServer Server { get; set; }

        public DiscoveryGalaxy Galaxy { get; set; }

        public GalaxyMap Map => Server.Realm.GalaxyMap;

        public ServerCharacter CurrentCharacter => DiscoveryClient?.CurrentCharacter;

        public DiscoveryClient DiscoveryClient { get; set; }

        protected override void OnBinaryInput(SfReader reader, SfaServerAction action)
        {
            base.OnBinaryInput(reader, action);

            switch (action)
            {
                case SfaServerAction.DiscoveryChannel:
                    DiscoveryClient?.InputFromDiscoveryChannel(reader);
                    break;
                case SfaServerAction.GalacticChannel:
                    DiscoveryClient?.InputFromGalacticChannel(reader);
                    break;
                case SfaServerAction.BattleGroundChannel:
                    DiscoveryClient?.InputFromBattleGroundChannel(reader);
                    break;
                case SfaServerAction.QuickMatchChannel:
                    DiscoveryClient?.InputFromQuickMatchChannel(reader);
                    break;
                case SfaServerAction.UserFriendChannel:
                    InputFromFriendChannel(reader, false);
                    break;
                case SfaServerAction.CharacterFriendChannel:
                    InputFromFriendChannel(reader, true);
                    break;
                case SfaServerAction.CharacterPartyChannel:
                    DiscoveryClient?.InputFromCharacterPartyChannel(reader);
                    break;
                case SfaServerAction.MatchmakerChannel:
                    InputFromMatchmakerChannel(reader);
                    break;
            }
        }

        protected override void OnTextInput(string text, SfaServerAction action)
        {
            base.OnTextInput(text, action);

            switch (action)
            {
                case SfaServerAction.DeleteCharacter:
                    ProcessDeleteChar(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.SyncCharacterCurrencies:
                    ProcessSyncCharacterCurrencies(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.SyncCharacterNewResearch:
                    ProcessSyncCharacterNewResearch(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                case SfaServerAction.SyncRankedFleets:
                    ProcessSyncRankedFleets(JsonHelpers.ParseNodeUnbuffered(text));
                    break;

                default:

                    if (action == SfaServerAction.RegisterChannel)
                        ProcessRegisterChannel(JsonHelpers.ParseNodeUnbuffered(text));

                    DiscoveryClient?.OnTextReceive(text, action);
                    break;
            }
        }

        protected override void OnRequestInput(SfaClientRequest request)
        {
            base.OnRequestInput(request);

            switch (request.Action)
            {
                case SfaServerAction.Auth:
                    ProcessAuth(JsonHelpers.ParseNodeUnbuffered(request.Text)?.AsObjectSelf(), request);
                    break;

                case SfaServerAction.GetServerInfo:
                    ProcessGetServerInfo(request);
                    break;

                case SfaServerAction.LoadGalaxyMap:
                    ProcessLoadGalaxyMap(request);
                    break;

                case SfaServerAction.LoadVariableMap:
                    ProcessLoadVariableMap(request);
                    break;

                case SfaServerAction.RegisterPlayer:
                    ProcessRegisterPlayer(JsonHelpers.ParseNodeUnbuffered(request.Text)?.AsObjectSelf(), request);
                    break;

                case SfaServerAction.RegisterNewCharacters:
                    ProcessRegisterNewChars(JsonHelpers.ParseNodeUnbuffered(request.Text)?.AsObjectSelf(), request);
                    break;

                default: 
                    DiscoveryClient?.OnRequestReceive(request);
                    break;
            }
        }

        public void ProcessAuth(JNode authData, SfaClientRequest request)
        {
            if ((string)authData["action"] is string action)
            {
                var comparsion = StringComparison.InvariantCultureIgnoreCase;

                if ((string)authData["version"] is string versionText &&
                    Version.TryParse(versionText, out var version) == true &&
                    (version.Major != SfaServer.Version.Major ||
                    version.Minor != SfaServer.Version.Minor))
                {
                    request.SendResponce(new JObject
                    {
                        ["auth_success"] = false,
                        ["reason"] = "bad_version"
                    }.ToJsonString(), SfaServerAction.Auth);

                    return;
                }

                if ("restore_session".Equals(action, comparsion) == true)
                {
                    if ((string)authData["auth"] is string lastAuth &&
                        Server.GetClient(lastAuth) is SfaServerClient currentClient)
                    {
                        if (currentClient.IsConnected == false)
                        {
                            currentClient.TravelToClient(this);
                            Server?.RemoveClient(currentClient);
                            Server?.RegisterPlayer(this);
                            SendSuccessAuth();
                        }
                        else
                        {
                            request.SendResponce(new JObject
                            {
                                ["auth_success"] = false,
                                ["reason"] = "client_already_connected"
                            }.ToJsonString(), SfaServerAction.Auth);
                        }
                    }
                    else
                    {
                        request.SendResponce(new JObject
                        {
                            ["auth_success"] = false,
                            ["reason"] = "auth_not_found"
                        }.ToJsonString(), SfaServerAction.Auth);
                    }
                }
                else if ("server_auth".Equals(action, comparsion) == true)
                {
                    if (Server?.CheckPassword((string)authData["password"]) == true)
                    {
                        Name = (string)authData["profile_name"] ?? "RenamedUser";
                        UniqueName = Server?.CreateUnicuePlayerName(Name);
                        ProfileId = (Guid?)authData["profile_id"] ?? Guid.Empty;
                        IsPlayer = true;
                        DiscoveryClient ??= new DiscoveryClient(this);
                        Server?.RegisterPlayer(this);
                        SendSuccessAuth();
                    }
                    else
                    {
                        request.SendResponce(new JObject
                        {
                            ["auth_success"] = false,
                            ["reason"] = "bad_password"
                        }.ToJsonString(), SfaServerAction.Auth);
                    }
                }
                else
                {
                    request.SendResponce(new JObject
                    {
                        ["auth_success"] = false,
                        ["reason"] = "unexpected_action"
                    }.ToJsonString(), SfaServerAction.Auth);
                }
            }

            void SendSuccessAuth()
            {
                request.SendResponce(new JObject
                {
                    ["auth_success"] = true,
                    ["auth"] = Auth ??= Guid.NewGuid().ToString("N"),
                    ["realm_id"] = Server.Realm.Id,
                    ["realm_name"] = Server.Realm.Name,
                    ["realm_description"] = Server.Realm.Description,
                    ["galaxy_hash"] = Server.Realm.GalaxyMapHash,
                    ["mobs_map_hash"] = Server.Realm.MobsMap?.Hash,
                    ["asteroids_map_hash"] = Server.Realm.RichAsteroidsMap?.Hash,
                    ["shops_hash"] = Server.Realm.ShopsMap?.Hash,
                }.ToJsonString(), SfaServerAction.Auth);
            }
        }

        private void ProcessGetServerInfo(SfaClientRequest request)
        {
            var realm = Server.Realm;

            if (realm is null)
                return;

            request.SendResponce(new JObject
            {
                ["version"] = SfaServer.Version.ToString(3),
                ["realm_id"] = realm.Id,
                ["realm_name"] = realm.Name,
                ["realm_description"] = realm.Description,
                ["need_password"] = string.IsNullOrEmpty(Server?.Password) == false,
            }.ToJsonString(), SfaServerAction.GetServerInfo);
        }

        private void ProcessLoadGalaxyMap(SfaClientRequest request)
        {
            var output = new MemoryStream();

            try
            {
                string mapCache;

                if (Server.Realm.GalaxyMapCache is null)
                {
                    mapCache = JsonHelpers.SerializeUnbuffered(Server.Realm.GalaxyMap);
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                }
                else
                {
                    mapCache = Server.Realm.GalaxyMapCache;
                }

                var data = Encoding.UTF8.GetBytes(mapCache);
                using var deflate = new DeflateStream(output, CompressionMode.Compress, true);
                deflate.Write(data, 0, data.Length);
                deflate.Flush();
            }
            catch { }

            request.SendResponce(new Memory<byte>(output.GetBuffer(), 0, (int)output.Length), SfaServerAction.LoadGalaxyMap);
        }

        private void ProcessLoadVariableMap(SfaClientRequest request)
        {
            request.SendResponce("{}", SfaServerAction.LoadVariableMap);
        }

        public void ProcessRegisterPlayer(JNode doc, SfaClientRequest request)
        {
            request.SendResponce(new JObject
            {
                ["unique_id"] = PlayerId,
                ["unique_name"] = UniqueName,
                ["index_space"] = IndexSpace,
                ["chars"] = HandleNewChars(doc?["chars"]?.AsArraySelf()),
                ["seasons"] = JsonHelpers.ParseNodeUnbuffered(Server?.Realm?.Seasons ?? new()),
                ["bg_shop"] = JsonHelpers.ParseNodeUnbuffered(Server?.Realm?.BGShop ?? new()),
            }.ToJsonString(), SfaServerAction.RegisterPlayer);

            State = SfaClientState.PendingGame;
        }

        public void ProcessRegisterNewChars(JNode doc, SfaClientRequest request)
        {
            request.SendResponce(new JObject
            {
                ["chars"] = HandleNewChars(doc?["chars"]?.AsArraySelf()),
            }.ToJsonString(), SfaServerAction.RegisterNewCharacters);
        }

        public void ProcessDeleteChar(JNode doc)
        {
            if (doc is JObject && (int)doc["char_id"] is int id)
            {
                if (Server?.GetCharacter(id) is ServerCharacter character)
                {
                    character.Party?.RemoveMember(character.Id);
                    Server.UseClients(_ => Server.Characters.RemoveId(character.UniqueId));
                    DiscoveryClient?.Characters?.Remove(character);
                }
            }
        }


        private void ProcessSyncCharacterCurrencies(JNode doc)
        {
            if (doc is JObject && 
                (int)doc["char_id"] is int id &&
                DiscoveryClient?.Characters?.FirstOrDefault(c => c?.UniqueId == id) is ServerCharacter character)
            {
                character.ProcessSyncCharacterCurrencies(doc);
            }
        }

        private void ProcessSyncCharacterNewResearch(JNode doc)
        {

            if (doc is JObject &&
                (int)doc["char_id"] is int id &&
                DiscoveryClient?.Characters?.FirstOrDefault(c => c?.UniqueId == id) is ServerCharacter character)
            {
                character.ProcessSyncCharacterNewResearch(doc);
            }
        }

        private void ProcessSyncRankedFleets(JNode doc)
        {
            RankedFleets = JsonHelpers.DeserializeUnbuffered<List<RankedFleetInfo>>(
                doc["fleets"] ?? new JArray()) ?? new();
        }

        public JNode HandleNewChars(JArray chars)
        {
            var doc = new JArray();

            if (chars is not JArray)
                return doc;

            if (DiscoveryClient is not null)
            {
                foreach (var item in chars)
                {
                    if ((int?)item?["id"] is int id)
                    {
                        var character = DiscoveryClient.Characters.FirstOrDefault(c => c.Id == id);

                        if (character is null)
                        {
                            character = new ServerCharacter()
                            {
                                DiscoveryClient = DiscoveryClient,
                                Id = (int?)item["id"] ?? -1,
                                Name = (string)item["name"] ?? "RenamedCharacter",
                                Faction = (Faction?)(byte?)item["faction"] ?? Faction.None
                            };

                            DiscoveryClient.Characters.Add(character);
                            Server.RegisterCharacter(character);
                        }
                        else
                        {
                            character.Faction = (Faction?)(byte?)item["faction"] ?? Faction.None;
                        }

                        doc.Add(new JObject
                        {
                            ["id"] = character.Id,
                            ["unique_id"] = character.UniqueId,
                            ["unique_name"] = character.UniqueName,
                            ["index_space"] = character.UniqueId * 2000,
                        });
                    }
                }
            }

            return doc;
        }

        public void SendToChat(string channel, string sender, string msg, bool isPrivate = false)
        {
            Send(new JsonObject
            {
                ["channel"] = channel,
                ["sender"] = sender,
                ["msg"] = msg,
                ["is_private"] = isPrivate,
            }, SfaServerAction.Chat);
        }

        private void ProcessRegisterChannel(JNode doc)
        {
            var channelName = (string)doc?["name"];

            if ("UserFriends".Equals(channelName, StringComparison.InvariantCultureIgnoreCase) == true)
                SendServerPlayerStatuses();
        }

        public void SendStartBattle(
            string gameMode,
            string address, int port, string auth,
            int systemId = 0, int charId = 0)
        {
            Send(new JObject
            {
                ["game_mode"] = gameMode,
                ["address"] = address,
                ["port"] = port,
                ["auth"] = auth,
                ["system_id"] = systemId,
                ["char_id"] = charId,
            }, SfaServerAction.StartBattle);
        }

        public void TravelToClient(SfaServerClient client)
        {
            if (client is null || client == this)
                return;

            client.Name = Name;
            client.UniqueName = UniqueName;
            client.Auth = Auth;
            client.PlayerId = PlayerId;
            client.ProfileId = ProfileId;
            client.IsPlayer = IsPlayer;
            client.Server = Server;
            client.Galaxy = Galaxy;
            client.DiscoveryClient = DiscoveryClient;

            DiscoveryClient?.TravelToClient(client);

            Server = null;
            Galaxy = null;
            DiscoveryClient = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Server = null;
            Galaxy = null;
            CurrentCharacter?.Fleet?.RemoveListener(DiscoveryClient);
            DiscoveryClient?.Dispose();
            DiscoveryClient = null;
        }
    }
}
