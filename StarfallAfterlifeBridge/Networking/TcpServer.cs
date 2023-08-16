using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public class TcpServer : IServer
    {
        public event EventHandler<TcpRequestEventArgs> ClientConnected;

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

        protected TcpListener Listener { get; set; }

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
            if (Address is null)
                return;

            Listener = new TcpListener(new IPEndPoint(
                IPAddress.Parse(Address.Host),
                Address.Port < 0 ? 0 : Address.Port));

            Listener.Start();

            var newUri = new UriBuilder(Address)
            {
                Port = (Listener.LocalEndpoint as IPEndPoint).Port
            };

            Address = newUri.Uri;
            IsStarted = true;

            Task.Run(() => AcceptClientsAsync());
        }

        public virtual void Start(Uri address)
        {
            Address = address;
            Start();
        }

        public void Stop()
        {
            if (IsStarted == false)
                return;

            Listener?.Stop();
            IsStarted = false;
        }

        protected virtual async Task AcceptClientsAsync()
        {
            while (IsStarted && Listener != null)
            {
                try
                {
                    TcpClient client = await Listener.AcceptTcpClientAsync();

                    Task task = Task.Run(() =>
                    {
                        OnClientConnected(new TcpRequestEventArgs(client));
                        HandleClient(client);
                    });
                }
                catch { }
            }
        }

        protected virtual void HandleClient(TcpClient client)
        {

        }

        protected virtual void OnClientConnected(TcpRequestEventArgs args)
        {
            ClientConnected?.Invoke(this, args);
        }
    }
}
