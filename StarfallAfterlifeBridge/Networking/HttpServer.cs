using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public abstract class HttpServer : IServer, IDisposable
    {
        public event EventHandler<EventArgs> StateChanged;

        public bool IsStarted
        {
            get => isStarted;
            protected set
            {
                if (value != isStarted)
                {
                    isStarted = value;
                    StateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected HttpListener Listener { get; set; } = new HttpListener();

        public Uri Address
        {
            get => address;
            set
            {
                if (value != address)
                {
                    if (IsStarted)
                        Stop();

                    address = value;
                }
            }
        }

        private Uri address = null;
        private bool isStarted = false;

        public virtual void Start()
        {
            Stop();

            if (Address is null)
                return;

            if (address.Port == 0)
            {
                StartWithRandomPort();
                return;
            }

            if (IsStarted)
                Stop();

            Listener = StartNewListener(address);

            IsStarted = true;
            Listener.BeginGetContext(new AsyncCallback(ListenerCallback), Listener);
        }

        public virtual void Start(Uri address)
        {
            Address = address;
            Start();
        }

        public virtual void StartWithRandomPort()
        {
            if (IsStarted)
                Stop();

            if (Address is null)
                return;

            var builder = new UriBuilder(Address);
            var rnd = new Random();
            var maxPort = 49151;

            for (int i = 10000; i < maxPort; i++)
            {
                try
                {
                    builder.Port = rnd.Next(i, maxPort);
                    Listener = StartNewListener(builder.Uri);
                    break;
                }
                catch (HttpListenerException) { }
                catch (ObjectDisposedException) { }
            }

            Address = builder.Uri;
            IsStarted = true;
            Listener.BeginGetContext(new AsyncCallback(ListenerCallback), Listener);
        }

        protected virtual HttpListener StartNewListener(Uri address)
        {
            var listener = new HttpListener();

            listener.Prefixes.Add(address.ToString());
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();

            return listener;
        }

        public virtual void Stop()
        {
            if (IsStarted == false)
                return;

            IsStarted = false;
            Listener?.Stop();
            Listener?.Close();
        }

        public void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context;

            try
            {
                context = listener.EndGetContext(result);
            }
            catch
            {
                return;
            }
            
            listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);

            try
            {
                Task.Run(() =>
                {
                    HandleRequest(context);
                    context.Response.Close();
                }).ContinueWith(t => t.Dispose());
            }
            catch { }
        }

        public virtual void Send(HttpListenerContext context, string packet)
        {
            if (string.IsNullOrWhiteSpace(packet) == false)
            {
                Send(context, Encoding.UTF8.GetBytes(packet));
            }
        }

        public virtual void Send(HttpListenerContext context, byte[] packet)
        {
            if (packet is not null &&
                context.Response.OutputStream is Stream stream)
            {
                stream.Write(packet, 0, packet.Length);
                stream.Flush();
                context.Response.Close();
            }
        }

        protected abstract void HandleRequest(HttpListenerContext context);

        public void Dispose()
        {
            Stop();
        }
    }
}
