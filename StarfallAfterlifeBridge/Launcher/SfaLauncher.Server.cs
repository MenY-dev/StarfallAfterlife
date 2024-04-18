using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public SfaServer Server { get; protected set; }

        public SfaRealmInfo ServerRealm
        {
            get
            {
                if (_isServerRealmValueValid == true)
                    return _serverRealm;

                var id = (ServerSettings ??= new ServerSettings()).RealmId;
                _isServerRealmValueValid = true;
                return _serverRealm = Realms.ToArray().FirstOrDefault(r => r?.Realm?.Id == id);
            }
            set
            {
                _serverRealm = value;
                (ServerSettings ??= new ServerSettings()).RealmId = value?.Realm?.Id;
                SaveServerSettings();
            }
        }

        private SfaRealmInfo _serverRealm = null;

        private bool _isServerRealmValueValid = false;

        public string ServerAddress
        {
            get => (ServerSettings ??= new ServerSettings()).Address;
            set
            {
                (ServerSettings ??= new ServerSettings()).Address = value;
                SaveServerSettings();
            }
        }

        public ushort ServerPort
        {
            get => (ushort)(ServerSettings ??= new ServerSettings()).Port;
            set
            {
                (ServerSettings ??= new ServerSettings()).Port = value;
                SaveServerSettings();
            }
        }

        public bool ServerUsePassword
        {
            get => (ServerSettings ??= new ServerSettings()).UsePassword;
            set
            {
                (ServerSettings ??= new ServerSettings()).UsePassword = value;
                SaveServerSettings();
            }
        }

        public string ServerPassword
        {
            get => (ServerSettings ??= new ServerSettings()).Password;
            set
            {
                (ServerSettings ??= new ServerSettings()).Password = value;
                SaveServerSettings();
            }
        }

        public bool ServerUsePortForwarding
        {
            get => (ServerSettings ??= new ServerSettings()).UsePortForwarding;
            set
            {
                (ServerSettings ??= new ServerSettings()).UsePortForwarding = value;
                SaveServerSettings();
            }
        }

        public SfaServer StartServer()
        {
            StopServer();

            if (ServerRealm is SfaRealmInfo realm &&
                realm.CreateServer() is SfaServer server &&
                IPAddress.TryParse(ServerAddress, out IPAddress address) == true)
            {
                if (ActiveInstanceManager?.IsStarted != true)
                    StartInstanceManager();

                realm.Use(r =>
                {
                    r.LoadDatabase();
                    r.LoadVariable();
                });

                server.InstanceManagerAddress = ActiveInstanceManager.Address;
                server.Address = new Uri($"tcp://{address}:{ServerPort}");

                if (ServerPort > 0 &&
                    address.Equals(IPAddress.Loopback) == false &&
                    address.Equals(IPAddress.IPv6Loopback) == false)
                    server.UsePortForwarding = ServerUsePortForwarding;

                if (ServerUsePassword == true &&
                    ServerPassword is string password &&
                    string.IsNullOrWhiteSpace(password) == false)
                    server.Password = password;

                server.Start();
                Server = server;

                SfaDebug.Print($"DiscoveryServer Started! ({server.Address})");
                return server;
            }

            return null;
        }

        public void StopServer()
        {
            Server?.Stop();
        }
    }
}
