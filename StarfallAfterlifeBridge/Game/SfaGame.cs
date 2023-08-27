using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Environment;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Networking.MgrHandlers;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Diagnostics.SfaDebug;
using System.Net.Http;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Diagnostics;
using System.Text.Json.Nodes;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame : IDisposable
    {
        public string Location { get; set; }

        public Uri ServerAddress { get; set; }

        public SfaProfile Profile { get; set; }

        public SfaGameProfile GameProfile => Profile?.GameProfile;

        public SfaRealm Realm => Profile.CurrentRealm?.Realm;

        protected string ExeLocation => Path.Combine(Location, "Msk", "starfall_game", "Starfall", "Binaries", "Win64", "Starfall.exe");

        protected string LogsLocation => Path.Combine(Location, "Msk", "starfall_game", "Starfall", "Saved", "Logs");

        protected static JsonNode EmptyMgrResponse => new JsonObject { ["ok"] = 1 };

        public SfaClient SfaClient { get; protected set; }

        public SfaProcess Process { get; protected set; }

        public Task Task => CompletionSource?.Task ?? Task.CompletedTask;

        protected TaskCompletionSource CompletionSource { get; set; }

        public class LogReceivedEventArgs : EventArgs
        {
            public string Message { get;}

            public LogReceivedEventArgs(string message)
            {
                Message = message;
            }
        }

        public Task<bool> Start()
        {
            Stop();
            Init();

            //GameProfile.CurrentCharacter.Database = Realm.Database;

            GameProfile.Database = Profile.Database;

            RealmMgrHandler = new RealmMgrHandler(GameProfile);

            SfMgrChannelManager.Start(new Uri("tcp://127.0.0.1:0"));
            Print($"SfmgrChannelManager Started! ({SfMgrChannelManager.Address})");

            SfMgrServer.Start(new Uri("http://127.0.0.1:0/sfmgr/"));
            Print($"SfMgrServer Started! ({SfMgrServer.Address})");

            RealmMgrChannelManager.Start(new Uri("tcp://127.0.0.1:0"));
            Print($"RealmMgrChannelManager Started! ({RealmMgrChannelManager.Address})");

            RealmMgrServer.Start(new Uri("http://127.0.0.1:0/realmmgr/"));
            Print($"RealmMgrServer Started! ({RealmMgrServer.Address})");

            Process = new SfaProcess
            {
                Executable = ExeLocation,
                MgrUrl = SfMgrServer.Address.ToString(),
                Username = GameProfile.Nickname,
                Auth = GameProfile.TemporaryPass,
                EnableLog = true,
                Windowed = true,
                DisableSplashScreen = true,
                DisableLoadingScreen = true
            };

            Process.Exited += OnProcessExited;

            SfaClient = new SfaClient(this);
            CompletionSource = new TaskCompletionSource();

            var startingTask = new Task<bool>(() =>
            {
                SfaDebug.Print($"Connecting... ({ServerAddress})", GetType().Name);
                SfaClient.ConnectAsync(ServerAddress).Wait();
                SfaDebug.Print("Connected to server!", GetType().Name);

                if (SfaClient.Auth(GameProfile).Result == false)
                {
                    SfaDebug.Print("Auth Error", GetType().Name);
                    return false;
                }

                if (Realm.GalaxyMapCache is null &&
                    SfaClient.LoadGalaxyMap().Result == false)
                {

                    SfaDebug.Print("LoadGalaxyMap Error", GetType().Name);
                    return false;
                }

                if (SfaClient.SyncPlayerData().Result == false)
                {

                    SfaDebug.Print("SyncPlayerData Error", GetType().Name);
                    return false;
                }

                Process.Start();
                return true;
            }, TaskCreationOptions.LongRunning);

            startingTask.ContinueWith(t =>
            {
                if (t.Result == false)
                    CompletionSource?.TrySetResult();
            });

            startingTask.Start();
            return startingTask;
        }

        public void Stop()
        {
            try
            {
                //Process?.Close();
                SfaClient?.Close();
                SfMgrChannelManager?.Stop();
                SfMgrServer?.Stop();
                RealmMgrChannelManager?.Stop();
                RealmMgrServer?.Stop();
            }
            catch { }

            CompletionSource?.TrySetResult();
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            if (sender is SfaProcess process)
                process.Exited -= OnProcessExited;

            Stop();
        }

        protected void OnProductionPointsCap(object state)
        {
            if (GameProfile?.CurrentCharacter is Character character &&
                SfaClient.IsConnected == true &&
                MatchmakerChannel?.Client is not null)
            {
                AddProductionPoints(1);
                MatchmakerChannel.RequestDataUpdate();
            }
        }

        protected virtual void Init()
        {
            InitSfMgr();
            InitRealmMgr();
        }

        public void Dispose()
        {
            SfaClient?.Dispose();
            SfaClient = null;
            Stop();
        }
    }
}
