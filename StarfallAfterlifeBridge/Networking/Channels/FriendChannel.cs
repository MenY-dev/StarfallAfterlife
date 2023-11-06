using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class FriendChannel : GameChannel
    {
        public bool IsUserChannel { get; init; }

        public FriendChannel(string name, int id, SfaGame game) : base(name, id, game)
        {

        }

        public override void Input(ChannelClient client, byte[] data)
        {
            base.Input(client, data);
            Game?.SfaClient?.Send(data, IsUserChannel ?
                SfaServerAction.UserFriendChannel :
                SfaServerAction.CharacterFriendChannel);
        }

        public virtual void SendFriendChannelMessage(FriendChannelServerAction action, Action<SfWriter> writeAction = null)
        {
            using var writer = new SfWriter();
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);
            Send(writer.ToArray());
        }
    }
}
