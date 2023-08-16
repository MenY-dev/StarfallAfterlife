using StarfallAfterlife.Bridge.Networking.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public partial class InstanceManager : MessagingServer<InstanceManagerServerClient>
    {
        protected List<InstanceManagerServerClient> Clients { get; } = new();

        protected object ClientsLockher { get; } = new object();

        protected override InstanceManagerServerClient CreateNewClient()
        {
            lock (ClientsLockher)
            {
                var newClient = base.CreateNewClient();
                Clients.Add(newClient);
                newClient.Init(this);
                return newClient;
            }
        }
    }
}
