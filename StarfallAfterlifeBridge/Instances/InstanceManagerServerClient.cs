using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Instances
{
    public partial class InstanceManagerServerClient : InstanceManagerClientBase
    {
        public InstanceManager Manager {  get; set; }

        protected Dictionary<string, SfaInstance> Instances { get; } = new();

        public MgrServer MgrServer { get; protected set; }

        public InstanceChannelManager GalaxyMgrChannelManager { get; protected set; }

        public event EventHandler<CharacterDataResponseEventArgs> CharacterDataReceived;

        public event EventHandler<MobDataResponseEventArgs> MobDataReceived;

        public event EventHandler<SpecialFleetResponseEventArgs> SpecialFleetReceived;

        public event EventHandler<RewardForEvenResponseEventArgs> RewardForEvenReceived;

        protected object Lockher { get; } = new();

        protected override void OnReceive(string msgType, JsonNode doc)
        {
            base.OnReceive(msgType, doc);

            if (doc is null)
                return;

            switch (msgType)
            {
                case "start_instance":
                    HandleStartInstance(doc); break;

                case "stop_instance":
                    HandleStopInstance(doc); break;

                case "join_new_char":
                    HandleJoinNewChar(doc); break;

                case "send_char_data":
                    HandleCharDataResponse(doc); break;

                case "send_mob_data":
                    HandleMobDataResponse(doc); break;

                case "send_special_fleet":
                    HandleSpecialFleetResponse(doc); break;

                case "send_reward_for_even":
                    HandleRewardForEven(doc); break;

            }
        }

        protected virtual void HandleStartInstance(JsonNode doc)
        {
            lock (Lockher)
            {
                var syncKey = (string)doc?["sync_key"];

                if (syncKey is not null &&
                    GetInstanceWithSyncKey(syncKey) is null &&
                    JsonHelpers.DeserializeUnbuffered<InstanceInfo>((string)doc["info"]) is InstanceInfo info)
                {
                    try
                    {
                        var instance = Manager.CreateNewInstance(this, info);
                        Instances.Add(syncKey, instance);
                        instance.Start();
                    }
                    catch  { }
                }
            }
        }

        private void HandleStopInstance(JsonNode doc)
        {
            lock (Lockher)
            {
                if ((string)doc?["sync_key"] is string syncKey &&
                    GetInstanceWithSyncKey(syncKey) is DiscoveryBattleInstance instance)
                {
                    instance.Stop();
                    Instances.Remove(syncKey);
                }
            }
        }

        private void HandleJoinNewChar(JsonNode doc)
        {
            lock (Lockher)
            {
                if (doc is not null &&
                    GetInstanceWithSyncKey((string)doc["sync_key"]) is DiscoveryBattleInstance instance &&
                    (string)doc["char_data"] is string data &&
                    JsonHelpers.DeserializeUnbuffered<InstanceCharacter>(data) is InstanceCharacter character)
                {
                    instance.DiscoveryChannel?.SendNewPlayerJoiningToInstance(character);
                }
            }
        }

        public virtual void SendInstanceState(SfaInstance instance)
        {
            if (instance is null)
                return;

            lock (Lockher)
            {
                var state = instance.State;
                var doc = new JsonObject
                {
                    ["sync_key"] = GetSyncKey(instance),
                    ["state"] = (byte)state
                };


                if (state == InstanceState.Created)
                {
                    doc["auth"] = instance.Auth;
                }
                else if (state == InstanceState.Started)
                {
                    doc["address"] = instance.Address;
                    doc["port"] = instance.Port;
                    doc["auth"] = instance.Auth;
                }

                Send("instance_state_changed", doc);
            }
        }

        public virtual void SendFleetLeaves(SfaInstance instance, DiscoveryObjectType fleetType, int fleetId, SystemHex hex)
        {
            if (instance is null)
                return;

            lock (Lockher)
            {
                var doc = new JsonObject
                {
                    ["sync_key"] = GetSyncKey(instance),
                    ["fleet_type"] = (byte)fleetType,
                    ["fleet_id"] = fleetId,
                    ["fleet_hex_x"] = hex.X,
                    ["fleet_hex_y"] = hex.Y,
                };

                Send("fleet_leaves", doc);
            }
        }

        public void SendInstanceAuthReady(SfaInstance instance, int charId, string auth)
        {

            if (instance is null)
                return;

            lock (Lockher)
            {
                Send("instance_auth_ready", new JsonObject
                {
                    ["sync_key"] = GetSyncKey(instance),
                    ["char_id"] = charId,
                    ["instance_auth"] = auth,
                });
            }
        }

        public virtual void RequestCharacterData(int characterId, string gameMode = null, bool includeDestroyedShips = false)
        {
            Send("get_char_data", new JsonObject
            {
                ["char_id"] = characterId,
                ["game_mode"] = gameMode,
                ["include_destroyed_ships"] = includeDestroyedShips,
            });
        }

        protected virtual void HandleCharDataResponse(JsonNode doc)
        {
            if (doc is not null &&
                (int?)doc["char_id"] is int id &&
                (string)doc["char_data"] is string data)
            {
                var characters = Instances.Values
                    .SelectMany(i => i.Characters ?? Enumerable.Empty<InstanceCharacter>());

                foreach (var item in characters)
                {
                    if (item?.Id == id)
                        item.DiscoveryData = data;
                }

                CharacterDataReceived?.Invoke(this, new(id, data));
            }
        }

        public virtual void RequestMobData(int mobId, string auth)
        {
            Send("get_mob_data", new JsonObject
            {
                ["auth"] = auth,
                ["mob_id"] = mobId,
            });
        }

        public virtual void RequestCustomMobData(int mobId, Faction faction, string[] tags, string auth)
        {
            Send("get_mob_data", new JsonObject
            {
                ["auth"] = auth,
                ["mob_id"] = mobId,
                ["custom"] = 1,
                ["faction"] = (int)faction,
                ["tags"] = JsonHelpers.ParseNodeUnbuffered(tags ?? Array.Empty<string>()),
            });
        }

        protected virtual void HandleMobDataResponse(JsonNode doc)
        {
            if (doc is not null &&
                (string)doc["auth"] is string auth &&
                (int?)doc["mob_id"] is int id &&
                (string) doc["mob_data"] is string data)
            {
                MobDataReceived?.Invoke(this, new(auth, id, JsonHelpers.ParseNodeUnbuffered(data)));
            }
        }

        private void RequestSpecialFleet(string fleetName, string auth)
        {
            Send("get_special_fleet", new JsonObject
            {
                ["auth"] = auth,
                ["name"] = fleetName,
            });
        }

        protected virtual void HandleSpecialFleetResponse(JsonNode doc)
        {
            if (doc is not null &&
                (string)doc["name"] is string name &&
                (string)doc["auth"] is string auth)
            {
                SpecialFleetReceived?.Invoke(this, new(auth, name, JsonHelpers.ParseNodeUnbuffered((string)doc["data"])));
            }
        }

        private void RequestRewardForEven(string auth)
        {
            Send("get_reward_for_even", new JsonObject
            {
                ["auth"] = auth,
            });
        }

        private void HandleRewardForEven(JsonNode doc)
        {
            if (doc is not null &&
                (string)doc["reward"] is string reward &&
                (string)doc["auth"] is string auth)
            {
                RewardForEvenReceived?.Invoke(this, new(auth, reward));
            }
        }

        public virtual void UpdateShipStatus(int shipId, string shipData, string shipStats, string auth)
        {
            if (shipId > -1 &&
                string.IsNullOrWhiteSpace(shipData) == false &&
                GetSyncKey(auth) is string syncKey)
            {
                Send("update_ship_status", new JsonObject
                {
                    ["sync_key"] = syncKey,
                    ["ship"] = new JsonObject
                    {
                        ["id"] = shipId,
                        ["data"] = shipData,
                        ["stats"] = shipStats
                    }
                });
            }
        }

        public void SendAddCharacterShipsXp(int charId, Dictionary<int, int> shipsXp)
        {
            if (charId > -1 &&
                shipsXp is not null)
            {
                Send("add_char_ships_xp", new JsonObject
                {
                    ["char_id"] = charId,
                    ["ships_xp"] = JsonHelpers.ParseNodeUnbuffered(shipsXp),
                });
            }
        }

        public void SendInstanceAction(string instanceAuth, string type, string data)
        {
            if (GetSyncKey(instanceAuth) is string syncKey)
            {
                Send("new_instance_action", new JsonObject
                {
                    ["sync_key"] = syncKey,
                    ["action"] = type,
                    ["data"] = data,
                });
            }
        }

        public SfaInstance GetInstance(string auth)
        {
            lock (Lockher)
                return Instances.Values.FirstOrDefault(i => i.Auth == auth);
        }

        public SfaInstance GetInstance(int id)
        {
            lock (Lockher)
                return Instances.Values.FirstOrDefault(i => i.InstanceId == id);
        }

        protected SfaInstance GetInstanceWithSyncKey(string syncKey)
        {
            lock (Lockher)
            {
                if (string.IsNullOrWhiteSpace(syncKey) == false &&
                    Instances.TryGetValue(syncKey, out var instance) == true)
                    return instance;

                return null;
            }
        }

        protected string GetSyncKey(SfaInstance instance)
        {
            lock (Lockher)
                return Instances.FirstOrDefault(i => i.Value == instance).Key;
        }

        protected string GetSyncKey(string auth)
        {
            lock (Lockher)
                return Instances.FirstOrDefault(i => i.Value.Auth == auth).Key;
        }

        protected string GetSyncKey(int id)
        {
            lock (Lockher)
                return Instances.FirstOrDefault(i => i.Value.InstanceId == id).Key;
        }

        internal void Init(InstanceManager manager)
        {
            Manager = manager;
            MgrServer ??= new MgrServer(GalaxyInput);
            MgrServer.Start(new Uri("http://127.0.0.1:0/instancemgr/"));
            GalaxyMgrChannelManager ??= new InstanceChannelManager(this);
            GalaxyMgrChannelManager.Start(new Uri("tcp://127.0.0.1:0"));
        }
    }
}
