using StarfallAfterlife.Bridge.Environment;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.Diagnostics;
using System.Threading;
using System.Text.Json.Nodes;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace StarfallAfterlife.Bridge.Instances
{
    public partial class SfaInstance
    {
        public InstanceState State
        {
            get => _state;
            set
            {
                if (value != _state)
                {
                    _state = value;
                    Context?.SendInstanceState(this);
                }
            }
        }

        public InstanceInfo Info { get; set; }

        public string Auth { get; set; }

        public int InstanceId { get; set; }

        public int SystemId { get; set; }

        public string Directory { get; set; }

        public string Address { get; set; } = "0.0.0.0";

        public int Port { get; set; }

        public bool PortFinded { get; protected set; } = false;

        public SfaProcess Process { get; protected set; }

        public SfaProcessSandbox Sandbox { get; protected set; }

        public virtual string Map => string.Empty;

        public List<InstanceCharacter> Characters { get; } = new();

        public InstanceManagerServerClient Context { get; protected set; }

        private InstanceState _state = InstanceState.None;

        private CancellationTokenSource _shutdownCancellation;
        private readonly object _shutdownCancellationLockher = new();

        public virtual void Init(InstanceManagerServerClient context)
        {
            Context = context;

            Sandbox = new SfaProcessSandbox()
            {
                WorkingDirectory = Directory,
                GameIni = new()
                {
                    new(@"/script/engine.gamenetworkmanager")
                    {
                        "MAXPOSITIONERRORSQUARED=32.00f",
                        "MAXCLIENTUPDATEINTERVAL=0.8f",
                        "MaxMoveDeltaTime=0.4f",
                        "ClientNetSendMoveDeltaTime=0,0665f",
                        "CLIENTADJUSTUPDATECOST=256.0f",
                        "MaxClientForcedUpdateDuration=1.0f",
                        "MoveRepSize=128",
                        "ServerForcedUpdateHitchThreshold=5.0f"
                    },
                    new("DefaultPlayer")
                    {
                        "Name=Server" + Auth,
                    },
                },
                EngineIni = new()
                {
                    new(@"CrashReportClient")
                    {
                        "bAgreeToCrashUpload=false",
                        "bImplicitSend=false",
                    },
                    new(@"Engine.ErrorHandling")
                    {
                        "bPromptForRemoteDebugging=false",
                        "bPromptForRemoteDebugOnEnsure=false",
                    },
                    new(@"/script/onlinesubsystemutils.ipnetdriver")
                    {
                        "LanServerMaxTickRate=15",
                        "NetServerMaxTickRate=15",
                    },
                    new(@"/script/onlinesubsystemutils.ipnetdriver")
                    {
                        "bSmoothFrameRate=true",
                        "bUseFixedFrameRate=false",
                        "SmoothedFrameRateRange=(LowerBound=(Type=Inclusive,Value=5.000000),UpperBound=(Type=Exclusive,Value=15.000000))",
                        "MinDesiredFrameRate=5",
                        "FixedFrameRate=15",
                        "NetClientTicksPerSecond=15",
                    },
                    new(@"/script/engine.networksettings")
                    {
                        "net.MaxRepArraySize=65535",
                        "net.MaxRepArrayMemory=65535",
                    },
                },
            };

            Process = new SfaProcess()
            {
                Sandbox = Sandbox,
                Executable = context.Manager.GameExeLocation,
                Listen = true,
                Map = "/Game/Maps/" + Map,
                Windowed = true,
                EnableLog = true,
                DisableSplashScreen = true,
                DisableLoadingScreen = true,
                ProcessArguments = new() { "CULTUREFORCOOKING=en" },
                ConsoleCommands =
                {
                    "DebugRemovePlayer 0"
                }
            };

            Process.OutputUpdated += OnProcessOutputUpdated;
            Process.Exited += OnProcessExited;
        }

        public virtual bool Start()
        {
            try
            {
                Sandbox.InstanceConfig = CreateInstanceJsonString();
                State = InstanceState.Created;
                Process.Start();
                SfaDebug.Print($"Instance started! (Map = {Map}, Type = {Info?.Type}, DungeonType = {Info?.DungeonType}, DungeonFaction = {Info?.DungeonFaction}, InstanceAuth = {Auth})", GetType().Name);
                SfaDebug.Print($"Instance players: (InstanceAuth = {Auth}, Players = {string.Join(", ", Info?.Players.ToArray().Select(p => p.Name) ?? [])})", GetType().Name);
                SfaDebug.Print($"Instance chars: (InstanceAuth = {Auth}, Chars = {string.Join(", ", Info?.Characters.ToArray().Select(c => c.Name) ?? [])})", GetType().Name);
                return true;
            }
            catch (Exception e)
            {
                SfaDebug.Log(e.ToString());
                Stop();
                return false;
            }
        }


        public virtual void Stop()
        {
            if (State != InstanceState.Finished)
                State = InstanceState.Finished;

            try
            {
                Process?.Close();
            }
            catch { }

            SfaDebug.Print($"Instance finished! (Map = {Map}, InstanceAuth = {Auth})", GetType().Name);
        }

        public virtual JsonNode CreateInstanceConfig()
        {
            if (Info is null)
                return new JsonObject();

            return new JsonObject
            {
                ["map"] = Map,
                ["sfmgr"] = new JsonObject
                {
                    ["url"] = Context.MgrServer?.Address?.ToString(),
                    ["auth"] = Auth
                },
                ["realmmgr"] = new JsonObject
                {
                    ["url"] = Context.MgrServer?.Address?.ToString(),
                    ["auth"] = Auth
                },
                ["galaxymgr"] = new JsonObject
                {
                    ["address"] = Context.GalaxyMgrChannelManager?.Address?.Host,
                    ["port"] = Context.GalaxyMgrChannelManager?.Address?.Port,
                    ["instanceid"] = InstanceId,
                    ["systemid"] = SystemId,
                    ["auth"] = Auth
                },
                ["game_mode"] = "",
                ["is_custom_game"] = 0,
                ["players_list"] = JsonHelpers.ParseNodeUnbuffered(Info.Players ?? new()),
                ["characters_list"] = JsonHelpers.ParseNodeUnbuffered(Info.Characters ?? new()),
            };
        }

        public string CreateInstanceJsonString() =>
            CreateInstanceConfig()?.ToJsonString(true) ?? string.Empty;

        public virtual void SetInstanceReady()
        {
            try
            {
                if (PortFinded == false &&
                    Process?.GetServerListeningPort() is int port &&
                    port > 0)
                {
                    Port = port;
                    PortFinded = true;
                }
            }
            catch (Exception e)
            {
                SfaDebug.Print($"Error getting listening port! (Map = {Map}, InstanceAuth = {Auth}, Error = {e})", GetType().Name);
            }

            if (Info?.UsePortForwarding == true)
            {
                SfaDebug.Print($"Punch instance port! (Port = {Port}, Map = {Map}, InstanceAuth = {Auth})", GetType().Name);

                try
                {
                    NatPuncher
                        .Create(ProtocolType.Udp, Port, Port, 7200)
                        .Wait(1000);
                }
                catch (Exception e)
                {
                    SfaDebug.Print($"Instance port punching error! (Port = {Port}, Map = {Map}, InstanceAuth = {Auth}, Error = {e})", GetType().Name);
                }
            }

            State = InstanceState.Started;
            SfaDebug.Print($"Instance ready! (Port = {Port}, Map = {Map}, InstanceAuth = {Auth})", GetType().Name);
        }

        public void OnAuthForInstanceReady(int charId, string auth)
        {
            Context?.SendInstanceAuthReady(this, charId, auth);
            SfaDebug.Print($"Auth ready! (CharId = {charId}, CharAuth = {auth}, InstanceAuth = {Auth})", GetType().Name);
        }

        public virtual void ConnectChannels(InstanceChannelClient client)
        {

        }

        public virtual void StartShutdownTimer(int seconds)
        {
            lock (_shutdownCancellationLockher)
            {
                CancelShutdown();
                SfaDebug.Print($"Start shutdown timer {seconds}s (InstanceAuth = {Auth})", GetType().Name);

                _shutdownCancellation = new();

                Task.Delay(TimeSpan.FromSeconds(seconds), _shutdownCancellation.Token).ContinueWith(t =>
                {
                    lock (_shutdownCancellationLockher)
                    {
                        if (t.IsCanceled == false &&
                            t.IsFaulted == false)
                        {
                            Stop();
                        }
                    }
                });
            }
        }

        public virtual void CancelShutdown()
        {
            lock (_shutdownCancellationLockher)
            {
                try
                {
                    _shutdownCancellation?.Cancel();

                    if (_shutdownCancellation is CancellationTokenSource cts &&
                        cts.IsCancellationRequested == false)
                    {
                        SfaDebug.Print($"Cancel Shutdown", GetType().Name);
                        cts.Cancel();
                        cts.Dispose();
                    }
                }
                catch { }
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            State = InstanceState.Finished;
        }

        private void OnProcessOutputUpdated(object sender, SfaProcessOutputEventArgs e)
        {
            if (PortFinded == false)
            {
                try
                {
                    if (ListeningOnPortRegex().Matches(e.Line) is { } matches && matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Groups["port"].Value is string portString &&
                                int.TryParse(portString, out var port) == true)
                            {
                                Port = port;
                                PortFinded = true;
                                SfaDebug.Print($"Get port from stdout! (Port = {port}, InstanceAuth = {Auth})", GetType().Name);
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
            
            try
            {
                if (MatchStateChangedRegex().Matches(e.Line) is { } matches && matches.Count > 0)
                    foreach (Match match in matches)
                        OnNewMatchState(match.Groups["from"].Value, match.Groups["to"].Value);
            }
            catch { }
        }

        [GeneratedRegex(@"[M]atch State Changed from (?<from>.+) to (?<to>.+)")]
        private static partial Regex MatchStateChangedRegex();

        [GeneratedRegex(@"[l]istening on port (?<port>\d+)")]
        private static partial Regex ListeningOnPortRegex();

        private void OnNewMatchState(string oldState, string newState)
        {
            if (newState == "WaitingToStart")
                StartShutdownTimer(120);

            if (newState == "InProgress")
                CancelShutdown();

            SfaDebug.Print($"New match state (OldState = {oldState}, NewState = {newState}, InstanceAuth = {Auth})", GetType().Name);
        }

        public static SfaInstance Create(InstanceInfo info)
        {
            switch (info?.Type)
            {
                case InstanceType.DiscoveryBattle: return new DiscoveryBattleInstance() { Info = info };
                case InstanceType.DiscoveryDungeon: return new DiscoveryDungeonInstance() { Info = info };
                case InstanceType.MothershipAssault: return new MothershipAssaultInstanse() { Info = info };
                case InstanceType.StationAttack: return new StationAttackInstance() { Info = info };
                case InstanceType.SurvivalMode: return new SurvivalModeInstance() { Info = info };
                case InstanceType.RankedMode: return new RankedInstance() { Info = info };
                case InstanceType.CaravanTutorial: return new CaravanTutorInstanse() { Info = info };
                default: return null;
            }
        }
    }
}
