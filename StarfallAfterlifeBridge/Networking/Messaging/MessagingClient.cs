using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Tasks;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingClient : IDisposable
    {
        public delegate void UseStreamDelegate(Stream stream);

        public bool IsConnected => TcpClient?.Client?.Connected ?? false;

        public event EventHandler<EventArgs> ConnectionEnd;

        public event EventHandler<EventArgs> Disconnected;

        public TcpClient TcpClient { get; protected set; }

        protected readonly object locker = new();

        protected bool disposed;

        public MessagingClient() { }

        public void Connect(Uri address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));

            if (disposed == true)
                throw new ObjectDisposedException(GetType().Name);

            lock (locker)
            {
                TcpClient = new TcpClient();
                TcpClient.BeginConnect(address.Host, address.Port, OnClientConnected, TcpClient);
            }
        }

        public Task<bool> ConnectAsync(Uri address)
        {
            var waiter = EventWaiter<EventArgs>
                .Create()
                .Subscribe(e => ConnectionEnd += e)
                .Unsubscribe(e => ConnectionEnd -= e)
                .Start()
                .ContinueWith(t => IsConnected);

            Connect(address);
            return waiter;
        }

        public void Connect(TcpClient tcpClient)
        {
            if (tcpClient is null)
                throw new ArgumentNullException(nameof(tcpClient));

            if (disposed == true)
                throw new ObjectDisposedException(GetType().Name);

            lock (locker)
            {
                TcpClient = tcpClient;
            }

            ConnectionEnd?.Invoke(this, EventArgs.Empty);
            HandleInputStream();
        }

        public virtual void Close()
        {
            lock (locker)
            {
                try
                {
                    if (TcpClient?.Connected == true)
                        TcpClient.Close();
                }
                catch
                {

                }
            }
        }

        protected virtual void OnClientConnected(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;

            try
            {
                client?.EndConnect(ar);
            }
            catch
            {
                return;
            }

            ConnectionEnd?.Invoke(this, EventArgs.Empty);
            SfaDebug.Print($"Connected to serveer!", GetType().Name);
            HandleInputStream();
        }

        protected virtual void HandleInputStream()
        {
            Task.Run(() =>
            {
                using var buffer = new PooledStream();
                using var reader = new SfReader(buffer);
                byte[] chunk = new byte[1024];
                long version = 0;

                void CopyToBuffer(Stream stream, int count)
                {
                    if (count < 1)
                        return;

                    int readResult;

                    while (count > 0 && (readResult = stream.Read(chunk, 0, Math.Min(chunk.Length, count))) > 0)
                    {
                        buffer.Write(chunk, 0, readResult);
                        count -= readResult;
                    }
                }

                try
                {
                    var stream = TcpClient.GetStream();

                    while (IsConnected == true)
                    {
                        var header = MessagingHeader.ReadNext(stream);
                        buffer.Seek(0, SeekOrigin.Begin);

                        if (header.Method == MessagingMethod.Binary ||
                            header.Method == MessagingMethod.Text)
                        {
                            CopyToBuffer(stream, header.Length);
                            buffer.SetLength(buffer.Position);
                            buffer.Seek(0, SeekOrigin.Begin);
                        }
                        else if (header.Method == MessagingMethod.HTTP && version == 0)
                        {
                            OnReceiveHttpInternal(header, stream);
                            break;
                        }

                        version++;
                        OnReceiveInternal(header, buffer, reader);

                        if (buffer.Length > 20480)
                        {
                            //buffer.SetLength(0);
                            //buffer.TrimBufferToLength();
                            //GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                        }
                    }
                }
                catch (Exception e)
                {
                    SfaDebug.Log(e.Message);
                }
                finally
                {
                    Close();
                    OnDisconnected();
                    SfaDebug.Log("Disconnected!");
                }

            }).ContinueWith(t =>
            {
                t.Dispose();
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        private void OnReceiveInternal(MessagingHeader header, PooledStream buffer, SfReader reader)
        {
            switch (header.Method)
            {
                case MessagingMethod.Binary:
                    OnReceiveBinary(reader);
                    break;
                case MessagingMethod.Text:
                    OnReceiveText(Encoding.UTF8.GetString(buffer.Span));
                    break;
            }
        }


        private void OnReceiveHttpInternal(MessagingHeader header, Stream buffer)
        {
            var stringReader = new StreamReader(buffer, Encoding.UTF8, false);
            string request = stringReader.ReadLine()?
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)?
                .FirstOrDefault();

            if (request == "/map.json")
            {
                var response = "HTTP/1.1 200 OK\r\n";

                response += $"Date: {DateTime.Now}\r\n";
                response += $"Connection: close\r\n";
                response += $"Content-Type: application/json\r\n\r\n";

                response += $"This Is Map Json";

                UseStream((s, response) =>
                {
                    var bytes = Encoding.UTF8.GetBytes(response);
                    s.Write(bytes);
                    s.Close();
                }, response);
            }

        }

        protected virtual void OnReceive(MessagingHeader header, SfReader reader) { }

        protected virtual void OnReceiveBinary(SfReader reader) { }

        protected virtual void OnReceiveText(string text) { }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Send(string text)
        {
            UseStream((s, text) =>
            {
                var encoding = Encoding.UTF8;
                var maxByteCount = encoding.GetMaxByteCount(text.Length);
                var buffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
                var bytesCount = encoding.GetEncoder().GetBytes(text, buffer, true);

                if (bytesCount < 0)
                    return;

                new MessagingHeader
                {
                    Method = MessagingMethod.Text,
                    Length = bytesCount,
                }.Write(s);
                s.Write(buffer.AsSpan(0, bytesCount));
                s.Flush();
                ArrayPool<byte>.Shared.Return(buffer);
            }, text);
        }

        public virtual void Send(JsonNode node)
        {
            if (node is null)
                return;

            UseStream((s, node) =>
            {
                using var buffer = new PooledStream();
                using var writer = new Utf8JsonWriter((IBufferWriter<byte>)buffer, new JsonWriterOptions { Indented = false });
                node.WriteTo(writer);
                writer.Flush();

                new MessagingHeader
                {
                    Method = MessagingMethod.Text,
                    Length = (int)buffer.Length,
                }.Write(s);
                s.Write(buffer.Span);
            }, node);
        }

        public virtual void Send(System.Text.Json.Nodes.JsonNode node)
        {
            if (node is null)
                return;

            UseStream((s, node) =>
            {
                using var buffer = new MemoryStream();
                using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false });
                node.WriteTo(writer);
                writer.Flush();

                new MessagingHeader
                {
                    Method = MessagingMethod.Text,
                    Length = (int)buffer.Length,
                }.Write(s);
                s.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
            }, node);
        }

        public virtual void Send(byte[] bytes)
        {
            UseStream((s, bytes) =>
            {
                new MessagingHeader
                {
                    Method = MessagingMethod.Binary,
                    Length = bytes.Length,
                }.Write(s);
                s.Write(bytes);
            }, bytes);
        }

        public virtual void UseStream(UseStreamDelegate action)
        {
            lock (locker)
            {
                if (IsConnected == true)
                    action?.Invoke(TcpClient.GetStream());
            }
        }

        public virtual void UseStream<TState>(Action<Stream, TState> action, TState state)
        {
            lock (locker)
            {
                if (IsConnected == true)
                    action?.Invoke(TcpClient.GetStream(), state);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Close();
                }

                TcpClient = null;
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }
    }
}
