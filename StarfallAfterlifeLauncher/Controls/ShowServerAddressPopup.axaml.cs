using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class ShowServerAddressPopup : SfaPopup
    {
        public static readonly StyledProperty<string> MainAddressProperty =
            AvaloniaProperty.Register<ShowServerAddressPopup, string>(nameof(MainAddress));

        public CreateServerPageViewModel Server { get; set; }

        public AvaloniaList<string> Addresses { get; } = new();

        public string MainAddress
        {
            get => GetValue(MainAddressProperty);
            set => SetValue(MainAddressProperty, value);
        }

        public ShowServerAddressPopup()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void CopyMainAddress()
        {
            if (MainAddress is string address)
                Clipboard?.SetTextAsync(address);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var server = Server;

            if (server is null)
                return;

            var serverAddress = server.ServerAddress;
            var isAny = IPAddress.Any.ToString().Equals(serverAddress);
            var interfaces = NetUtils.GetInterfaces();
            var addresses = interfaces
                .Where(i => i.Address.Equals(IPAddress.Loopback) == false)
                .Select(i => i.Address.ToString())
                .ToArray();

            if (isAny == true)
            {
                serverAddress = addresses.FirstOrDefault();
            }
            else
            {
                addresses = new string[] { serverAddress };
            }

            if (server.UsePortForwarding == true)
            {
                Task.Factory.StartNew(() =>
                {
                    var request = NatPuncher
                        .GetExternalAdresses();

                    if (request.Wait(TimeSpan.FromSeconds(5)) == true &&
                        request.IsCompleted == false)
                        return;

                    var externalAdresses = request.Result;

                    if (isAny == true)
                    {
                        serverAddress = externalAdresses
                            .FirstOrDefault(i => i.External is not null).External?
                            .ToString() ?? serverAddress;

                        addresses = externalAdresses
                            .Where(i => i.External is not null)
                            .Select(i => i.External.ToString())
                            .Where(i => addresses.Contains(i) == false)
                            .Concat(addresses)
                            .ToArray();
                    }
                    else
                    {
                        var targetInterface = interfaces.FirstOrDefault(i => 
                            i.Address?.ToString().Equals(serverAddress) == true);

                        serverAddress = externalAdresses
                            .FirstOrDefault(i =>
                                i.External is not null &&
                                targetInterface.Info?.GetIPProperties()?.GatewayAddresses
                                .Any(g => g.Address.Equals(i.Internal)) == true).External?
                            .ToString() ?? serverAddress;
                    }

                    Dispatcher.UIThread.Invoke(() => ShowAddresses(serverAddress, addresses));
                });
            }

            ShowAddresses(serverAddress, addresses);
        }

        protected void ShowAddresses(string mainAddress, IEnumerable<string> addresses)
        {
            addresses = addresses?
                .ToArray()
                .Where(i => i?.Equals(mainAddress) == false)
                .ToArray() ?? Enumerable.Empty<string>();

            if (Server?.ServerPort is ushort port &&
                port != 50200)
            {
                mainAddress = $"{mainAddress}:{port}";
                addresses = addresses.Select(i => $"{i}:{port}");
            }

            MainAddress = mainAddress;
            Addresses.Clear();
            Addresses.AddRange(addresses);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (Owner is Window window)
            {
                var offset = (window.Bounds.Size - Bounds.Size) / 2;

                Position = new PixelPoint(
                    window.Position.X + (int)offset.Width,
                    window.Position.Y + (int)offset.Height);
            }
        }
    }
}
