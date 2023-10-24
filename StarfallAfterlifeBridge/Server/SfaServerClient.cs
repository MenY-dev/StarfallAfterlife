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

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient : SfaClientBase
    {
        public string Name { get; protected set; }

        public string Auth { get; protected set; }

        public int PlayerId { get; set; } = -1;

        public int IndexSpace => PlayerId < 0 ? 0 : PlayerId * 2000;

        public Guid ProfileId { get; protected set; }

        public SfaClientState State { get; set; } = SfaClientState.PendingLogin;

        public bool IsPlayer { get; protected set; }

        public SfaServer Server { get; set; }

        public DiscoveryGalaxy Galaxy { get; set; }

        public GalaxyMap Map => Server.Realm.GalaxyMap;

        public ServerCharacter CurrentCharacter { get; set; }

        public DiscoveryClient DiscoveryClient { get; protected set; }

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

                default:
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

                case SfaServerAction.LoadGalaxyMap:
                    ProcessLoadGalaxyMap(request);
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
            Name = (string)authData["profile_name"] ?? "RenamedUser";
            ProfileId = (Guid?)authData["profile_id"] ?? Guid.Empty;
            IsPlayer = true;

            DiscoveryClient ??= new DiscoveryClient(this);

            request.SendResponce(new JObject
            {
                ["auth_success"] = true,
                ["auth"] = Auth ??= Guid.NewGuid().ToString("N"),
                ["realm_id"] = Server.Realm.Id,
                ["galaxy_hash"] = Server.Realm.GalaxyMapHash,
                ["mobs_map_hash"] = Server.Realm.MobsMap?.Hash,
                ["asteroids_map_hash"] = Server.Realm.RichAsteroidsMap?.Hash,
                ["shops_hash"] = Server.Realm.ShopsMap?.Hash,
            }.ToJsonString(), SfaServerAction.Auth);
        }

        private void ProcessLoadGalaxyMap(SfaClientRequest request)
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

            var doc = new JObject
            {
                ["hash"] = Server.Realm.GalaxyMap.Hash,
                ["map"] = mapCache,
            };

            request.SendResponce(doc, SfaServerAction.LoadGalaxyMap);
        }

        public void ProcessRegisterPlayer(JNode doc, SfaClientRequest request)
        {
            DiscoveryClient.Characters?.Clear();

            request.SendResponce(new JObject
            {
                ["chars"] = HandleNewChars(doc?["chars"]?.AsArraySelf()),
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
                DiscoveryClient?.Characters?.RemoveAll(c => c.UniqueId == id);
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

        public JNode HandleNewChars(JArray chars)
        {
            var doc = new JArray();

            if (chars is null)
                return doc;

            if (DiscoveryClient is not null)
            {
                foreach (var item in chars)
                {
                    if ((int?)item?["id"] is int id &&
                        DiscoveryClient.Characters.Any(c => c.Id == id) == false)
                    {
                        var character = new ServerCharacter()
                        {
                            DiscoveryClient = DiscoveryClient,
                            Id = (int?)item["id"] ?? -1,
                            Name = (string)item["name"] ?? "RenamedCharacter",
                            Faction = (Faction?)(byte?)item["faction"] ?? Faction.None

                        };

                        DiscoveryClient.Characters.Add(character);
                        Server.RegisterCharacter(character);

                        doc.Add(new JObject
                        {
                            ["id"] = character.Id,
                            ["unique_id"] = character.UniqueId,
                            ["unique_name"] = character.UniqueName,
                        });
                    }
                }
            }

            return doc;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Server = null;
            Galaxy = null;
            CurrentCharacter?.Fleet?.RemoveListener(DiscoveryClient);
            CurrentCharacter = null;
            DiscoveryClient?.Dispose();
            DiscoveryClient = null;
        }
    }
}
