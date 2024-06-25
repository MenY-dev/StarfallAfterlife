using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Server.Matchmakers;
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

        public DiscoveryClient(SfaServerClient client)
        {
            Client = client;
        }

        public void Invoke(Action action) => Client?.Invoke(action);

        public void Invoke(Action<DiscoveryClient> action) => Client?.Invoke(() => action?.Invoke(this));

        public void ProcessCharacterSelect(JsonNode authData, SfaClientRequest request)
        {
            if ((int?)authData?["unique_id"] is int id)
            {
                if (Characters.FirstOrDefault(c => c.UniqueId == id) is ServerCharacter character)
                {
                    CurrentCharacter?.SetOnlineStatus(false);
                    Server.ProcessNewUserStatus(Client, UserInGameStatus.None, true);
                    CurrentCharacter = character;
                    var progress = new CharacterProgress();
                    progress.LoadFromJson(authData["progress"]);
                    character.LoadProgress(progress);
                    character.Level = (int?)authData?["lvl"] ?? 0;
                    character.AccessLevel = (int?)authData?["access_lvl"] ?? 0;
                    character.HouseTag = character.GetHouse()?.Tag;
                    character.UpdateFleetInfo();
                    character.UpdateQuestLines();
                    character.UpdateDailyQuests();
                    character.SyncDoctrines();
                }
            }

            request.SendResponce(
                new JsonObject{}.ToJsonString(),
                SfaServerAction.SyncCharacterSelect);

            Client.State = SfaClientState.InDiscoveryMod;
            CurrentCharacter?.SetOnlineStatus(true);
            UpdateHouseMemberInfo();
        }

        public void EnterToGalaxy()
        {
            State = SfaCharacterState.EnterToGalaxy;

            if (CurrentCharacter is ServerCharacter character)
            {
                if (character.Fleet is UserFleet fleet &&
                    fleet.State is not FleetState.None)
                {
                    foreach (var item in character.CustomInstances.ToArray().Where(i => i.Type == 3))
                        character.RemoveCustomInstance(item.Id);

                    Galaxy?.BeginPreUpdateAction(g =>
                    {
                        fleet.IsIsolated = false;
                        fleet.SetFleetState(FleetState.InGalaxy);
                        Invoke(() => EnterToStarSystem(character.Fleet?.System?.Id ?? GetCharactDefaultSystem()?.Id ?? 0));
                    });
                }
                else
                {
                    character.CustomInstances.Clear();

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

        public void StartGhostSession()
        {
            State = SfaCharacterState.EnterToGalaxy;

            if (CurrentCharacter is ServerCharacter character)
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
                        var doc = JsonHelpers.ParseNodeUnbuffered(response.Text);

                        if (doc is not JsonObject)
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

                            var spawnSystem = GetCharactDefaultSystem() ?? new();
                            var spawnPos = spawnSystem.GetDefaultSpawnPosition(CurrentCharacter?.Faction ?? Faction.None);
                            character.LoadFromCharacterData(charData);

                            SyncSessionFleetInfo();
                            SyncSessionSystemInfo(spawnSystem.Id, spawnPos);
                        }
                    }
                });
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
            SyncSessionFleetInfo();
            State = SfaCharacterState.InShipyard;

            if (CurrentCharacter is ServerCharacter character)
            {
                if (Client is SfaServerClient client)
                {
                    client.CurrentSystemId = -1;
                    client.CurrentSystemName = null;
                    Server.ProcessNewUserStatus(Client, UserInGameStatus.CharMainMenu, true);
                }

                character.Party?.SetMemberStarSystem(character.UniqueId, -1);

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
            Client?.SendStartBattle(gameMode, address, port, auth, systemId, charId);
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
                character.HouseTag = character.GetHouse()?.Tag;

                if (doc["active_session"]?["ships"] is JsonArray ships)
                    character.LoadActiveShips(ships);

                SyncSessionFleetInfo();

                character.CreateNewFleet();
                character.Fleet?.AddListener(this);
                character.SetOnlineStatus(true);
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

            if (CurrentCharacter is ServerCharacter character &&
                character.Fleet is DiscoveryFleet fleet &&
                fleet.System is not null &&
                fleet.State is not FleetState.Destroyed)
            {
                response["system"] = character.Fleet?.System?.Id ?? 0;
                response["location"] = JsonHelpers.ParseNode(fleet.Location);
                response["active_ships"] = JsonHelpers.ParseNodeUnbuffered(character.Ships ?? new());
            }

            request.SendResponce(response, SfaServerAction.GetFullGalaxySessionData);
            SendQuestDataUpdate();
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

        public Task<JsonNode> RequestDiscoveryCharacterData(bool allShips = true)
        {
            return Client?.SendRequest(
                SfaServerAction.RequestCharacterDiscoveryData,
                new JsonObject { ["all_ships"] = allShips ? 1 : 0 },
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
            Galaxy?.BeginPreUpdateAction(g =>
            {
                g.EnterToStarSystem(system, character.Fleet, location);
                character.Fleet?.AddEffect(new() { Duration = 5, Logic = GameplayEffectType.Immortal });
            });

            character.Party?.SetMemberStarSystem(character.UniqueId, system);
            character.SyncDoctrines();

            if (character.Fleet is UserFleet fleet &&
                fleet.System is not null &&
                Server.Matchmaker is SfaMatchmaker matchmaker)
            {
                Galaxy?.BeginPreUpdateAction(g =>
                {
                    var systemBattle = fleet.GetBattle();

                    Invoke(c =>
                    {
                        if (systemBattle is not null &&
                            matchmaker.GetBattle(systemBattle) is DiscoveryBattle battle)
                        {
                            var battleChar = battle.GetCharacter(character);

                            if (battleChar is not null &&
                                battleChar.InBattle == true &&
                                battle.State != MatchmakerBattleState.Finished &&
                                systemBattle.IsPossibleToJoin == true)
                            {
                                Galaxy.BeginPreUpdateAction(g => systemBattle.AddToBattle(fleet, BattleRole.Join, Vector2.Zero));
                            }
                            else
                            {
                                battle.Leave(battleChar, systemBattle.Hex);

                                Galaxy.BeginPreUpdateAction(g =>
                                {
                                    systemBattle.Leave(
                                        fleet,
                                        systemBattle.System?.GetNearestSafeHex(fleet, systemBattle.Hex) ?? default,
                                        false);

                                    fleet.SetFleetState(FleetState.InGalaxy);
                                });
                            }
                        }
                    });
                });
            }
        }

        public GalaxyMapStarSystem GetCharactDefaultSystem()
        {
            return Galaxy?.Map?.GetStartingSystem(CurrentCharacter?.Faction ?? Faction.None);
        }

        public int GetMainShipHull()
        {
            return CurrentCharacter?.Ships.LastOrDefault()?.Hull ?? 0;
        }

        public void SyncSessionSystemInfo(int systemId, Vector2 location)
        {
            Client?.Send(new JsonObject
            {
                ["system"] = systemId,
                ["location"] = JsonHelpers.ParseNode(location)
            }, SfaServerAction.SyncGalaxySessionData);
        }

        public void SyncSessionFleetInfo()
        {
            Client?.Send(new JsonObject
            {
                ["ships"] = JsonHelpers.ParseNodeUnbuffered(CurrentCharacter?.Ships ?? new())
            }, SfaServerAction.SyncGalaxySessionData);
        }

        public void SyncShipDestroyed(params int[] ships)
        {
            Client?.Send(new JsonObject
            {
                ["destroyed_ships"] = JsonHelpers.ParseNodeUnbuffered(ships)
            }, SfaServerAction.SyncGalaxySessionData);
        }

        public void AddNewCharacterStats(Dictionary<string, float> stats)
        {
            Client?.Send(
                JsonHelpers.ParseNodeUnbuffered(stats ?? new()),
                SfaServerAction.AddNewCharacterStats);
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
                doc["igc"] = igc;

            if (bgc is not null)
                doc["bgc"] = bgc;

            if (xp is not null)
                doc["xp"] = xp;

            if (shipsXp is not null)
                doc["ships_xp"] = JsonHelpers.ParseNodeUnbuffered(shipsXp);

            Client?.Send(doc, SfaServerAction.AddCharacterCurrencies);
        }

        public void SendAddEffect(int effectId, double duration)
        {
            if (CurrentCharacter is ServerCharacter character)
                SendAddEffect(character.UniqueId, effectId, duration);
        }

        public void SendAddEffect(int charId, int effectId, double duration)
        {
            Client?.Send(new JsonObject()
            {
                ["char_id"] = charId,
                ["effect"] = new JsonObject
                {
                    ["id"] = effectId,
                    ["duration"] = duration,
                },
            }, SfaServerAction.AddCharacterCurrencies);
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


        public void TravelToClient(SfaServerClient client)
        {
            if (client is null)
                return;

            Client = client;
        }

        public void Dispose()
        {
            Client = null;
            Characters?.Clear();
            CurrentCharacter = null;
            CurrentCharacter?.Fleet?.RemoveListener(this);
        }
    }
}
