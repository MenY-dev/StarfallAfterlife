using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StarfallAfterlife.Bridge;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Serialization.Json;
using static StarfallAfterlife.Bridge.Diagnostics.SfaDebug;

namespace StarfallAfterlife
{
    public partial class MainForm : Form
    {

        public string AppLocation => Application.StartupPath;
        public string ExeLocation => Path.Combine(Settings.Current.GameDirectory, "Msk", "starfall_game", "Starfall", "Binaries", "Win64", "Starfall.exe");
        public string LogsLocation => Path.Combine(Settings.Current.GameDirectory, "Msk", "starfall_game", "Starfall.exe", "Starfall", "Saved", "Logs");

        public SfaGameProfile Profile { get; set; }
        public Tests Tests { get; } = new Tests();

        public MainForm()
        {
            Settings.Load();
            SfaDatabase.LoadDefault();
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            Console.SetOut(new ConsoleWriter(ConsoleView));
            LoadProfile();
            Tests.Profile = Profile;
            UpdateValues();
        }

        public void LoadProfile()
        {
            try
            {
                string profilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games", "StarfallAfterlife", "Profiles", "0", "Profile.json");

                if (File.Exists(profilePath))
                    Profile = JsonSerializer.Deserialize<SfaGameProfile>(File.ReadAllText(profilePath));

            }
            catch { }
            finally
            {
                bool needSave = false;

                if (Profile is null)
                {
                    Profile = new SfaGameProfile() { Nickname = "Meny" };
                    needSave = true;
                }

                if (Profile.DiscoveryModeProfile.Chars.Count < 1)
                {
                    Profile.DiscoveryModeProfile.Chars.Add(new Character { Name = "MenY" });
                    needSave = true;
                }

                if (Profile.CurrentCharacter is null)
                {
                    Profile.CurrentCharacter = Profile.DiscoveryModeProfile.Chars[0];
                }

                if (needSave == true)
                    SaveProfile();
            }

            Profile.Edited += (o, e) =>
            {
                Profile.Use((h) => SaveProfile());
            };
        }

        public void SaveProfile()
        {
            try
            {
                string profilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games", "StarfallAfterlife", "Profiles", "0", "Profile.json");

                if (Directory.Exists(AppLocation))
                    File.WriteAllText(profilePath, JsonSerializer.Serialize(Profile, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        protected override void OnClosed(EventArgs e)
        {
            //SaveProfile();
            base.OnClosed(e);
        }

        private void StartBtnClick(object sender, EventArgs e)
        {
            Tests.StartGame();
            Tabs.SelectTab(LogTab);
        }

        private void StartGalaxyInstanseClick(object sender, EventArgs e)
        {

        }

        private void SelectGameDirectoryBtnClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();

            dialog.SelectedPath = Settings.Current.GameDirectory;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Current.GameDirectory = dialog.SelectedPath;
                Settings.Save();
                UpdateValues();
            }
        }

        protected void UpdateValues()
        {
            var settings = Settings.Current;
            GameDirectoryBox.Text = settings.GameDirectory;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Save();
            base.OnClosing(e);
        }

        private void ScanQuestsBtnClick(object sender, EventArgs e)
        {

        }

        private void SystemHexMapTestBtn_Click(object sender, EventArgs e)
        {
        }

        private void PathFindingTestBtn_Click(object sender, EventArgs e)
        {
            new PathFindingTestForm().ShowDialog();
        }
    }
}
