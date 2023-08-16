using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class BattleGroundChannel : GameChannel
    {
        public BattleGroundChannel(string name, int id, SfaGame game) : base(name, id, game)
        {
        }

        public override void Input(ChannelClient client, byte[] data)
        {
            base.Input(client, data);
            Game?.SfaClient?.Send(data, SfaServerAction.BattleGroundChannel);
        }

        public void SendInstanceReady(string address, int port, string auth)
        {
            SendBattleGroundMessage(
                BattleGroundServerAction.InstanceReady,
                writer =>
                {
                    writer.WriteShortString(address, -1, true, Encoding.ASCII);
                    writer.WriteUInt16((ushort)port);
                    writer.WriteShortString(auth, -1, true, Encoding.ASCII);
                });
        }


        public virtual void SendBattleGroundMessage(BattleGroundServerAction action, Action<SfWriter> writeAction = null)
        {
            using var writer = new SfWriter();
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);
            Send(writer.ToArray());
        }
    }
}
