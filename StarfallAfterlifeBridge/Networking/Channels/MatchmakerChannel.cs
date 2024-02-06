using StarfallAfterlife.Bridge.Diagnostics;
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
    public class MatchmakerChannel : GameChannel
    {
        public MatchmakerChannel(string name, int id, SfaGame game) : base(name, id, game)
        {

        }

        public override void Register(ChannelClient client)
        {
            base.Register(client);

            SetClientState(1);
            RequestDataUpdate();
        }

        public override void Input(ChannelClient client, byte[] data)
        {
            base.Input(client, data);
            Game?.SfaClient?.Send(data, SfaServerAction.MatchmakerChannel);
        }

        public virtual void SetClientState(byte state)
        {
            byte[] data = new byte[6];

            using (SfWriter writer = new(data))
            {
                writer.WriteByte(11);
                writer.WriteByte(state);
                writer.WriteInt32(0);
            }

            Send(data);
            SfaDebug.Print($"SetClientState: (State = {state})", Name);
        }

        public virtual void RequestDataUpdate()
        {
            Send(new byte[1] { 12 });
            SfaDebug.Print($"Request data update!", Name);
        }

        public override void Send(byte[] data, int ErrorCode = 0)
        {
            base.Send(data, ErrorCode);
        }

        public void SendInstanceReady(string address, int port, string auth)
        {
            SendMatchmakerMessage(
                MatchmakerChannelServerAction.InstanceReady,
                writer =>
                {
                    writer.WriteShortString(address, -1, true, Encoding.ASCII);
                    writer.WriteUInt16((ushort)port);
                    writer.WriteShortString(auth, -1, true, Encoding.ASCII);
                });
        }

        public virtual void SendMatchmakerMessage(MatchmakerChannelServerAction action, Action<SfWriter> writeAction = null)
        {
            using var writer = new SfWriter();
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);
            Send(writer.ToArray());
        }
    }
}
