using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceChannelManager : ChannelManager<InstanceChannelClient>
    {
        public InstanceManagerServerClient Owner { get; protected set; }

        public InstanceChannelManager(InstanceManagerServerClient owner)
        {
            Owner = owner;
        }

        protected override void HandleInstanceAuth(InstanceChannelClient client, SFCP.InstanceAuthRequest request)
        {
            base.HandleInstanceAuth(client, request);
            var instance = Owner?.GetInstance(request.Auth);

            if (instance is not null)
            {
                instance.ConnectChannels(client);
                instance.SetInstanceReady();
            }
        }
    }
}
