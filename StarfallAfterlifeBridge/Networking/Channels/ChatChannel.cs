using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class ChatChannel : GameChannel
    {
        public ChatChannel(string name, int id, SfaGame game) : base(name, id, game)
        {

        }

        public override void Input(ChannelClient client, string text)
        {
            base.Input(client, text);

            if (text is null or { Length: < 1 })
                return;

            var msg = text[1..];
            var isPrivate = text.StartsWith('U');

            if (isPrivate == true)
            {
                var packet = msg.Split(':', StringSplitOptions.RemoveEmptyEntries);

                Game?.SfaClient?.SendChatMessage(
                    Name,
                    packet.ElementAtOrDefault(1),
                    true,
                    packet.ElementAtOrDefault(0));
            }
            else
            {
                Game?.SfaClient?.SendChatMessage(Name, msg, isPrivate);
            }
        }

        public virtual void SendMessage(string user, string text, bool isPrivate = false)
        {
            string tag = Encoding.ASCII.GetString(new byte[] { (byte)(isPrivate ? 85 : 65) });
            Send($"{tag}{user}:{text}", encoding: Encoding.Unicode);
        }
    }
}
