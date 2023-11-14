using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.IO
{
    public partial class SfReader
    {
        public SFCP.Header ReadSfcpHeader()
        {
            return new()
            {
                Id = ReadByte(),
                Size = ReadUInt16(),
                Cmd = ReadByte()
            };
        }

        public SFCP.UserAuthRequest ReadSfcpUserAuthRequest()
        {
            return new()
            {
                UserName = ReadString(64, Encoding.UTF8),
                TemporaryPass = ReadString(64, Encoding.UTF8)
            };
        }

        public SFCP.UserAuthResponse ReadSfcpUserAuthResponse()
        {
            return new()
            {
                ErrorCode = ReadByte(),
                UserName = ReadString(64, Encoding.UTF8),
                TemporaryPass = ReadString(64, Encoding.UTF8)
            };
        }

        public SFCP.RegisterRequest ReadSfcpRegisterRequest()
        {
            return new()
            {
                ChannelId = ReadInt32(),
                ChannelName = ReadString(64, Encoding.UTF8)
            };
        }

        public SFCP.RegisterResponse ReadSfcpRegisterResponse()
        {
            return new()
            {
                ErrorCode = ReadByte(),
                ChannelId = ReadInt32(),
                ChannelName = ReadString(64, Encoding.UTF8)
            };
        }

        public SFCP.UnregisterRequest ReadSfcpUnregisterRequest()
        {
            return new()
            {
                ChannelId = ReadInt32(),
            };
        }

        public SFCP.UnregisterResponse ReadSfcpUnregisterResponse()
        {
            return new()
            {
                ErrorCode = ReadByte(),
                ChannelId = ReadInt32()
            };
        }

        public SFCP.InstanceAuthRequest ReadSfcpInstanceAuthRequest()
        {
            SFCP.InstanceAuthRequest packet = new();

            packet.InstanceId = ReadInt32();
            packet.Len = ReadByte();
            packet.Auth = ReadString(packet.Len, Encoding.UTF8);

            return packet;
        }

        public SFCP.InstanceAuthResponse ReadSfcpInstanceAuthResponse()
        {
            SFCP.InstanceAuthResponse packet = new();

            packet.ErrorCode = ReadByte();
            packet.InstanceId = ReadInt32();
            packet.Len = ReadByte();
            packet.Password = ReadString(packet.Len, Encoding.UTF8);

            return packet;
        }

        public SFCP.BinaryPacket ReadSfcpBinaryPacket(SFCP.Header header)
        {
            SFCP.BinaryPacket packet = new();
            byte[] data = new byte[header.Size - header.Info.Size - packet.Info.Size];

            packet.Channel = ReadInt32();
            packet.Gap8 = ReadByte();
            Read(data);
            packet.Data = data;

            return packet;
        }

        public SFCP.TextPacket ReadSfcpTextPacket(SFCP.Header header)
        {
            SFCP.TextPacket packet = new();
            byte[] data = new byte[header.Size - header.Info.Size - packet.Info.Size];

            packet.Channel = ReadInt32();
            packet.Charset = ReadByte();
            Read(data);

            switch (packet.Charset)
            {
                case 2: packet.Text = Encoding.Unicode.GetString(data)?.TrimEnd('\0'); break;
                default: packet.Text = Encoding.UTF8.GetString(data)?.TrimEnd('\0'); break;
            }

            return packet;
        }
    }
}
