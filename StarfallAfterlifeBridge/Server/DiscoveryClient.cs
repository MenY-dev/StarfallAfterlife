using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient : IDisposable
    {
        public SfaServerClient Client { get; protected set; }

        public SfaCharacterState State { get; set; }

        public bool IsPlayer => Client.IsPlayer;

        public SfaServer Server => Client.Server;

        public DiscoveryGalaxy Galaxy => Server.Galaxy;

        public GalaxyMap Map => Galaxy?.Map;

        public List<ServerCharacter> Characters { get; } = new();

        public ServerCharacter CurrentCharacter { get; set; }

        protected ActionBuffer ActionBuffer { get; set; } = new();

        public DiscoveryClient(SfaServerClient client)
        {
            Client = client;
        }

        public void Invoke(Action action) => ActionBuffer?.Invoke(action);

        public void Invoke(Action<DiscoveryClient> action) => ActionBuffer?.Invoke(() => action?.Invoke(this));

        public void ProcessCharacterSelect(JsonNode authData, SfaClientRequest request)
        {
            if ((int?)authData?["unique_id"] is int id)
            {
                if (Characters.FirstOrDefault(c => c.UniqueId == id) is ServerCharacter character)
                {
                    CurrentCharacter = character;
                    var progress = new CharacterProgress();
                    progress.LoadFromJson(authData["progress"]);
                    character.LoadProgress(progress);
                }
            }

            request.SendResponce(
                new JsonObject{}.ToJsonString(),
                SfaServerAction.SyncCharacterSelect);

            Client.State = SfaClientState.InDiscoveryMod;
        }

        public void EnterToGalaxy()
        {
            State = SfaCharacterState.EnterToGalaxy;

            if (CurrentCharacter is ServerCharacter character)
            {
                if (character.Fleet is UserFleet fleet &&
                    fleet.State is FleetState.None or FleetState.InBattle)
                {
                    Galaxy?.BeginPreUpdateAction(g =>
                    {
                        fleet.State = FleetState.InGalaxy;
                        Invoke(() => EnterToStarSystem(character.Fleet?.System?.Id ?? GetCharactDefaultSystem()?.Id ?? 0));
                    });
                }
                else
                {
                    Client.SendRequest(SfaServerAction.StartSession, new JsonObject
                    {
                        ["character_id"] = CurrentCharacter.UniqueId
                    }
                    ).ContinueWith(t =>
                    {
                        if (t.Result is SfaClientResponse response &&
                            response.IsSuccess == true &&
                            response.Action == SfaServerAction.StartSession)
                        {
                            ProcessGalaxyEntryData(JsonHelpers.ParseNodeUnbuffered(response.Text));
                        }
                    });
                }
            }
        }

        public void EndGalaxySession()
        {
            if (State != SfaCharacterState.InGalaxy)
                return;

            SendDiscoverySessionEnded(CurrentCharacter?.Fleet);
            FinishGalaxySession();
        }

        public void DropGalaxySession()
        {
            SendSessionDropDone();
            FinishGalaxySession();
        }

        public void FinishGalaxySession()
        {
            SynckSessionFleetInfo();
            State = SfaCharacterState.InShipyard;

            if (CurrentCharacter is ServerCharacter character)
            {
                character.InGalaxy = false;

                if (character.Fleet is UserFleet fleet)
                {
                    fleet.Listeners?.Clear();
                    character.Fleet = null;
                    Galaxy?.BeginPreUpdateAction(g => fleet.LeaveFromGalaxy());
                }
            }
        }

        public void SendStartBattle(
            string gameMode,
            string address, int port, string auth,
            int systemId = 0, int charId = 0)
        {
            Client.Send(new JsonObject
            {
                ["game_mode"] = gameMode,
                ["address"] = address,
                ["port"] = port,
                ["auth"] = auth,
                ["system_id"] = systemId,
                ["char_id"] = charId,
            }, SfaServerAction.StartBattle);
        }

        public void ProcessGalaxyEntryData(JsonNode doc)
        {
            if (doc is null)
                return;

            if (CurrentCharacter is ServerCharacter character &&
                doc["char_data"] is JsonObject charData)
            {
                if (character.Fleet is UserFleet currentFleet)
                {
                    currentFleet.RemoveListener(this);
                    character.Fleet = null;
                    Galaxy?.BeginPreUpdateAction(g => currentFleet.LeaveFromGalaxy());
                }

                character.LoadFromCharacterData(charData);

                if (doc["active_session"]?["ships"] is JsonArray ships)
                    character.LoadActiveShips(ships);

                SynckSessionFleetInfo();

                character.CreateNewFleet();
                character.Fleet?.AddListener(this);
            }

            if (State == SfaCharacterState.EnterToGalaxy)
            {
                if (doc["active_session"] is JsonObject session &&
                    (int?)session["system"] is int systemId &&
                    session["location"]?.Deserialize<Vector2>() is Vector2 location &&
                    Galaxy?.Map?.GetSystem(systemId) is not null)
                {
                    EnterToStarSystem(systemId, location);
                }
                else
                {
                    var spawnSystem = GetCharactDefaultSystem() ?? new();
                    var spawnPos = spawnSystem.GetDefaultSpawnPosition(CurrentCharacter?.Faction ?? Faction.None);

                    EnterToStarSystem(spawnSystem.Id, spawnPos);
                }
            }
        }

        private void ProcessGetFullGalaxySesionData(JsonNode doc, SfaClientRequest request)
        {
            var response = new JsonObject();

            if (
                CurrentCharacter is ServerCharacter character &&
                character.InGalaxy == true &&
                character.Fleet?.System is not null)
            {
                response["system"] = character.Fleet?.System?.Id ?? 0;
                response["location"] = JsonHelpers.ParseNode(character.Fleet?.Location ?? Vector2.Zero);
                response["active_ships"] = JsonHelpers.ParseNodeUnbuffered(CurrentCharacter?.Ships ?? new());
            }

            request.SendResponce(response, SfaServerAction.GetFullGalaxySessionData);
        }

        public Task<ShipConstructionInfo> RequestShipData(int shipId)
        {
            return Client?.SendRequest(
                SfaServerAction.RequestCharacterShipData,
                new JsonObject { ["ship_id"] = shipId },
                5000)
                .ContinueWith<ShipConstructionInfo>(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        JsonHelpers.ParseNodeUnbuffered(response.Text) is JsonObject doc &&
                        doc["data"]?.DeserializeUnbuffered<ShipConstructionInfo>() is ShipConstructionInfo data)
                    {
                        return data;
                    }

                    return null;
                });
        }

        public Task<JsonNode> RequestDiscoveryCharacterData()
        {
            return Client?.SendRequest(
                SfaServerAction.RequestCharacterDiscoveryData,
                new JsonObject {},
                5000)
                .ContinueWith<JsonNode>(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        JsonHelpers.ParseNodeUnbuffered(response.Text) is JsonNode doc)
                    {
                        return doc;
                    }

                    return null;
                });
        }

        public void EnterToStarSystem(int system, Vector2? location = null)
        {
            var character = CurrentCharacter;

            if (character is null)
                return;

            if (character.Fleet is null)
                character.CreateNewFleet();

            State = SfaCharacterState.InGalaxy;
            Galaxy?.BeginPreUpdateAction(g => g.EnterToStarSystem(system, character.Fleet, location));
        }

        public GalaxyMapStarSystem GetCharactDefaultSystem()
        {
            return Galaxy?.Map?.GetStartingSystem(CurrentCharacter?.Faction ?? Faction.None);
        }

        public int GetMainShipHull()
        {
            return CurrentCharacter?.Ships.LastOrDefault()?.Hull ?? 0;
        }

        public void SynckSessionSystemInfo(int systemId, Vector2 location)
        {
            Client?.Send(new JsonObject
            {
                ["system"] = systemId,
                ["location"] = JsonHelpers.ParseNode(location)
            }, SfaServerAction.SyncGalaxySessionData);
        }

        public void SynckSessionFleetInfo()
        {
            Client?.Send(new JsonObject
            {
                ["ships"] = JsonHelpers.ParseNodeUnbuffered(CurrentCharacter?.Ships ?? new())
            }, SfaServerAction.SyncGalaxySessionData);
        }

        public void SendAddCharacterCurrencies(
            int charId,
            int? igc = null,
            int? bgc = null,
            int? xp = null,
            Dictionary<int, int> shipsXp = null)
        {
            var doc = new JsonObject()
            {
                ["char_id"] = charId,
            };

            if (igc is not null)
                doc["igs"] = igc;

            if (bgc is not null)
                doc["bgc"] = bgc;

            if (xp is not null)
                doc["xp"] = xp;

            if (shipsXp is not null)
                doc["ships_xp"] = JsonHelpers.ParseNodeUnbuffered(shipsXp);

            Client?.Send(doc, SfaServerAction.AddCharacterCurrencies);
        }

        public virtual void SendDiscoveryMessage(int systemId, DiscoveryObjectType objectType, int objectId, DiscoveryServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToDiscoveryChannel(writer =>
            {
                writer.WriteInt32(systemId);
                writer.WriteByte((byte)objectType);
                writer.WriteInt32(objectId);
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public virtual void SendDiscoveryMessage(StarSystemObject sender, DiscoveryServerAction action, Action<SfWriter> writeAction = null)
        {
            if (sender is null || sender.Id < 0 || sender.System is null || sender.System.Id < 0)
                return;

            SendDiscoveryMessage(
                sender.System.Id,
                sender.Type,
                sender.Id,
                action,
                writer => writeAction?.Invoke(writer));
        }

        public virtual void SendGalaxyMessage(DiscoveryServerGalaxyAction action, Action<SfWriter> writeAction = null)
        {
            SendToGalacticChannel(writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public virtual void SendBattleGroundMessage(BattleGroundServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToBattleGroundChannel(writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public virtual void SendToDiscoveryChannel(Action<SfWriter> writeAction)
        {
            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Client?.Send(writer.ToArray(), SfaServerAction.DiscoveryChannel);
        }

        public virtual void SendToGalacticChannel(Action<SfWriter> writeAction)
        {
            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Client?.Send(writer.ToArray(), SfaServerAction.GalacticChannel);
        }

        public virtual void SendToBattleGroundChannel(Action<SfWriter> writeAction)
        {
            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Client?.Send(writer.ToArray(), SfaServerAction.BattleGroundChannel);
        }

        public void Dispose()
        {
            Client = null;
            Characters?.Clear();
            CurrentCharacter = null;
            CurrentCharacter?.Fleet?.RemoveListener(this);
            ActionBuffer?.Dispose();
            ActionBuffer = null;
        }
    }
}
