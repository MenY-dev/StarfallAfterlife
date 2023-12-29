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

        public string ServerSettingsFile => Path.Combine(WorkingDirectory, "Launcher", "ServerSettings.json");

        public ServerSettings ServerSettings { get; set; } = new ServerSettings();

        public Guid LastSelectedProfileId { get; set; }

        public string LastSelectedLocalRealmId { get; set; }

        public void SaveSettings()
        {
            try
            {
                var doc = new JsonObject()
                {
                    ["game_dir"] = GameDirectory,
                    ["last_selected_profile_id"] = LastSelectedProfileId,
                    ["last_selected_local_realm_id"] = LastSelectedLocalRealmId,
                };

                if (SettingsFile is string path &&
                    doc.ToJsonStringUnbuffered(true) is string text)
                {
                    if (Path.GetDirectoryName(path) is string dir &&
                        Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(path, text);
                }
            }
            catch { }
        }

        public void SaveServerSettings()
        {
            try
            {
                if (ServerSettingsFile is string path &&
                    JsonHelpers.SerializeUnbuffered(ServerSettings ??= new(), new() { WriteIndented = true}) is string text)
                {
                    if (Path.GetDirectoryName(path) is string dir &&
                            Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(path, text);
                }
            }
            catch { }
        }

        public void LoadSettings()
        {
            try
            {
                if (SettingsFile is string path &&
                    File.Exists(path) == true &&
                    File.ReadAllText(path) is string text &&
                    JsonHelpers.ParseNodeUnbuffered(text) is JsonObject doc)
                {
                    GameDirectory = (string)doc["game_dir"];
                    LastSelectedProfileId = (Guid?)doc["last_selected_profile_id"] ?? Guid.Empty;
                    LastSelectedLocalRealmId = (string)doc["last_selected_local_realm_id"];
                }
            }
            catch { }
        }


        public void LoadeServerSettings()
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
