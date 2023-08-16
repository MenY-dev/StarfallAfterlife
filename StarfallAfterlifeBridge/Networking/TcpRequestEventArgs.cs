using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public class TcpRequestEventArgs : EventArgs
    {
        public TcpClient Client { get; }

        public TcpRequestEventArgs(TcpClient client)
        {
            Client = client;
        }
    }
}