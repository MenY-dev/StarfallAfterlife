using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public struct IpInfo
    {
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
        public TcpState State;
        public int ProcessId;
    }
}
