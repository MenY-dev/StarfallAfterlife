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

        public bool IsConnected => CheckStatus();

        public event EventHandler<EventArgs> ConnectionEnd;

        public event EventHandler<EventArgs> Disconnected;

        public TcpClient TcpClient { get; protected set; }

        public DateTime LastInput { get; protected set; }

        protected TaskCompletionSource<bool> ConnectionCompletion;

        protected readonly object _locker = new();

        protected bool disposed;

        private Dictionary<Guid, MessagingResponse> _responses = new();

        private readonly object _requestsLockher = new();

        private bool _readingAvailable = false;

        private string _debugTargetAddsess = null;

        public MessagingClient() { }

        public void Connect(Uri address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));

            if (disposed == true)
                throw new ObjectDisposedException(GetType().Name);

            lock (_locker)
            {
                _debugTargetAddsess = address?.ToString();
                _readingAvailable = true;
                ConnectionCompletion = new();
                TcpClient = new TcpClient();
                TcpClient.BeginConnect(address.Host, address.Port, OnClientConnected, TcpClient);
            }
        }

        public Task<bool> ConnectAsync(Uri address)
        {
            Connect(address);
            return ConnectionCompletion.Task;
        }

        public void Connect(TcpClient tcpClient)
        {
            if (tcpClient is null)
                throw new ArgumentNullException(nameof(tcpClient));

            if (disposed == true)
                throw new ObjectDisposedException(GetType().Name);

            lock (_locker)
            {
                _readingAvailable = true;
                ConnectionCompletion = new();
                TcpClient = tcpClient;
            }

            lock (_locker)
            {
                ConnectionEnd?.Invoke(this, new());
                ConnectionCompletion?.TrySetResult(IsConnected);
            }

            try
            {
                if (tcpClient.Client?.RemoteEndPoint is { } rep)
                    _debugTargetAddsess = rep.ToString();
            }
            catch { }

            HandleInputStream();
        }

        public virtual void Close()
        {
            lock (_locker)
            {
                try
                {
                    _readingAvailable = false;

                    if (TcpClient?.Connected == true)
                        TcpClient.Close();

                    ConnectionCompletion?.TrySetResult(IsConnected);
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
                try { SfaDebug.Print($"Connection faled! ({GetDebugTargetAddsess()})", GetType().Name); } catch { }
                return;
            }

            try { SfaDebug.Print($"Connected ({GetDebugTargetAddsess()})", GetType().Name); } catch { }

            lock (_locker)
            {
                ConnectionEnd?.Invoke(this, new());
                ConnectionCompletion?.SetResult(IsConnected);
            }

            HandleInputStream();
        }

        public bool CheckStatus()
        {
            if (_readingAvailable == false)
                return false;

            try
            {
                return TcpClient?.Client?.Connected ?? false;
            }
            catch { }

            return false;
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
                    if (TcpClient is TcpClient client &&
                        client?.GetStream() is NetworkStream stream)
                    {
                        while (IsConnected == true)
                        {
                            var header = MessagingHeader.ReadNext(stream);
                            buffer.Seek(0, SeekOrigin.Begin);

                            if (header.Method == MessagingMethod.Binary ||
                                header.Method == MessagingMethod.Text ||
                                header.Method == MessagingMethod.BinaryRequest ||
                                header.Method == MessagingMethod.TextRequest)
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

                            LastInput = DateTime.UtcNow;
                        }
                    }
                }
                catch (IOException){ }
                catch (Exception e)
                {
                    SfaDebug.Log(e.ToString());
                }
                finally
                {
                    _readingAvailable = false;
                    Close();
                    OnDisconnected();
                    try { SfaDebug.Print($"Disconnected ({GetDebugTargetAddsess()})", GetType().Name); } catch { }
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

        public virtual MessagingResponse SendRequest(string text) => SendRequestInternal(text);

        public virtual MessagingResponse SendRequest(byte[] bytes) => SendRequestInternal(bytes);

        protected virtual MessagingResponse SendRequestInternal(string text, Func<Guid, MessagingMethod, MessagingResponse> responceCreator = null)
        {
            lock (_requestsLockher)
            {
                var method = MessagingMethod.TextRequest;
                var requestId = Guid.NewGuid();
                var response =
                    responceCreator?.Invoke(requestId, method) ??
                    MessagingResponse.Create(requestId, method);

                _responses.TryAdd(requestId, response);
                SendInternal(new() { Method = method, RequestId = requestId }, text);
                return response;
            }
        }

        protected virtual MessagingResponse SendRequestInternal(ReadOnlyMemory<byte> data, Func<Guid, MessagingMethod, MessagingResponse> responceCreator = null)
        {
            lock (_requestsLockher)
            {
                var method = MessagingMethod.BinaryRequest;
                var requestId = Guid.NewGuid();
                var response =
                    responceCreator?.Invoke(requestId, method) ??
                    MessagingResponse.Create(requestId, method);

                _responses.TryAdd(requestId, response);
                SendInternal(new() { Method = method, RequestId = requestId }, data);
                return response;
            }
        }

        public void SendResponse(MessagingRequest request, string text)
        {
            var header = new MessagingHeader() { RequestId = request.Id, Method = MessagingMethod.TextRequest };
            SendInternal(header, text);
        }

        public void SendResponse(MessagingRequest request, ReadOnlyMemory<byte> data)
        {
            var header = new MessagingHeader() { RequestId = request.Id, Method = MessagingMethod.BinaryRequest };
            SendInternal(header, data);
        }

        public void SendResponse(MessagingRequest request, params ReadOnlyMemory<byte>[] packets)
        {
            var header = new MessagingHeader() { RequestId = request.Id, Method = MessagingMethod.BinaryRequest };
            SendInternal(header, packets);
        }

        public virtual void Send(string text) =>
            SendInternal(new() { Method = MessagingMethod.Text }, text);

        public virtual void Send(JsonNode node) =>
            SendInternal(new() { Method = MessagingMethod.Text }, node);

        public virtual void Send(byte[] bytes) =>
            SendInternal(new() { Method = MessagingMethod.Binary }, bytes);

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

        protected virtual void SendInternal(MessagingHeader header, ReadOnlyMemory<byte> bytes)
        {
            UseStream((s, bytes) =>
            {
                header.Length = bytes.Length;
                header.Write(s);
                s.Write(bytes.Span);
                s.Flush();
            }, bytes);
        }

        protected virtual void SendInternal(MessagingHeader header, params ReadOnlyMemory<byte>[] packets)
        {
            UseStream((s, packets) =>
            {
                header.Length = packets.Sum(p => p.Length);
                header.Write(s);

                for (int i = 0; i < packets.Length; i++)
                    s.Write(packets[i].Span);

                s.Flush();
            }, packets);
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

        private string GetDebugTargetAddsess()
        {
            try
            {
                return TcpClient?.Client?.RemoteEndPoint?.ToString() ?? _debugTargetAddsess?.ToString();
            }
            catch
            {
                return _debugTargetAddsess?.ToString();
            }
        }
    }
}
