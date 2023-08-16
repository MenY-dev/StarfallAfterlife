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
    public class ChatConsoleChannel : GameChannel
    {
        public ChatConsoleChannel(string name, int id, SfaGame game) : base(name, id, game)
        {

        }

        public override void Input(ChannelClient client, string text)
        {
            base.Input(client, text);
            Game?.SfaClient?.Send(text?[1..], SfaServerAction.GlobalChat);
        }

        public virtual void SendMessage(string user, string text, ushort type)
        {
            string tag = Encoding.ASCII.GetString(new byte[] { 65 });
            SfaDebug.Print(tag);
            Send($"{tag}{user}:{text}", encoding: Encoding.Unicode);
        }
    }
}
