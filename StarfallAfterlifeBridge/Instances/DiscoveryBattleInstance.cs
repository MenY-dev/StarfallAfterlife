﻿using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class DiscoveryBattleInstance : SfaInstance
    {
        public override string Map => "DFA_first";

        public InstanceDiscoveryChannel DiscoveryChannel { get; protected set; }

        public InstanceCharacterPartyChannel CharactPartyChannel { get; protected set; }

        public override JsonNode CreateInstanceConfig()
        {
            var doc = base.CreateInstanceConfig();

            doc["game_mode"] = "discovery";

            if (Info.ExtraData is not null &&
                JsonNode.Parse(Info.ExtraData) is JsonObject extraData)
                doc["extradata"] = extraData;

            return doc;
        }

        public virtual void OnFleetLeaves(DiscoveryObjectType fleetType, int fleetId, SystemHex hex)
        {
            Context?.SendFleetLeaves(this, fleetType, fleetId, hex);
        }

        public virtual void UpdatePartyMembers(int partyId, List<CharacterPartyMember> members) =>
            CharactPartyChannel?.SendPartyMembers(partyId, members);

        public override void ConnectChannels(InstanceChannelClient client)
        {
            DiscoveryChannel ??= new InstanceDiscoveryChannel("Discovery", 1, Context, this, client);
            CharactPartyChannel ??= new InstanceCharacterPartyChannel("CharactParty", 2, Context, this, client);

            client.Add(DiscoveryChannel);
            client.Add(CharactPartyChannel);
            client.Add(new InstanceChannel("UserFriends", 3, Context, this, client));
            client.Add(new InstanceChannel("CharacterFriends", 4, Context, this, client));
        }
    }
}
