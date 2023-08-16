using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public partial class GameChannelManager : ChannelManager<GameChannelClient>
    {
        public GameChannelClient Client
        {
            get
            {
                lock (ClientsLocker)
                    return Clients?.FirstOrDefault();
            }
        }

        protected override GameChannelClient CreateNewClient(TcpClient tcpClient)
        {
            var client = base.CreateNewClient(tcpClient);
            client?.AddRange(Channels);
            return client;
        }
    }
}
