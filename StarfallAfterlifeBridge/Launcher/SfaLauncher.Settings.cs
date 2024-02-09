using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public string SettingsFile => Path.Combine(WorkingDirectory, "Launcher", "Settings.json");

        public SettingsStorage SettingsStorage { get; } = new();

        public string ServerSettingsFile => Path.Combine(WorkingDirectory, "Launcher", "ServerSettings.json");

        public ServerSettings ServerSettings { get; set; } = new ServerSettings();

        public Guid LastSelectedProfileId { get; set; }

        public string LastSelectedLocalRealmId { get; set; }

        public void SaveSettings()
        {
            try
            {
                var storage = SettingsStorage;
                storage.Path = SettingsFile;

                storage["game_dir"] = GameDirectory;
                storage["last_selected_profile_id"] = LastSelectedProfileId;
                storage["last_selected_local_realm_id"] = LastSelectedLocalRealmId;

                storage.Save();
            }
            catch { }
        }

        public void SaveServerSettings()
        {
            try
            {
                var doc = JsonHelpers.ParseNodeFromFileUnbuffered(ServerSettingsFile)?
                    .AsObjectSelf() ?? new JsonObject();

                doc.Override(JsonHelpers.ParseNodeUnbuffered(ServerSettings ?? new())?.AsObjectSelf());
                doc.WriteToFileUnbuffered(ServerSettingsFile, new() { WriteIndented = true });
            }
            catch { }
        }

        public void LoadSettings()
        {
            try
            {
                var storage = SettingsStorage;
                storage.Path = SettingsFile;
                storage.Load();

                GameDirectory = (string)storage["game_dir"];
                LastSelectedProfileId = (Guid?)storage["last_selected_profile_id"] ?? Guid.Empty;
                LastSelectedLocalRealmId = (string)storage["last_selected_local_realm_id"];
            }
            catch { }
        }


        public void LoadServerSettings()
        {
            try
            {
                if (ServerSettingsFile is string path &&
                    File.Exists(path) == true &&
                    File.ReadAllText(path) is string text &&
                    JsonHelpers.DeserializeUnbuffered<ServerSettings>(text) is ServerSettings settings)
                {
                    ServerSettings = settings;
                    _isServerRealmValueValid = false;
                }
            }
            catch { }
        }
    }
}
