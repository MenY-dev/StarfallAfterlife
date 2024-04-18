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


        public bool ShowGameLog
        {
            get => (bool?)SettingsStorage["game_show_log"] ?? false;
            set
            {
                SettingsStorage["game_show_log"] = value;
                SaveSettings();
            }
        }

        public bool ForceGameWindowed
        {
            get => (bool?)SettingsStorage["game_force_windowed"] ?? false;
            set
            {
                SettingsStorage["game_force_windowed"] = value;
                SaveSettings();
            }
        }

        public bool HideGameLoadingScreen
        {
            get => (bool?)SettingsStorage["game_hide_loading_screen"] ?? false;
            set
            {
                SettingsStorage["game_hide_loading_screen"] = value;
                SaveSettings();
            }
        }

        public bool HideGameSplashScreen
        {
            get => (bool?)SettingsStorage["game_hide_splash_screen"] ?? false;
            set
            {
                SettingsStorage["game_hide_splash_screen"] = value;
                SaveSettings();
            }
        }

        protected string GameExeLocation => Path.Combine(GameDirectory, "Msk", "starfall_game", "Starfall.exe");

        public Task<SfaSession> StartGame(SfaProfile profile, string serverAddress, Func<string> passwordRequest = null) =>
            StartGame(profile, new Uri($"tcp://{serverAddress}"), passwordRequest);

        public Task<SfaSession> StartGame(SfaProfile profile, Uri serverAddress, Func<string> passwordRequest = null)
        {
            return CreateGame(profile, passwordRequest).StartGame(serverAddress);
        }

        public SfaSession CreateGame(SfaProfile profile, Func<string> passwordRequest = null)
        {
            var session = new SfaSession()
            {
                Profile = profile,
                PasswordRequested = passwordRequest,
                GameDirectory = GameDirectory,
            };

            session.Game.ShowLog = ShowGameLog;
            session.Game.ForceWindowed = ForceGameWindowed;
            session.Game.HideSplashScreen = HideGameLoadingScreen;
            session.Game.HideLoadingScreen = HideGameSplashScreen;

            return session;
        }

        public SfaSession StartLocalGame() => StartLocalGame(CurrentProfile);

        public SfaSession StartLocalGame(SfaProfile profile)
        {
            if (profile is not null &&
                CurrentLocalRealm is SfaRealmInfo realm &&
                realm.CreateServer() is SfaServer server)
            {
                if (ActiveInstanceManager?.IsStarted != true)
                    StartInstanceManager();

                realm.Use(r =>
                {
                    r.LoadDatabase();
                    r.LoadVariable();
                });

                server.InstanceManagerAddress = ActiveInstanceManager.Address;
                server.UsePortForwarding = false;
                server.Start();

                SfaDebug.Print($"DiscoveryServer Started! ({server.Address})");

                var session = CreateGame(profile);

                session.StartGame(server.Address).ContinueWith(task =>
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
