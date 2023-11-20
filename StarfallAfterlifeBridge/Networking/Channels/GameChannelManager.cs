using StarfallAfterlife.Bridge.Game;
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
        public SfaGame Game { get; protected set; }

        public GameChannelManager() { }

        public GameChannelManager(SfaGame game)
        {
            Game = game;
        }

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

        public override void HandleChannelRegister(GameChannelClient client, SFCP.RegisterRequest request)
        {
            lock(ChannelsLocker)
            {
                if (Channels.Any(c => c.Name == request.ChannelName || c.Id == request.ChannelId) == false)
                {
                    var id = Enumerable.Range(1, Count + 2).FirstOrDefault(i => this[i] is null);
                    var newChannel = new ChatChannel(request.ChannelName, id, Game);
                    Channels.Add(newChannel);
                    Client?.Add(newChannel);
                }
            }

            base.HandleChannelRegister(client, request);
        }
    }
}
