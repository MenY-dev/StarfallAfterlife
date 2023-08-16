using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class MatchmakerChannel : GameChannel
    {
        public MatchmakerChannel(string name, int id) : base(name, id)
        {

        }

        public override void Register(ChannelClient client)
        {
            base.Register(client);

            SetClientState(1);
            RequestDataUpdate();
        }

        protected virtual void SetClientState(byte state)
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
    }
}
