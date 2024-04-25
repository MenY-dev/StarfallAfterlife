using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Environment;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
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
using System.Net.Http;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Globalization;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame : IDisposable
    {
        public string Location { get; set; }

        public Uri ServerAddress { get; set; }

        public string ServerPassword { get; set; }

        public Func<string> PasswordRequested { get; set; }

        public SfaProfile Profile { get; set; }

        public SfaGameProfile GameProfile => Profile?.GameProfile;

        public SfaRealm Realm => Profile.CurrentRealm?.Realm;

        protected string ExeLocation => Path.Combine(Location, "Msk", "starfall_game", "Starfall.exe");

        protected string LogsLocation => Path.Combine(Location, "Msk", "starfall_game", "Starfall", "Saved", "Logs");

        protected string GameUserSettingsLocation => Path.Combine(
            Location, "Msk", "starfall_game", "Starfall", "Saved",
            "Config", "WindowsNoEditor", "GameUserSettings.ini");

        protected static JsonNode EmptyMgrResponse => new JsonObject { ["ok"] = 1 };

        public SfaClient SfaClient { get; protected set; }

        public SfaProcess Process { get; protected set; }

        public bool ShowLog { get; set; } 

        public bool ForceWindowed { get; set; }

        public bool HideLoadingScreen { get; set; }

        public bool HideSplashScreen { get; set; }

        public Task<StartResult> StartingTask => CompletionSource?.Task ?? Task.FromResult<StartResult>(new(false, null));

        protected TaskCompletionSource<StartResult> CompletionSource { get; set; }

        public record struct StartResult(bool IsSucces, string Reason);

        public Task<StartResult> Start(IProgress<string> progress = null)
        {
            progress?.Report("init");

            Stop();
            Init();

            //GameProfile.CurrentCharacter.Database = Realm.Database;

            GameProfile.Database = Profile.Database;

            progress?.Report("start_sfmgr");

            SfMgrChannelManager.Start(new Uri("tcp://127.0.0.1:0"));
            SfaDebug.Print($"SfmgrChannelManager Started! ({SfMgrChannelManager.Address})");

            SfMgrServer.Start(new Uri("http://127.0.0.1:0/sfmgr/"));
            SfaDebug.Print($"SfMgrServer Started! ({SfMgrServer.Address})");

            progress?.Report("start_realmmgr");

            RealmMgrChannelManager.Start(new Uri("tcp://127.0.0.1:0"));
            SfaDebug.Print($"RealmMgrChannelManager Started! ({RealmMgrChannelManager.Address})");

            RealmMgrServer.Start(new Uri("http://127.0.0.1:0/realmmgr/"));
            SfaDebug.Print($"RealmMgrServer Started! ({RealmMgrServer.Address})");

            string locale = null;

            try
            {
                if (File.Exists(GameUserSettingsLocation) == true)
                {
                    var text = File.ReadAllText(GameUserSettingsLocation);

                    if (Regex.IsMatch(text, @"\[Internationalization\][^\[]*?Language\s*?\=",
                        RegexOptions.Multiline | RegexOptions.IgnoreCase))
                    {
                        if (Regex.IsMatch(
                            text,
                            @"\[Internationalization\][^\[]*?Language\s*?\=[^\[\r\n]*?ru(?>\-|\s+)",
                            RegexOptions.Multiline | RegexOptions.IgnoreCase))
                            locale = "ru";
                    }
                }
            }
            catch { }

            if (locale is null &&
                CultureInfo.CurrentCulture.Name is string culture &&
                Regex.IsMatch(culture, @"^ru(?>$|\-)", RegexOptions.IgnoreCase))
            {
                locale = "ru";
            }

            SfaClient = new SfaClient(this) { Localization = locale };
            CompletionSource = new TaskCompletionSource<StartResult>();

            (bool IsSucces, string Reason) Auth(string lastAuth = null)
            {
                var result = SfaClient.Auth(GameProfile, ServerPassword, lastAuth).Result;

                if (result.IsSucces == true)
                    return (true, null);

                if (result.Reason is "bad_password")
                {
                    ServerPassword = PasswordRequested?.Invoke();

                    if (ServerPassword == null)
                        return new(false, "auth_cancelled");

                    return SfaClient.Auth(GameProfile, ServerPassword).Result;
                }

                return result;
            }

            var startingTask = new Task<StartResult>(() =>
            {
                progress?.Report("connecting");

                SfaDebug.Print($"Connecting... ({ServerAddress})", GetType().Name);
                SfaClient.ConnectAsync(ServerAddress).Wait();
                SfaDebug.Print("Connected to server!", GetType().Name);

                progress?.Report("auth");

                if (Realm?.LastAuth is string lastAuth &&
                    Auth(lastAuth).IsSucces == true)
                {
                    SfaDebug.Print("Restore Session: Done!", GetType().Name);
                }
                else if (Auth() is var result && result.IsSucces == false)
                {
                    SfaDebug.Print("Auth Error", GetType().Name);
                    return new(result.IsSucces, result.Reason);
                }

                progress?.Report("load_galaxy");

                if (SfaClient.GalaxyHash is string galaxyHash &&
                    Profile.MapsCache.LoadText(galaxyHash) is string galaxyCache)
                {
                    Realm.GalaxyMapHash = galaxyHash;
                    Realm.GalaxyMapCache = galaxyCache;
                }
                else if (SfaClient.LoadGalaxyMap().Result == false)
                {
                    SfaDebug.Print("LoadGalaxyMap Error", GetType().Name);
                    return new(false, "load_map_error");
                }

                progress?.Report("sync_player_data");

                if (SfaClient.SyncPlayerData().Result == false)
                {
                    SfaDebug.Print("SyncPlayerData Error", GetType().Name);
                    return new(false, "sync_player_data_error");
                }

#if DEBUG
                ShowLog = true;
                ForceWindowed = true;
                HideSplashScreen = true;
                HideLoadingScreen = true;
#endif

                var process = Process = new SfaProcess
                {
                    Executable = ExeLocation,
                    MgrUrl = SfMgrServer.Address.ToString(),
                    Username = GameProfile.Nickname,
                    Auth = GameProfile.TemporaryPass,

                    EnableLog = ShowLog,
                    Windowed = ForceWindowed,
                    DisableSplashScreen = HideSplashScreen,
                    DisableLoadingScreen = HideLoadingScreen,
                };

                Process.Exited += OnProcessExited;

                progress?.Report("start_game");
                process.Start();

                return new(true, null);
            }, TaskCreationOptions.LongRunning);

            startingTask.ContinueWith(t =>
            {
                if (t.Result.IsSucces == false)
                    CompletionSource?.TrySetResult(new(false, t.Result.Reason));

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

            CompletionSource?.TrySetResult(new(true, null));
        }

        public GameChannel GetChannel(string name)
        {
            return SfMgrChannelManager?[name] ?? RealmMgrChannelManager?[name];
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
