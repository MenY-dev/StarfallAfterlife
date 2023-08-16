using StarfallAfterlife.Bridge.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class GameChannel : Channel
    {
        public ChannelClient Client { get; protected set; }

        public SfaGame Game { get; protected set; }

        public GameChannel(string name, int id) : base(name, id)
        {
        }

        public GameChannel(string name, int id, SfaGame game) : base(name, id)
        {
            Game = game;
        }

        public override void Register(ChannelClient client)
        {
            Client = client;
            base.Register(client);
        }

        public virtual void Send(string text, int ErrorCode = 0, Encoding encoding = null)
        {
            Send(Client, text, ErrorCode, encoding);
        }

        public virtual void Send(byte[] data, int ErrorCode = 0)
        {
            Send(Client, data, ErrorCode);
        }
    }
}
