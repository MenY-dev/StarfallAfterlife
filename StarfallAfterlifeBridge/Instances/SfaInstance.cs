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

namespace StarfallAfterlife.Bridge.Instances
{
    public class SfaInstance
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

            Process.Exited += OnProcessExited;
        }

        public virtual bool Start()
        {
            Sandbox.InstanceConfig = CreateInstanceJsonString();
            State = InstanceState.Created;
            Process.Start();
            return true;
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
        }

        public virtual JsonNode CreateInstanceConfig()
        {
            if (Info is null)
                return new JsonObject();

            return new JsonObject
            {
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
            Port = Process?.GetServerListeningPort() ?? -1;

            if (Info?.UsePortForwarding == true)
                NatPuncher
                    .Create(ProtocolType.Udp, Port, Port, 7200)
                    .Wait(1000);

            State = InstanceState.Started;
        }

        public void OnAuthForInstanceReady(int charId, string auth)
        {
            Context?.SendInstanceAuthReady(this, charId, auth);
        }

        public virtual void ConnectChannels(InstanceChannelClient client)
        {

        }

        public virtual void StartShutdownTimer(int seconds)
        {
            lock (_shutdownCancellationLockher)
            {
                CancelShutdown();
                SfaDebug.Print($"StartShutdownTimer {seconds}s", GetType().Name);

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
                default: return null;
            }
        }
    }
}
