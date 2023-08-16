using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.IO
{
    public partial class SfWriter
    {
        public void WriteSfcp(SFCP.Header header)
        {
            Write(header.Id);
            Write(header.Size);
            Write(header.Cmd);
        }

        public void WriteSfcp(SFCP.UserAuthRequest packet)
        {
            WriteSfcp(SFCP.BuildHeader(packet));
            WriteString(packet.UserName, 64, false, Encoding.UTF8);
            WriteString(packet.TemporaryPass, 64, false, Encoding.UTF8);
        }

        public void WriteSfcp(SFCP.UserAuthResponse packet)
        {
            WriteSfcp(SFCP.BuildHeader(packet));
            WriteByte(packet.ErrorCode);
            WriteString(packet.UserName, 64, false, Encoding.UTF8);
            WriteString(packet.TemporaryPass, 64, false, Encoding.UTF8);
        }

        public void WriteSfcp(SFCP.RegisterResponse packet)
        {
            WriteSfcp(SFCP.BuildHeader(packet));
            WriteByte(packet.ErrorCode);
            WriteInt32(packet.ChannelId);
            WriteString(packet.ChannelName, 64, false, Encoding.UTF8);
        }

        public void WriteSfcp(SFCP.UnregisterResponse packet)
        {
            WriteSfcp(SFCP.BuildHeader(packet));
            WriteByte(packet.ErrorCode);
            WriteInt32(packet.ChannelId);
        }

        public void WriteSfcp(SFCP.InstanceAuthRequest packet)
        {
            byte[] password = Encoding.UTF8.GetBytes(packet.Auth);
            packet.Len = (byte)password.Length;

            WriteSfcp(SFCP.BuildHeader(packet));
            WriteInt32(packet.InstanceId);
            WriteByte(packet.Len);
            Write(password);
        }

        public void WriteSfcp(SFCP.InstanceAuthResponse packet)
        {
            WriteSfcp(SFCP.BuildHeader(packet));
            WriteByte(packet.ErrorCode);
            WriteInt32(packet.InstanceId);
            WriteByte(packet.Len);
            WriteString(packet.Password, 64, false, Encoding.ASCII);
        }

        public void WriteSfcp(SFCP.BinaryPacket packet)
        {
            var data = packet.Data ?? Array.Empty<byte>();
            ushort length = (ushort)data.Length;
            WriteSfcp(SFCP.BuildHeader(packet, length));
            WriteInt32(packet.Channel);
            WriteByte(packet.Gap8);
            Write(data);
        }

        public void WriteSfcp(SFCP.TextPacket packet, Encoding encoding = null)
        {
            if (encoding is null)
                encoding = Encoding ?? Encoding.UTF8;

            byte[] data = encoding.GetBytes(packet.Text ?? string.Empty);

            WriteSfcp(SFCP.BuildHeader(packet, (ushort)data.Length));
            WriteInt32(packet.Channel);
            WriteByte(packet.Charset);
            Write(data);
        }
    }
}
