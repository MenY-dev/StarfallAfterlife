using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingServer : MessagingServer<MessagingClient> {}

    public class MessagingServer<TClient> : IServer where TClient : MessagingClient, new()
    {
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

        public event EventHandler<EventArgs> StateChanged;

        public virtual event EventHandler<MessagingClientEventArgs> ClientConnected;

        public virtual event EventHandler<MessagingClientEventArgs> ClientDisconnected;

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

        protected TcpListener Listener { get; set; }

        private Uri address = null;
        private bool isStarted = false;

        public virtual void Start()
        {
            if (Address is null)
                Address = new Uri("tcp://127.0.0.1:0");

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
            Listener.BeginAcceptTcpClient(ConnectNewClient, Listener);
        }

        public void Start(Uri address)
        {
            Address = address;
            Start();
        }

        public virtual void Stop()
        {
            if (IsStarted == false)
                return;

            Listener?.Stop();
            IsStarted = false;
        }

        protected virtual TClient CreateNewClient()
        {
            return new();
        }

        protected void ConnectNewClient(IAsyncResult ar)
        {
            try
            {
                TcpListener listener = ar.AsyncState as TcpListener;
                TcpClient client = listener?.EndAcceptTcpClient(ar);

                listener?.BeginAcceptTcpClient(ConnectNewClient, listener);

                if (CreateNewClient() is TClient messagingClient)
                {
                    messagingClient.ConnectionEnd += OnClientConnectedInternal;
                    messagingClient.Disconnected += OnClientDisconnectedInternal;
                    messagingClient.Connect(client);
                }
            }
            catch { }
        }

        protected virtual void HandleNewClient(TClient client)
        {

        }

        protected virtual void HandleClientDisconnect(TClient client)
        {

        }

        private void OnClientConnectedInternal(object sender, EventArgs e)
        {
            HandleNewClient(sender as TClient);
            ClientConnected?.Invoke(this, new(sender as TClient));
        }

        private void OnClientDisconnectedInternal(object sender, EventArgs e)
        {
            TClient client = sender as TClient;
            client.ConnectionEnd -= OnClientConnectedInternal;
            client.Disconnected -= OnClientDisconnectedInternal;
            HandleClientDisconnect(client);
            ClientDisconnected?.Invoke(this, new(client));
        }
    }
}
