using StarfallAfterlife.Bridge.Networking.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceChannel : Channel
    {
        public InstanceManagerServerClient Owner { get; protected set; }

        public SfaInstance Instance { get; protected set; }

        public ChannelClient Client { get; protected set; }

        public InstanceChannel(string name, int id) : base(name, id)
        {
        }

        public InstanceChannel(string name, int id, InstanceManagerServerClient owner, SfaInstance instance, ChannelClient client) : base(name, id)
        {
            Owner = owner;
            Instance = instance;
            Client = client;
        }
    }
}
