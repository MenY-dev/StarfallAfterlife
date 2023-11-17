using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Serialization;
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
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingClient : IDisposable
    {
        public delegate void UseStreamDelegate(Stream stream);

        public bool IsConnected => TcpClient?.Client?.Connected ?? false;

        public event EventHandler<EventArgs> ConnectionEnd;

        public event EventHandler<EventArgs> Disconnected;

        public TcpClient TcpClient { get; protected set; }

        public DateTime LastInput { get; protected set; }

        protected readonly object _locker = new();

        protected bool disposed;

        private Dictionary<Guid, MessagingResponse> _responses = new();

        private readonly object _requestsLockher = new();

        public MessagingClient() { }

        public void Connect(Uri address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));

            if (disposed == true)
                throw new ObjectDisposedException(GetType().Name);

            lock (_locker)
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

            lock (_locker)
            {
                TcpClient = tcpClient;
            }

            ConnectionEnd?.Invoke(this, EventArgs.Empty);
            HandleInputStream();
        }

        public virtual void Close()
        {
            lock (_locker)
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

                        LastInput = DateTime.Now;
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
                case MessagingMethod.BinaryRequest:
                    byte[] data = new byte[Math.Max(0, header.Length)];
                    buffer.Read(data, 0, data.Length);
                    OnReceiveBinaryRequestInternal(data, header.RequestId);
                    break;
                case MessagingMethod.Text:
                    OnReceiveText(Encoding.UTF8.GetString(buffer.Span));
                    break;
                case MessagingMethod.TextRequest:
                    OnReceiveTextRequestInternal(Encoding.UTF8.GetString(buffer.Span), header.RequestId);
                    break;
            }
        }

        protected virtual void OnReceiveBinaryRequestInternal(byte[] data, Guid requestId)
        {
            lock (_requestsLockher)
            {

                if (_responses.TryGetValue(requestId, out MessagingResponse response) == true)
                {
                    response.SetInput(data);
                    _responses.Remove(requestId);
                }
                else
                {
                    OnReceiveRequest(new MessagingRequest(requestId, data, this));
                }
            }
        }

        protected virtual void OnReceiveTextRequestInternal(string text, Guid requestId)
        {
            lock (_requestsLockher)
            {

                if (_responses.TryGetValue(requestId, out MessagingResponse response) == true)
                {
                    response.SetInput(text);
                    _responses.Remove(requestId);
                }
                else
                {
                    OnReceiveRequest(new MessagingRequest(requestId, text, this));
                }
            }
        }

        protected virtual void OnReceiveRequest(MessagingRequest request)
        {

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

        protected virtual void OnReceiveBinary(SfReader reader) { }

        protected virtual void OnReceiveText(string text) { }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public virtual MessagingResponse SendRequest(string text)
        {
            lock (_requestsLockher)
            {
                var requestId = Guid.NewGuid();
                var response = MessagingResponse.Create(requestId, MessagingMethod.TextRequest);
                SendInternal(new() { Method = MessagingMethod.TextRequest }, text);
                return response;
            }
        }

        public virtual MessagingResponse SendRequest(JsonNode node)
        {
            lock (_requestsLockher)
            {
                var requestId = Guid.NewGuid();
                var response = MessagingResponse.Create(requestId, MessagingMethod.TextRequest);
                SendInternal(new() { Method = MessagingMethod.TextRequest }, node);
                return response;
            }
        }

        public virtual MessagingResponse SendRequest(byte[] bytes)
        {
            lock (_requestsLockher)
            {
                var requestId = Guid.NewGuid();
                var response = MessagingResponse.Create(requestId, MessagingMethod.TextRequest);
                SendInternal(new() { Method = MessagingMethod.Binary }, bytes);
                return response;
            }
        }

        public virtual void Send(string text) =>
            SendInternal(new() { Method = MessagingMethod.Text }, text);

        public virtual void Send(JsonNode node) =>
            SendInternal(new() { Method = MessagingMethod.Text }, node);

        public virtual void Send(byte[] bytes) =>
            SendInternal(new() { Method = MessagingMethod.Binary }, bytes);

        public void SendResponse(MessagingResponse response)
        {
            var header = new MessagingHeader() { RequestId = response.Id, Method = response.Method };

            switch (response.Method)
            {
                case MessagingMethod.BinaryRequest:
                    SendInternal(header, response.Data);
                    break;

                case MessagingMethod.TextRequest:
                    SendInternal(header, response.Text);
                    break;
            }
        }

        protected virtual void SendInternal(MessagingHeader header, string text)
        {
            UseStream((s, text) =>
            {
                var encoding = Encoding.UTF8;
                var maxByteCount = encoding.GetMaxByteCount(text.Length);
                var buffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
                var bytesCount = encoding.GetEncoder().GetBytes(text, buffer, true);

                if (bytesCount < 0)
                    return;

                header.Length = bytesCount;
                header.Write(s);
                s.Write(buffer.AsSpan(0, bytesCount));
                s.Flush();
                ArrayPool<byte>.Shared.Return(buffer);
            }, text);
        }

        protected virtual void SendInternal(MessagingHeader header, JsonNode node)
        {
            UseStream((s, node) =>
            {
                using var buffer = new MemoryStream();
                using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false });
                node.WriteTo(writer);
                writer.Flush();

                header.Length = (int)buffer.Length;
                header.Write(s);
                s.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
                s.Flush();
            }, node);
        }

        protected virtual void SendInternal(MessagingHeader header, byte[] bytes)
        {
            UseStream((s, bytes) =>
            {
                header.Length = bytes.Length;
                header.Write(s);
                s.Write(bytes);
                s.Flush();
            }, bytes);
        }

        public virtual void UseStream(UseStreamDelegate action)
        {
            lock (_locker)
            {
                if (IsConnected == true)
                    action?.Invoke(TcpClient.GetStream());
            }
        }

        public virtual void UseStream<TState>(Action<Stream, TState> action, TState state)
        {
            lock (_locker)
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
