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
        public SfaRealmInfo CurrentServerRealm
        {
            get => _currentServerRealm;
            set
            {
                _currentServerRealm = value;
                LastSelectedServerRealmId = value?.Realm?.Id;
                SaveSettings();
            }
        }

        public SfaServer Server { get; protected set; }

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                _serverAddress = value;
                SaveSettings();
            }
        }

        public ushort ServerPort
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                SaveSettings();
            }
        }

        private SfaRealmInfo _currentServerRealm;
        private ushort _serverPort = 50200;
        private string _serverAddress = "0.0.0.0";

        public SfaServer StartServer()
        {
            StopServer();

            if (CurrentServerRealm is SfaRealmInfo realm &&
                realm.Realm.CreateServer() is SfaServer server &&
                IPAddress.TryParse(ServerAddress, out IPAddress address) == true)
            {
                if (ActiveInstanceManager?.IsStarted != true)
                    StartInstanceManager();

                realm.LoadDatabase();
                server.InstanceManagerAddress = ActiveInstanceManager.Address;
                server.Address = new Uri($"tcp://{address}:{ServerPort}");
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
