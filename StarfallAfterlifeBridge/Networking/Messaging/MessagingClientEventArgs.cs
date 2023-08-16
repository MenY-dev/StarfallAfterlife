using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingClientEventArgs : EventArgs
    {
        public MessagingClient Client { get; }

        public MessagingClientEventArgs(MessagingClient client)
        {
            Client = client;
        }
    }
}
