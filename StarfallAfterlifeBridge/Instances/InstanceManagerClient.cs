﻿using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceManagerClient : InstanceManagerClientBase
    {
        public event EventHandler<InstanceInfoEventArgs> InstanceStateChanged;

        public event EventHandler<InstanceAuthReadyEventArgs> InstanceAuthReady;

        public event EventHandler<CharacterDataRequestEventArgs> CharacterDataRequested;

        public event EventHandler<MobDataRequestEventArgs> MobDataRequested;

        public event EventHandler<SpecialFleetRequestEventArgs> SpecialFleetRequested;

        public event EventHandler<RewardForEvenRequestEventArgs> RewardForEvenRequested;

        public event EventHandler<InstanceFleetLeavesEventArgs> FleetLeaves;

        public event EventHandler<ShipStatusUpdatedEventArgs> ShipStatusUpdated;

        public event EventHandler<AddCharacterShipsXpEventArgs> AddCharacterShipsXp;

        public event EventHandler<InstanceActionEventArgs> NewInstanceAction;

        public event EventHandler<DropListRequestEventArgs> DropListRequested;

        public event EventHandler<RankedFleetRequestEventArgs> RankedFleetRequested;

        protected Dictionary<string, InstanceInfo> Instances { get; } = new();

        protected object Lockher { get; } = new();

        protected override void OnReceive(string msgType, JsonNode doc)
        {
            base.OnReceive(msgType, doc);

            if (doc is null)
                return;

            switch (msgType)
            {
                case "instance_state_changed":
                    HandleInstanceStateChanged(doc); break;

                case "instance_auth_ready":
                    HandleInstanceAuthReady(doc); break;

                case "get_char_data":
                    HandleGetCharacterData(doc); break;

                case "get_mob_data":
                    HandleGetMobData(doc); break;

                case "get_special_fleet":
                    HandleSpecialFleet(doc); break;

                case "get_drop_list":
                    HandleGetDropList(doc); break;

                case "get_reward_for_even":
                    HandleRewardForEven(doc); break;

                case "fleet_leaves":
                    HandleFleetLeaves(doc); break;

                case "update_ship_status":
                    HandleUpdateShipStatus(doc); break;

                case "add_char_ships_xp":
                    HandleAddCharacterShipsXp(doc); break;

                case "new_instance_action":
                    HandleNewInstanceAction(doc); break;

                case "get_ranked_fleet_data":
                    HandleRankedFleetData(doc); break;
            }
        }

        public virtual bool StartInstance(InstanceInfo instance)
        {
            lock (Lockher)
            {
                if (Instances.ContainsValue(instance))
                    return false;

                var syncKey = CreateSyncKey();
                Instances.Add(syncKey, instance);

                Send("start_instance", new JsonObject
                {
                    ["sync_key"] = syncKey,
                    ["info"] = JsonHelpers.ParseNodeUnbuffered(instance).ToJsonString(false),
                });
            }

            return true;
        }

        public virtual void StopInstance(InstanceInfo instance)
        {
            lock (Lockher)
            {
                if (GetSyncKey(instance) is string syncKey)
                {
                    Instances.Remove(syncKey);

                    Send("stop_instance", new JsonObject
                    {
                        ["sync_key"] = syncKey
                    });
                }
            }
        }


        public void JoinNewChar(InstanceInfo instance, InstanceCharacter character)
        {
            lock (Lockher)
            {
                if (JsonHelpers.SerializeUnbuffered(character) is string charData &&
                    GetSyncKey(instance) is string syncKey)
                {
                    Send("join_new_char", new JsonObject
                    {
                        ["sync_key"] = syncKey,
                        ["char_data"] = charData,
                    });
                }
            }
        }

        public void SendCharDropSession(InstanceInfo instance, int charId)
        {
            lock (Lockher)
            {
                if (GetSyncKey(instance) is string syncKey)
                {
                    Send("char_drop_session", new JsonObject
                    {
                        ["sync_key"] = syncKey,
                        ["char_id"] = charId,
                    });
                }
            }
        }

        public virtual void SendCharacterData(InstanceCharacter character, JsonNode data) =>
            SendCharacterData(character?.Id ?? -1, data);

        public virtual void SendCharacterData(int id, JsonNode data)
        {
            if (id < 0)
                return;

            Send("send_char_data", new JsonObject
            {
                ["char_id"] = id,
                ["char_data"] = data?.ToJsonString(false),
            });
        }

        public virtual void SendMobData(string instanceAuth, int id, JsonNode data)
        {
            Send("send_mob_data", new JsonObject
            {
                ["auth"] = instanceAuth,
                ["mob_id"] = id,
                ["mob_data"] = data?.ToJsonString(false) ?? "{}",
            });
        }

        public virtual void SendSpecialFleetData(string fleetName, string instanceAuth, JsonNode data)
        {
            Send("send_special_fleet", new JsonObject
            {
                ["name"] = fleetName,
                ["auth"] = instanceAuth,
                ["data"] = data?.ToJsonString(false),
            });
        }

        public virtual void UpdatePartyMembers(InstanceInfo instance, int partyId, List<CharacterPartyMember> members)
        {
            lock (Lockher)
            {
                if (GetSyncKey(instance) is string syncKey)
                {
                    Send("send_update_party_members", new JsonObject
                    {
                        ["sync_key"] = syncKey,
                        ["party_id"] = partyId,
                        ["party_members"] = JsonHelpers.ParseNodeUnbuffered(members)
                    });
                }
            }
        }

        protected void HandleInstanceStateChanged(JsonNode doc)
        {
            lock (Lockher)
            {
                if (doc is not null &&
                   GetInstance(doc) is InstanceInfo info &&
                   (InstanceState?)(byte?)doc["state"] is InstanceState state)
                {
                    if (state == InstanceState.Created)
                    {
                        info.Auth = (string)doc["auth"];
                    }
                    else if (state == InstanceState.Started)
                    {
                        info.Address = CreateBattleIpAddress();
                        info.Port = (int?)doc["port"] ?? -1;
                    }

                    InstanceStateChanged?.Invoke(this, new(info, state));
                }
            }
        }

        protected void HandleInstanceAuthReady(JsonNode doc)
        {
            lock (Lockher)
            {
                if (doc is not null &&
                   GetInstance(doc) is InstanceInfo info &&
                   (int?)doc["char_id"] is int charId &&
                   (string)doc["instance_auth"] is string auth)
                {
                    InstanceAuthReady?.Invoke(this, new(info, charId, auth));
                }
            }
        }

        protected void HandleGetCharacterData(JsonNode doc)
        {

            if (doc is not null &&
                (int?)doc["char_id"] is int id)
            {
                CharacterDataRequested?.Invoke(this, new(
                    id,
                    (string)doc["game_mode"],
                    (bool?)doc["include_destroyed_ships"] ?? false));
            }
        }

        protected void HandleGetMobData(JsonNode doc)
        {
            if (doc is not null &&
                (string)doc["auth"] is string auth &&
                (int?)doc["mob_id"] is int id)
            {
                if ((int?)doc["custom"] == 1)
                {
                    MobDataRequested?.Invoke(this, new(
                        auth,
                        id,
                        (int?)doc["min_lvl"] ?? 0,
                        (int?)doc["max_lvl"] ?? 0,
                        (Faction?)(int?)doc["faction"] ?? Faction.None,
                        doc["tags"]?.DeserializeUnbuffered<string[]>() ?? Array.Empty<string>()));
                }
                else
                {
                    MobDataRequested?.Invoke(this, new(auth, id));
                }
            }
        }


        private void HandleSpecialFleet(JsonNode doc)
        {

            if (doc is not null &&
                (string)doc["name"] is string fleetName &&
                (string)doc["auth"] is string auth)
            {
                SpecialFleetRequested?.Invoke(this, new(auth, fleetName));
            }
        }

        private void HandleGetDropList(JsonNode doc)
        {
            if (doc is not null &&
                (string)doc["name"] is string dropName &&
                (string)doc["auth"] is string auth)
            {
                DropListRequested?.Invoke(this, new(auth, dropName));
            }
        }

        public void SendDropList(string dropName, string instanceAuth, string drop)
        {
            Send("send_drop_list", new JsonObject
            {
                ["auth"] = instanceAuth,
                ["name"] = dropName,
                ["data"] = drop,
            });
        }

        private void HandleRewardForEven(JsonNode doc)
        {
            if ((string)doc?["auth"] is string auth)
            {
                RewardForEvenRequested?.Invoke(this, new(auth));
            }
        }


        internal void SendRewardForEven(string reward, string auth)
        {
            Send("send_reward_for_even", new JsonObject
            {
                ["auth"] = auth,
                ["reward"] = reward,
            });
        }

        protected void HandleFleetLeaves(JsonNode doc)
        {
            if (doc is not null &&
                GetInstance(doc) is InstanceInfo info)
            {
                FleetLeaves?.Invoke(this, new(
                    instance: info,
                    fleetType: (DiscoveryObjectType?)(byte?)doc["fleet_type"] ?? DiscoveryObjectType.None,
                    fleetId: (int?)doc["fleet_id"] ?? -1,
                    hex: new((int?)doc["fleet_hex_x"] ?? 0, (int?)doc["fleet_hex_y"] ?? 0)));
            }
        }

        private void HandleUpdateShipStatus(JsonNode doc)
        {
            if (doc is not null &&
                GetInstance(doc) is InstanceInfo info &&
                doc["ship"] is JsonObject ship &&
                (int?)ship["id"] is int id)
            {
                ShipStatusUpdated?.Invoke(this, new(
                    info,
                    id,
                    (string)ship["data"],
                    (string)ship["stats"]));
            }
        }

        public void HandleAddCharacterShipsXp(JsonNode doc)
        {
            if (doc is not null &&
                (int?)doc["char_id"] is int charId &&
                doc["ships_xp"]?.DeserializeUnbuffered<Dictionary<int, int>>() is Dictionary<int, int> xps)
            {
                AddCharacterShipsXp?.Invoke(this, new(charId, xps));
            }
        }

        public void HandleNewInstanceAction(JsonNode doc)
        {
            if (doc is not null &&
                GetInstance(doc) is InstanceInfo info &&
                (string)doc["action"] is string action)
            {
                NewInstanceAction?.Invoke(this, new(info, action, (string)doc["data"]));
            }
        }

        public void SendRankedFleetData(JsonNode data, int fleetId, string auth)
        {
            Send("send_ranked_fleet_data", new JsonObject
            {
                ["auth"] = auth,
                ["fleet_id"] = fleetId,
                ["data"] = data?.ToJsonStringUnbuffered(false),
            });
        }

        public void HandleRankedFleetData(JsonNode doc)
        {
            if (doc is not null &&
                (string)doc["auth"] is string auth &&
                (int?)doc["fleet_id"] is int fleetId)
            {
                RankedFleetRequested?.Invoke(this, new(auth, fleetId));
            }
        }

        protected InstanceInfo GetInstance(JsonNode doc)
        {
            if (doc is null)
                return null;

            return GetInstance((string)doc["sync_key"]);
        }

        protected InstanceInfo GetInstance(string syncKey)
        {
            lock (Lockher)
                if (string.IsNullOrWhiteSpace(syncKey) == false &&
                    Instances.TryGetValue(syncKey, out var instance) == true)
                    return instance;

            return null;
        }

        protected InstanceInfo GetInstance(InstanceCharacter character)
        {
            lock (Lockher)
                return Instances.Values?.FirstOrDefault(i => i.Characters?.Contains(character) == true);
        }

        protected string GetSyncKey(InstanceInfo instance)
        {
            lock (Lockher)
                return instance is null ? null : Instances?.FirstOrDefault(i => i.Value == instance).Key;
        }

        protected virtual string CreateSyncKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string CreateBattleIpAddress()
        {
            if (RemoteEndPoint?.Address is IPAddress address)
            {
                if (address.IsIPv4MappedToIPv6 == true)
                    address = address.MapToIPv4();

                if (address.Equals(IPAddress.Loopback) == false &&
                    address.Equals(IPAddress.IPv6Loopback) == false)
                    return address.ToString();
            }

            return IPAddress.Any.ToString();
        }
    }
}
