using Avalonia.OpenGL;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CreateServerPageViewModel : ViewModelBase
    {
        public AppViewModel AppVM { get; }

        public SfaLauncher Launcher => AppVM?.Launcher;

        public bool ServerStarted
        {
            get => _serverStarted;
            set => SetAndRaise(ref _serverStarted, value);
        }

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                SetAndRaise(ref _serverAddress, value);
                Launcher.ServerAddress = value;
            }
        }

        public ushort ServerPort
        {
            get => _serverPort;
            set
            {
                SetAndRaise(ref _serverPort, value);
                Launcher.ServerPort = value;
            }
        }

        public ObservableCollection<InterfaceInfo> Interfaces { get; } = new();

        private string _serverAddress;
        private ushort _serverPort;
        private bool _serverStarted;

        public CreateServerPageViewModel(AppViewModel mainWindowViewModel)
        {
            AppVM = mainWindowViewModel;
            _serverAddress = Launcher?.ServerAddress;
            _serverPort = Launcher?.ServerPort ?? 0;
            UpdateInterfaces();
        }

        public void UpdateInterfaces()
        {
            var result = new List<InterfaceInfo>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                   .Where(i => i.OperationalStatus is OperationalStatus.Up);

                foreach (var item in interfaces)
                {
                    if (item is null)
                        continue;

                    var ip = item.GetIPProperties().UnicastAddresses?.FirstOrDefault(
                       x => x.Address.AddressFamily == AddressFamily.InterNetwork);

                    if (ip is null)
                        continue;

                    result.Add(new() { Address = ip.Address, Name = item.Name});
                }
            }
            catch { }

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


        public void StartServer()
        {
            if (Launcher is SfaLauncher launcher)
            {
                var server = launcher.StartServer();

                if (server is not null)
                {
                    ServerStarted = true;

                    server.Task.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                    {
                        ServerStarted = false;
                    }));
                }
            }
        }

        public void StopServer()
        {
            Launcher?.StopServer();
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
    }
}
