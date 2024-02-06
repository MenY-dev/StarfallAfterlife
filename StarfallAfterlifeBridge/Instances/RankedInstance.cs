using StarfallAfterlife.Bridge.Networking.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class RankedInstance : SfaInstance
    {
        public override string Map => RankedMap;

        protected string RankedMap { get; set; } = "r1_confrontation";

        public InstanceDiscoveryChannel DiscoveryChannel { get; protected set; }

        public InstanceCharacterPartyChannel CharactPartyChannel { get; protected set; }

        public override void Init(InstanceManagerServerClient context)
        {
            base.Init(context);

            RankedMap = Info?.Map ?? "r1_confrontation";
        }

        public override JsonNode CreateInstanceConfig()
        {
            var doc = base.CreateInstanceConfig();
            doc["game_mode"] = "ranked";
            return doc;
        }

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
