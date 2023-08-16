using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public partial class ChannelClient : IDisposable
    {
        public TcpClient Client { get; set; }

        public string Name { get; set; }

        public string Auth { get; set; }

        public bool IsInstance { get; set; }

        public int InstanceId { get; set; }

        protected object SendLocker { get; } = new object();

        protected SfReader Reader;

        protected SfWriter Writer;

        public virtual void Init(TcpClient client)
        {
            Client = client;
        }

        public virtual void Send(int Channel, string text, int ErrorCode = 0, Encoding encoding = null)
        {
            if (Channel < 0)
                return;

            Use((reader, writer) =>
            {
                writer.WriteSfcp(new SFCP.TextPacket
                {
                    Channel = Channel,
                    Charset = 2,
                    Text = text,
                }, encoding);
            });
        }

        public virtual void Send(int Channel, byte[] data, int ErrorCode = 0)
        {
            if (Channel < 0)
                return;

            Use((reader, writer) =>
            {
                writer.WriteSfcp(new SFCP.BinaryPacket
                {
                    Channel = Channel,
                    Data = data,
                    
                });
            });
        }

        public void Use(Action action)
        {
            lock (SendLocker)
                action?.Invoke();
        }

        public void Use(Action<Stream> streamHandler)
        {
            lock (SendLocker)
            {
                try
                {
                    Stream stream = Client?.GetStream();

                    if (stream is null || stream.CanWrite == false)
                        return;

                    streamHandler?.Invoke(stream);
                }
                catch { }
            }
        }

        public void Use(Action<SfReader, SfWriter> readerWriterHandler)
        {
            lock (SendLocker)
            {
                try
                {
                    Stream stream = Client?.GetStream();

                    if (stream is null || stream.CanWrite == false)
                        return;

                    Reader ??= new(stream);
                    Writer ??= new(stream);

                    readerWriterHandler?.Invoke(Reader, Writer);
                }
                catch { }
            }
        }

        public virtual void Disconnect()
        {
            lock (SendLocker)
            {
                try
                {
                    if (Client is TcpClient client &&
                        client.Connected == true &&
                        (client.Client is null) == false)
                    {
                        Client = null;
                        client.Close();
                    }

                }
                catch { }
            }
        }

        public Channel GetChannelById(int id) => this[id];

        public Channel GetChannelByName(string name) => this[name];

        public virtual string GetChannelName(int id) => GetChannelById(id)?.Name ?? null;

        public virtual int GetChannelId(string name) => GetChannelByName(name)?.Id ?? -1;

        public void Dispose()
        {
            Channels?.Clear();
            Reader?.Dispose();
            Writer?.Dispose();
            Reader = null;
            Writer = null;
        }
    }
}
