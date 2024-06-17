using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public partial class MobsEditorWindow : Window
    {
        public Dictionary<string, DiscoveryMobInfo> Mobs { get; protected set; }

        public SelectionModel<KeyValuePair<string, DiscoveryMobInfo>> Selection { get; } = new();

        public string MobsDirectory => App.Launcher?.GameDirectory is string gameDir ?
            Path.Combine(gameDir, "Msk", "starfall_game", "Mgrs", "sfmgr", "mobs") : null;

        public MobsEditorWindow()
        {
            InitializeComponent();
            Selection.SelectionChanged += SelectionChanged;
        }

        private void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<KeyValuePair<string, DiscoveryMobInfo>> e)
        {
            if (e.SelectedItems.FirstOrDefault().Key is string file &&
                File.Exists(file) == true)
                new EditFleetWindow() { FleetFile = file }.ShowDialog(this).ContinueWith(t =>
                {
                    Dispatcher.UIThread.Invoke(UpdateMobsList);
                });
        }

        public void UpdateMobsList()
        {
            var list = this.Find<ListBox>("MobsList");

            if (list is not null &&
                MobsDirectory is string mobsDir &&
                Directory.Exists(mobsDir) == true)
            {
                var files = new List<string>();
                Mobs = new();

                try
                {
                    files.AddRange(Directory.GetFiles(mobsDir));

                    foreach (var file in files)
                    {
                        var mob = LoadMob(file);

                        if (mob is not null)
                            Mobs.Add(file, mob);
                    }
                }
                catch { }

                var scroll = list.Scroll.Offset;
                list.ItemsSource = Mobs;
                list.CoerceValue(ListBox.ItemsSourceProperty);
                list.UpdateLayout();
                list.Scroll.Offset = scroll;
            }
        }

        public DiscoveryMobInfo LoadMob(string path)
        {
            try
            {
                return JsonHelpers.DeserializeUnbuffered<DiscoveryMobInfo>(File.ReadAllText(path));
            }
            catch { }

            return null;
        }

        public void OpenFolder()
        {
            if (MobsDirectory is string mobsDir)
                Process.Start("explorer.exe", $"\"{mobsDir}\"");
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            UpdateMobsList();
        }
    }
}
