using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Launcher.MapEditor;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public partial class EditFleetWindow : Window
    {
        public string FleetFile { get; set; }

        public ShipSelector ShipSelector { get; protected set; }

        public EditFleetWindow()
        {
            InitializeComponent();
            IsEnabled = false;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            try
            {
                DataContext = new MobFleetViewModel
                {
                    Info = JsonHelpers.DeserializeUnbuffered<DiscoveryMobInfo>(File.ReadAllText(FleetFile))
                };
            }
            catch
            {
                DataContext = new MobFleetViewModel { Info = new() };
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            Trace.WriteLine("fdgfdgdfg");

            Task.Delay(300).ContinueWith(t =>
                Dispatcher.UIThread.Invoke(() => IsEnabled = true));
        }

        public void SaveFleet()
        {
            Close();

            if ((DataContext as MobFleetViewModel)?.Info is DiscoveryMobInfo mobInfo)
            {
                try
                {
                    string text = JsonHelpers.SerializeUnbuffered(mobInfo, new() { WriteIndented = true });

                    if (Path.GetDirectoryName(FleetFile) is string dir &&
                        Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(FleetFile, text);
                }
                catch { }
            }
        }

        public void AddShip()
        {
            var selector = ShipSelector = new(ShipSelector);

            selector?.ShowDialog<bool>(this).ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
            {
                var selectedShip = selector.Selection;

                if (t.Result == false || selectedShip is null)
                    return;

                (DataContext as MobFleetViewModel)?.AddShip(selectedShip.Id);
            }));
        }

        public void ShipPressed(object shipData)
        {
            if (shipData is MobShipViewModel ship)
            {
                new EditShipWindow() { DataContext = ship }.ShowDialog(this);
            }
        }
    }
}
