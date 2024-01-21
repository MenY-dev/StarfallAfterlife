using StarfallAfterlife.Bridge.Native.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;

namespace StarfallAfterlife.Bridge.Networking
{
    public static class NetUtils
    {

        public static List<IpInfo> GetProcessUdpListeners(Process process) =>
            GetProcessUdpListeners(process?.Id ?? 0);

        public static List<IpInfo> GetProcessUdpListeners(int pid) =>
            GetUdpListenersInfo().Where(i => i.ProcessId == pid).ToList();

        public static List<IpInfo> GetUdpListenersInfo()
        {
            List<IpInfo> result = new();

            static int ToLittleEndianPort(uint port)
            {
                return (int)(((port & 0x00FF) << 8) |
                             ((port & 0xFF00) >> 8));
            }

            try
            {
                foreach (var item in GetUdpListeners())
                {
                    result.Add(new IpInfo
                    {
                        LocalEndPoint = new IPEndPoint(
                            item.localAddr,
                            ToLittleEndianPort(item.localPort)),
                        RemoteEndPoint = new IPEndPoint(0, 0),
                        State = TcpState.Listen,
                        ProcessId = (int)item.owningPid
                    });
                }


                foreach (var item in GetUdp6Listeners())
                {
                    result.Add(new IpInfo
                    {
                        LocalEndPoint = new IPEndPoint(new IPAddress(
                            item.localAddr,
                            item.localScopeId),
                            ToLittleEndianPort(item.localPort)),
                        RemoteEndPoint = new IPEndPoint(0, 0),
                        State = TcpState.Listen,
                        ProcessId = (int)item.owningPid
                    });
                }
            }
            catch { }

            return result;
        }

        private static List<MIB_UDPROW_OWNER_PID> GetUdpListeners() => GetUdpListeners<MIB_UDPROW_OWNER_PID>();

        private static List<MIB_UDP6ROW_OWNER_PID> GetUdp6Listeners() => GetUdpListeners<MIB_UDP6ROW_OWNER_PID>();

        private static List<TRow> GetUdpListeners<TRow>() where TRow : struct
        {
            TRow[] tableRows;
            int buffSize = 0;
            Type rowType = typeof(TRow);
            int ipVersion = rowType == typeof(MIB_UDPROW_OWNER_PID) ? 2 :
                            rowType == typeof(MIB_UDP6ROW_OWNER_PID) ? 23 : 0;

            GetExtendedUdpTable(nint.Zero, ref buffSize, true, ipVersion, UDP_TABLE_CLASS.OWNER_PID);
            nint tcpTablePtr = Marshal.AllocHGlobal(buffSize);

            try
            {
                if (GetExtendedUdpTable(tcpTablePtr, ref buffSize, true, ipVersion, UDP_TABLE_CLASS.OWNER_PID) != 0)
                    return new();

                int rowStructSize = Marshal.SizeOf(rowType);
                uint numEntries = Marshal.PtrToStructure<uint>(tcpTablePtr);
                nint rowPtr = (nint)((long)tcpTablePtr + sizeof(uint));

                tableRows = new TRow[numEntries];

                for (int i = 0; i < numEntries; i++)
                {
                    tableRows[i] = Marshal.PtrToStructure<TRow>(rowPtr);
                    rowPtr = (nint)((long)rowPtr + rowStructSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }

            return tableRows?.ToList() ?? new();
        }

        public static (IPAddress Address, string Name, NetworkInterface Info)[] GetInterfaces()
        {
            var result = new List<(IPAddress, string, NetworkInterface)>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                   .Where(i => i?.OperationalStatus is OperationalStatus.Up);

                foreach (var item in interfaces)
                {
                    var ip = item.GetIPProperties()?.UnicastAddresses?.FirstOrDefault(
                       x => x.Address?.AddressFamily == AddressFamily.InterNetwork);

                    if (ip is null)
                        continue;

                    result.Add((ip.Address, item.Name, item));
                }
            }
            catch { }

            return result.ToArray();
        }
    }
}
