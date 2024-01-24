using Avalonia.Controls.Documents;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class FindServerPageViewModel : ViewModelBase
    {
        public AppViewModel AppVM { get; }

        public SfaLauncher Launcher => AppVM?.Launcher;

        public ObservableCollection<RemoteServerInfoViewModel> Servers { get; } = new();

        public RemoteServerInfoViewModel SelectedServer
        {
            get => _selectedServer;
            set => SetAndRaise(ref _selectedServer, value);
        }

        public bool IsUpdateStarted { get; protected set; }

        private Task _updateTask;
        private RemoteServerInfoViewModel _selectedServer;

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                SetAndRaise(ref _serverAddress, value);
            }
        }

        private string _serverAddress;

        public FindServerPageViewModel(AppViewModel appVM)
        {
            AppVM = appVM;
            UpdateList();
        }

        public void ShowAddServerDialog()
        {
            Dispatcher.UIThread.InvokeAsync(() => new AddServerDialog()
                .ShowDialog()
                .ContinueWith(t => Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (t.Result is AddServerDialog result &&
                        result.IsDone == true &&
                        result.Text is string text &&
                        IPEndPoint.TryParse(text, out var ip) == true)
                    {
                        if (ip.Port < 1)
                            ip.Port = 50200;

                        if (AddServer(ip.ToString()) is var(IsSuccess, Reason) &&
                            IsSuccess == false)
                        {
                            SfaMessageBox.ShowDialog(
                                App.GetString("s_dialog_server_connect_add_" + (Reason ?? string.Empty)),
                                App.GetString("s_dialog_server_connect_add_error_title"));
                        }
                    }
                })));
        }

        public (bool Result, string Reason) AddServer(string address)
        {
            if (address is null ||
                IPEndPoint.TryParse(address, out var endpoint) == false ||
                endpoint?.Address?.Equals(IPAddress.Any) == true ||
                endpoint?.Address?.AddressFamily == AddressFamily.InterNetworkV6)
                return (false, "bad_address");

            var comparison = StringComparison.InvariantCultureIgnoreCase;

            if (Servers.Any(s => s?.Address?.Equals(address, comparison) == true))
                return (false, "already_exist");

            if (Launcher is SfaLauncher launcher)
            {
                var result = launcher.AddServer(address);
                UpdateList();
                UpdateStatuses();
                return result;
            }

            return (false, "internal_error");
        }

        public void UpdateList()
        {
            if (Launcher is SfaLauncher launcher)
            {
                var selected = SelectedServer;
                var newServers = launcher.ServerList.ToArray();
                var toRemove = Servers.Where(s => s is null || newServers.Contains(s.Info) == false).ToArray();
                var toAdd = newServers.Where(s => s is not null && Servers.Any(vm => vm.Info == s) == false).ToArray();

                foreach (var server in toRemove)
                    Servers.Remove(server);

                foreach (var server in Servers)
                    if (server is not null)
                        server.Info = server.Info;

                foreach (var server in toAdd)
                    Servers.Add(new(this, server));

                Servers.Sort(Comparer<RemoteServerInfoViewModel>.Create((x, y) => 
                {
                    if (x is null || y is null)
                        return x == y ? 0 : (x == null ? -1 : 1);

                    var xVersion = x.Version ?? SfaServer.Version;
                    var yVersion = y.Version ?? SfaServer.Version;
                    var xIsActual = SfaServer.IsVersionCompatible(xVersion);
                    var yIsActual = SfaServer.IsVersionCompatible(yVersion);

                    if (xIsActual != yIsActual)
                        return yIsActual.CompareTo(xIsActual);

                    var result = yVersion.CompareTo(xVersion);

                    if (result != 0)
                        return result;

                    result = y.IsOnline.CompareTo(x.IsOnline);

                    if (result != 0)
                        return result;

                    var xName = x.Name ?? x.Address ?? string.Empty;
                    var yName = y.Name ?? y.Address ?? string.Empty;

                    return xName.CompareTo(yName);
                }));

                SelectedServer = Servers.FirstOrDefault(s => s.Address == selected?.Address);
                SelectedServer ??= Servers.FirstOrDefault();
            }
        }

        public void UpdateStatuses()
        {
            if (Launcher is SfaLauncher launcher)
            {
                launcher.SaveServerList();

                var oldTask = _updateTask;
                var progress = new Progress<RemoteServerInfo>(info =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var vm = Servers.FirstOrDefault(vm => vm?.Info == info);

                        if (vm is not null)
                            vm.Info = info;
                    });
                });

                var task = _updateTask = launcher.UpdateServerList(progress);

                IsUpdateStarted = true;
                RaisePropertyChanged(oldTask is not null, IsUpdateStarted, nameof(IsUpdateStarted));

                _updateTask.ContinueWith(t => Dispatcher.UIThread.Invoke(() => 
                {
                    launcher.SaveServerList();
                    UpdateList();

                    if (_updateTask == task)
                    {
                        IsUpdateStarted = false;
                        RaisePropertyChanged(true, false, nameof(IsUpdateStarted));
                        _updateTask = null;
                    }
                }));
            }
        }

        public void RemoveSelectedServer()
        {
            var server = SelectedServer;

            if (server is null)
                return;

            var name = server.Name ?? server.Address ?? "server";

            SfaMessageBox.ShowDialog(
                string.Format(App.GetString("s_dialog_server_connect_remove_server_msg") ?? string.Empty, name),
                string.Format(App.GetString("s_dialog_server_connect_remove_server_title") ?? string.Empty, name),
                MessageBoxButton.Delete | MessageBoxButton.Cancell)
                .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    if (t.Result is MessageBoxButton.Delete &&
                        Launcher is SfaLauncher launcher)
                    {
                        launcher.ServerList?.Remove(SelectedServer?.Info);
                        launcher.SaveServerList();
                        UpdateList();
                    }
                }));
        }

        public void ConnectToServer()
        {
            try
            {
                var launcher = Launcher;
                var server = SelectedServer;
                var appVM = AppVM;

                if (server is null || server.Info is null || appVM is null)
                    SfaMessageBox.ShowDialog(
                        App.GetString("s_dialog_server_connect_error_not_selected"),
                        App.GetString("s_dialog_server_connect_error_title"));

                var serverName = server.Name ?? server.Address ?? "server";
                var connectionCancellation = new CancellationTokenSource();
                var connectingDialog = new SfaMessageBox
                {
                    Title = App.GetString("s_dialog_server_connect_title"),
                    Text = string.Format(App.GetString("s_dialog_server_connect_msg") ?? string.Empty, serverName),
                    Buttons = MessageBoxButton.Cancell
                };

                appVM.MakeBaseTests(true, true)
                    .ContinueWith(t => Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (t.Result == false)
                        return;

                    connectingDialog.ShowDialog().ContinueWith(t =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (connectingDialog.PressedButton == MessageBoxButton.Cancell)
                                connectionCancellation.Cancel();

                        });
                    });

                    var stateRequest = server.Update(connectionCancellation.Token).ContinueWith(t =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() => connectingDialog.Close());

                        if (connectionCancellation.IsCancellationRequested == true)
                            return false;

                        if (t.IsCanceled == true ||
                            t.Result == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                                SfaMessageBox.ShowDialog(
                                    App.GetString("s_dialog_server_connect_error_offline"),
                                    App.GetString("s_dialog_server_connect_error_title")));

                            return false;
                        }

                        if (AppVM is AppViewModel appVM &&
                            appVM.ProcessSessionsCancellationBeforePlay(server.Id).Result == false)
                            return false;

                        if (server.Version is Version serverVersion &&
                            SfaServer.Version is Version currentVersion &&
                            (serverVersion.Major != currentVersion.Major ||
                            serverVersion.Minor != currentVersion.Minor))
                        {

                            Dispatcher.UIThread.Invoke(() =>
                                SfaMessageBox.ShowDialog(
                                    App.GetString("s_dialog_server_connect_error_version"),
                                    App.GetString("s_dialog_server_connect_error_title")));

                            return false;
                        }

                        return true;
                    });

                    stateRequest.ContinueWith(t =>
                    {
                        if (t?.Result is not true)
                            return;

                        string address = IPEndPoint.Parse(SelectedServer?.Address).ToString();

                        if (Launcher is SfaLauncher launcher)
                        {
                            var writePasswordDialog = new Func<string>(() =>
                            {
                                var completionSource = new TaskCompletionSource<string>();

                                Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    var dialog = new EnterPasswordDialog();

                                    dialog.ShowDialog().ContinueWith(t => Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        if (dialog.IsDone == true &&
                                            string.IsNullOrWhiteSpace(dialog.Text) == false &&
                                            SfaServer.CreatePasswordHash(dialog.Text) is string hash)
                                            completionSource.SetResult(hash);
                                        else
                                            completionSource.SetResult(null);
                                    }));

                                });

                                return completionSource.Task.Result;
                            });

                            AppVM.StartGame(launcher.CurrentProfile, address, writePasswordDialog)
                                .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                                {
                                    var session = t.Result;
                                    if (session.IsSuccess == false &&
                                        session.Reason is string reason &&
                                        reason != "auth_cancelled")
                                        SfaMessageBox.ShowDialog(
                                            App.GetString("s_dialog_server_connect_error_" + (reason ?? string.Empty)) ?? reason,
                                            App.GetString("s_dialog_server_connect_error_title"));
                                }));
                        }
                    });
                }));
            }
            catch (Exception e)
            {
                SfaMessageBox.ShowDialog(e.Message, App.GetString("s_dialog_server_connect_error_title"));
            }
        }

        public void OnPageShow()
        {
            UpdateStatuses();
        }
    }
}
