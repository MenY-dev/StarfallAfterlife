using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class ChannelManager : ChannelManager<ChannelClient> { }

    public partial class ChannelManager<TClient> : TcpServer where TClient : ChannelClient, new()
    {
        protected List<TClient> Clients { get; } = new();

        protected object ClientsLocker { get; } = new();

        protected object ChannelsLocker { get; } = new();

        protected override void HandleClient(TcpClient tcpClient)
        {
            SfaDebug.Print($"HandleClient (Client = {tcpClient.Client.RemoteEndPoint}");

            TClient channelClient = CreateNewClient(tcpClient);
            NetworkStream stream = tcpClient.GetStream();

            lock (ClientsLocker)
                Clients.Add(channelClient);

            try
            {
                using SfReader reader = new(stream);

                while (stream.CanRead && stream.CanWrite && stream.Socket is not null)
                {
                    var header = reader.ReadSfcpHeader();

                    if (header.Id == 85 && header.Size > 3)
                    {
                        OnDataReceived(channelClient, header);
                    }
                }
            }
            catch { }
            finally
            {
                lock (ClientsLocker)
                {
                    channelClient.Disconnect();

                    if (Clients.Contains(channelClient))
                    {
                        Clients.Remove(channelClient);
                        channelClient.Dispose();
                    }
                }
            }
        }

        protected void OnDataReceived(TClient client, SFCP.Header header)
        {
            SfaDebug.Print($"OnDataReceived (Cmd = {header.Cmd}, Size = {header.Size})", GetType().Name);

            switch (header.Cmd)
            {
                case 1:
                    HandleUserAuth(client, ReadUserAuth(client, header));
                    break;

                case 2:
                    HandleChannelRegister(client, ReadChannelRegister(client, header));
                    break;

                case 4:
                    HandleInstanceAuth(client, ReadInstanceAuth(client, header));
                    break;

                case 16:
                    HandleTextPacket(client, ReadTextPacket(client, header));
                    break;

                case 32:
                    HandleBinaryPacket(client, ReadBinaryPacket(client, header));
                    break;

                default:
                    break;
            }
        }

        protected virtual SFCP.UserAuthRequest ReadUserAuth(TClient client, SFCP.Header header)
        {
            SFCP.UserAuthRequest request = default;
            client?.Use((reader, writer) => request = reader.ReadSfcpUserAuthRequest());
            return request;
        }

        protected virtual void HandleUserAuth(TClient client, SFCP.UserAuthRequest request)
        {
            client?.Use((reader, writer) =>
            {
                client.IsInstance = false;
                client.Name = request.UserName;
                client.Auth = request.TemporaryPass;

                SfaDebug.Print($"UserAuth (UserName = {request.UserName}, TemporaryPass = {request.TemporaryPass})", GetType().Name);

                writer.WriteSfcp(new SFCP.UserAuthResponse()
                {
                    ErrorCode = 0,
                    UserName = request.UserName,
                    TemporaryPass = request.TemporaryPass
                });
            });
        }

        protected virtual SFCP.InstanceAuthRequest ReadInstanceAuth(TClient client, SFCP.Header header)
        {
            SFCP.InstanceAuthRequest request = default;
            client?.Use((reader, writer) => request = reader.ReadSfcpInstanceAuthRequest());
            return request;
        }

        protected virtual void HandleInstanceAuth(TClient client, SFCP.InstanceAuthRequest request)
        {
            client?.Use((reader, writer) =>
            {
                client.IsInstance = true;
                client.InstanceId = request.InstanceId;
                client.Auth = request.Auth;

                writer.WriteSfcp(new SFCP.InstanceAuthResponse()
                {
                    ErrorCode = 0,
                    InstanceId = request.InstanceId,
                    Len = request.Len,
                    Password = request.Auth
                });
            });
        }

        protected virtual SFCP.RegisterRequest ReadChannelRegister(TClient client, SFCP.Header header)
        {
            SFCP.RegisterRequest request = default;
            client?.Use((reader, writer) => request = reader.ReadSfcpRegisterRequest());
            return request;
        }

        public virtual void HandleChannelRegister(TClient client, SFCP.RegisterRequest request)
        {
            client?.Use((reader, writer) =>
            {
                var channel = client?.GetChannelByName(request.ChannelName);
                channel?.Register(client);
                SfaDebug.Print(
                    $"HandleChannelRegister (Name = {request.ChannelName}, Id = {channel?.Id}, Succes = {channel is not null})",
                    GetType().Name);
            });
        }

        protected virtual SFCP.TextPacket ReadTextPacket(TClient client, SFCP.Header header)
        {
            SFCP.TextPacket request = default;
            client?.Use((reader, writer) => request = reader.ReadSfcpTextPacket(header));
            return request;
        }

        public virtual void HandleTextPacket(TClient client, SFCP.TextPacket request)
        {
            client?.Use((reader, writer) =>
            {
                client?.GetChannelById(request.Channel)?.Input(client, request.Text ?? string.Empty);
            });
        }

        protected virtual SFCP.BinaryPacket ReadBinaryPacket(TClient client, SFCP.Header header)
        {
            SFCP.BinaryPacket request = default;
            client?.Use((reader, writer) => request = reader.ReadSfcpBinaryPacket(header));
            return request;
        }

        public virtual void HandleBinaryPacket(TClient client, SFCP.BinaryPacket request)
        {
            client?.Use((reader, writer) =>
            {
                client?.GetChannelById(request.Channel)?.Input(client, request.Data ?? Array.Empty<byte>());
            });
        }

        public virtual TClient GetClient(string auth)
        {
            if (string.IsNullOrWhiteSpace(auth))
                return null;

            lock (ClientsLocker)
            {
                foreach (var item in Clients)
                    if (item.Auth == auth)
                        return item;

                return null;
            }
        }

        public virtual TClient GetClient(int instanceId)
        {
            lock (ClientsLocker)
                return Clients?.FirstOrDefault(c => c.InstanceId == instanceId);
        }

        public TClient GetClient(TcpClient tcpClient)
        {
            lock (ClientsLocker)
                return Clients?.FirstOrDefault(c => c.Client == tcpClient);
        }

        protected virtual TClient CreateNewClient(TcpClient tcpClient)
        {
            TClient client = new();
            client.Init(tcpClient);
            return client;
        }
    }
}
