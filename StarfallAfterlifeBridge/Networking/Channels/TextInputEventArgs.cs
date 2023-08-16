using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class TextInputEventArgs
    {
        public ChannelClient Client { get; }

        public string Text { get; }

        public TextInputEventArgs(ChannelClient client, string text)
        {
            Client = client;
            Text = text;
        }
    }
}
