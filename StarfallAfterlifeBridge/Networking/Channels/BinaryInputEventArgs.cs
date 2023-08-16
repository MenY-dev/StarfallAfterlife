using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class BinaryInputEventArgs : EventArgs
    {
        public ChannelClient Client { get; }

        public byte[] Data { get; }

        public BinaryInputEventArgs(ChannelClient client, byte[] data)
        {
            Client = client;
            Data = data;
        }
    }
}
