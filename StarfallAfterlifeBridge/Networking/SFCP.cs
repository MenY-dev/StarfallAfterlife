using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public static class SFCP
    {
        public static Header BuildHeader(ISFCP packet, ushort additionalSize = 0) => new Header
        {
            Id = 85,
            Size = (ushort)(packet.Info.Size + 4 + additionalSize),
            Cmd = packet.Info.Cmd
        };

        public interface ISFCP
        {
            public PacketInfo Info { get; }
        }

        public struct PacketInfo
        {
            public byte Cmd;
            public ushort Size;

            public PacketInfo(byte cmd, ushort size)
            {
                Cmd = cmd;
                Size = size;
            }
        }

        public struct Header
        {
            public PacketInfo Info => new(0, 4);
            public byte Id;
            public ushort Size;
            public byte Cmd;
        }

        public struct UserAuthRequest : ISFCP
        {
            public PacketInfo Info => new(1, 128);
            public string UserName;
            public string TemporaryPass;
        }

        public struct UserAuthResponse : ISFCP
        {
            public PacketInfo Info => new(1, 129);
            public byte ErrorCode;
            public string UserName;
            public string TemporaryPass;
        }

        public struct RegisterRequest : ISFCP
        {
            public PacketInfo Info => new(2, 68);
            public int ChannelId;
            public string ChannelName;
        }

        public struct RegisterResponse : ISFCP
        {
            public PacketInfo Info => new(2, 69);
            public byte ErrorCode;
            public int ChannelId;
            public string ChannelName;
        }

        public struct UnregisterRequest : ISFCP
        {
            public PacketInfo Info => new(3, 4);
            public int ChannelId;
        }

        public struct UnregisterResponse : ISFCP
        {
            public PacketInfo Info => new(3, 5);
            public byte ErrorCode;
            public int ChannelId;
        }

        public struct InstanceAuthRequest : ISFCP
        {
            public PacketInfo Info => new(4, 70);
            public int InstanceId;
            public byte Len;
            public string Auth;
        }

        public struct InstanceAuthResponse : ISFCP
        {
            public PacketInfo Info => new(4, 70);
            public byte ErrorCode;
            public int InstanceId;
            public byte Len;
            public string Password;
        }

        public struct TextPacket : ISFCP
        {
            public PacketInfo Info => new(16, 5);
            public int Channel;
            public byte Charset;
            public string Text;
        }

        public struct BinaryPacket : ISFCP
        {
            public PacketInfo Info => new(32, 5);
            public int Channel;
            public byte Gap8;
            public byte[] Data;
        }
    }
}
