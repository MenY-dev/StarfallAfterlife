using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CreateServerPageViewModel : ViewModelBase
    {
        public AppViewModel AppVM { get; }

        public SfaLauncher Launcher => AppVM?.Launcher;

        public SfaServer Server { get; protected set; }

        public ObservableCollection<PlayerStatusInfoViewModel> Players { get; } = new();

        public bool ServerStarted
        {
            get => _serverStarted;
            set => SetAndRaise(ref _serverStarted, value);
        }

        public string ServerAddress
        {
            get => Launcher?.ServerAddress;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.ServerAddress, value, v => launcher.ServerAddress = v);
            }
        }

        public ushort ServerPort
        {
            get => Launcher?.ServerPort ?? 0;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.ServerPort, value, v => launcher.ServerPort = v);
            }
        }

        public bool UsePassword
        {
            get => Launcher?.ServerUsePassword ?? false;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.ServerUsePassword, value, v => launcher.ServerUsePassword = v);
            }
        }

        public bool UsePortForwarding
        {
            get => Launcher?.ServerUsePortForwarding ?? false;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.ServerUsePortForwarding, value, v => launcher.ServerUsePortForwarding = v);
            }
        }

        public ObservableCollection<InterfaceInfo> Interfaces { get; } = new();

        private bool _serverStarted;

        private BattlegroundsEditorWindow _battlegroundsEditor;

        public CreateServerPageViewModel(AppViewModel mainWindowViewModel)
        {
            AppVM = mainWindowViewModel;
            UpdateInterfaces();
        }

        public void UpdateInterfaces()
        {
            var result = new List<InterfaceInfo>();

            result.AddRange(NetUtils.GetInterfaces().Select(i => new InterfaceInfo()
            {
                Name = i.Name,
                Address = i.Address
            }));

            if (result.FirstOrDefault(i => i.Address.Equals(IPAddress.Loopback)) is InterfaceInfo info)
                info.Name = "Localhost";
            else
                result.Add(new() { Address = IPAddress.Loopback, Name = "Localhost" });

            result.Add(new() { Address = IPAddress.Any, Name = "Any" });

            Interfaces.Clear();

            foreach (var item in result)
                Interfaces.Add(item);
        }

        public void SelectInterface(object info)
        {
            if (info is InterfaceInfo interfaceInfo)
            {
                ServerAddress = interfaceInfo.Address?.ToString();
            }
        }

        public void ShowPasswordDialog(object info)
        {
            if (Launcher is SfaLauncher launcher)
            {
                var dialog = new EnterPasswordDialog();

                dialog.ShowDialog().ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    if (dialog.IsDone == true &&
                        dialog.Text is string password &&
                        string.IsNullOrWhiteSpace(password) == false)
                    {
                        launcher.ServerPassword = SfaServer.CreatePasswordHash(password);
                    }
                }));
            }
        }


        public void StartServer()
        {
            if (Launcher is SfaLauncher launcher &&
                AppVM is AppViewModel appVM)
            {
                if (Server?.IsStarted == true)
                    return;

                var startingPopup = new SFAWaitingPopup() { Title = App.GetString("s_page_cs_dialog_starting") };
                startingPopup.ShowDialog();

                Task.Factory.StartNew(() =>
                {
                    if (appVM.MakeBaseTests(true, false).Result == false)
                    {
                        Dispatcher.UIThread?.InvokeAsync(() => startingPopup.Close());
                        return;
                    }

                    if (launcher.Realms.Count < 1)
                        Dispatcher.UIThread.InvokeAsync(() => CreateNewRealm())?.Wait();

                    if (launcher.Realms.Count < 1)
                    {
                        Dispatcher.UIThread?.InvokeAsync(() => startingPopup.Close());
                        return;
                    }

                    appVM.SelectedServerRealm ??= appVM.Realms.FirstOrDefault();
                    Dispatcher.UIThread?.InvokeAsync(() => Players.Clear()).Wait();

                    var server = Server = launcher.StartServer();

                    Dispatcher.UIThread?.InvokeAsync(() => startingPopup.Close());

                    if (server is not null)
                    {
                        server.PlayerStatusUpdated += PlayerStatusUpdated;
                        Dispatcher.UIThread?.InvokeAsync(() =>
                        {
                            ServerStarted = true;
                            appVM.UpdateRealms();
                        }).Wait();

                        server.Task.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                        {
                            ServerStarted = false;
                            appVM.UpdateRealms();
                        }));
                    }
                });
            }
        }

        public void ConnectToServer()
        {
            var gameLoadingPopup = new SfaMessageBox()
            {
                Title = App.GetString("s_dialog_game_starting_title"),
                Text = App.GetString("s_dialog_game_starting_loading"),
                Buttons = MessageBoxButton.Undefined,
            };

            gameLoadingPopup.ShowDialog();

            if (ServerStarted == true &&
                Launcher is SfaLauncher launcher &&
                Server is SfaServer server &&
                AppVM is AppViewModel appVM &&
                launcher.CurrentProfile is SfaProfile profile &&
                IPAddress.TryParse(ServerAddress, out var address) == true)
            {
                Task.Factory.StartNew(() =>
                {
                    if (appVM.ProcessSessionsCancellationBeforePlay(server.Realm?.Id).Result == false)
                        return;

                    if (appVM.MakeBaseTests(true, true).Result == false)
                        return;

                    try
                    {
                        if (address.Equals(IPAddress.Any) == true)
                        {
                            address = null;

                            Task.Factory.StartNew(() =>
                            {
                                IPAddress newAddress = null;
                                AutoResetEvent monitor = new(true);
                                object locker = new();

                                try
                                {
                                    monitor.Reset();

                                    var interfaces = Interfaces
                                        .ToArray()
                                        .Where(i => i is not null &&
                                               i.Address is not null &&
                                               IPAddress.Any.Equals(i.Address) == false);

                                    foreach (var item in interfaces)
                                    {
                                        SfaClient
                                            .GetServerInfo(
                                                new IPEndPoint(item.Address, ServerPort).ToString(), 4000)
                                            .ContinueWith(t =>
                                            {
                                                if (t.IsCanceled == true || t.Result is null)
                                                    return;


                                                lock (locker)
                                                {
                                                    if (newAddress is null)
                                                    {
                                                        newAddress = item.Address;
                                                        monitor.Set();
                                                    }
                                                }
                                            });
                                    }
                                }
                                catch
                                {
                                    monitor.Set();
                                }

                                monitor.WaitOne(4000);
                                address = newAddress;
                            }).ContinueWith(t =>
                            {
                                Dispatcher.UIThread.Invoke(() => gameLoadingPopup?.Close());

                                if (address is null)
                                    return;

                                appVM.StartGame(profile, new IPEndPoint(address, ServerPort).ToString(), () => server.Password);
                            });
                        }
                    }
                    catch { }
                });
            }
        }

        private void PlayerStatusUpdated(object sender, PlayerStatusInfoEventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var newInfo = e.Info;
                var vm = Players.FirstOrDefault(p => p.Info.ProfileId == newInfo.ProfileId);

                if (vm is null)
                    Players.Add(new(newInfo));
                else
                    vm.Info = newInfo;
            });
        }

        public void StopServer()
        {
            SfaMessageBox.ShowDialog(
                App.GetString("s_dialog_stop_server_msg"),
                App.GetString("s_dialog_stop_server_title"),
                MessageBoxButton.Yes | MessageBoxButton.Cancell)
                .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    if (t.IsCompleted == true &&
                        t.Result == MessageBoxButton.Yes)
                    {
                        var server = Server;

                        if (server is not null)
                            server.PlayerStatusUpdated -= PlayerStatusUpdated;

                        Launcher?.StopServer();
                        Players.Clear();
                    }
                }));
        }

        public Task CreateNewRealm()
        {
            return AppVM.ShowCreateNewRealm().ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
            {
                if (AppVM is AppViewModel app &&
                    t.Result is RealmInfoViewModel realm)
                    app.SelectedServerRealm = realm;
            }));
        }

        public Task<bool> DeleteSelectedRealm() =>
            AppVM?.ShowDeleteRealm(AppVM?.SelectedServerRealm) ?? Task.FromResult(false);

        public void ShowBattlegroundsEditor()
        {
            if (Server is null)
                return;

            if (_battlegroundsEditor is not null &&
                _battlegroundsEditor.IsVisible == true)
            {
                if (_battlegroundsEditor.WindowState == WindowState.Minimized)
                    _battlegroundsEditor.WindowState = WindowState.Normal;

                _battlegroundsEditor.Activate();
                return;
            }

            _battlegroundsEditor = new BattlegroundsEditorWindow()
            {
                DataContext = new BGEditorViewModel(this),
            };

            _battlegroundsEditor.Show(App.MainWindow);
        }

        public PlayerStatusInfoViewModel GetCharacterVM(ServerCharacter character) =>
            character is null ? null : GetCharacterVM(character.UniqueId);

        public PlayerStatusInfoViewModel GetCharacterVM(int id)
        {
            return Players.FirstOrDefault(p => p.CharacterId == id);
        }

        public void ShowCurrentAddress()
        {
            new ShowServerAddressPopup()
            {
                Server = this,
            }.ShowDialog();
        }
    }
}
