using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public SfaRealmInfo CurrentLocalRealm
        {
            get => _currentLocalRealm;
            set
            {
                _currentLocalRealm = value;
                LastSelectedLocalRealmId = value?.Realm?.Id;
                SaveSettings();
            }
        }

        protected string GameExeLocation => Path.Combine(GameDirectory, "Msk", "starfall_game", "Starfall", "Binaries", "Win64", "Starfall.exe");

        public SfaSession StartGame(SfaProfile profile, string serverAddress) =>
            StartGame(profile, new Uri($"tcp://{serverAddress}"));

        public SfaSession StartGame(SfaProfile profile, Uri serverAddress)
        {
            var session = new SfaSession()
            {
                Profile = profile,
            };

            session.StartGame(GameDirectory, serverAddress);
            return session;
        }

        public SfaSession StartLocalGame() => StartLocalGame(CurrentProfile);

        public SfaSession StartLocalGame(SfaProfile profile)
        {
            if (profile is not null &&
                CurrentLocalRealm is SfaRealmInfo realm &&
                realm.Realm.CreateServer() is SfaServer server)
            {
                if (ActiveInstanceManager?.IsStarted != true)
                    StartInstanceManager();

                realm.LoadDatabase();
                server.InstanceManagerAddress = ActiveInstanceManager.Address;
                server.Start();

                SfaDebug.Print($"DiscoveryServer Started! ({server.Address})");

                var session = new SfaSession()
                {
                    Profile = profile,
                };

                session.StartGame(GameDirectory, server.Address).ContinueWith(task =>
                {
                    server.Stop();
                    SfaDebug.Print($"Game stopped!");
                });

                return session;
            }

            return null;
        }
    }
}
