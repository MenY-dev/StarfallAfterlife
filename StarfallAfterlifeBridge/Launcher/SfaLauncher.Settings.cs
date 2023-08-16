using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public string SettingsFile => Path.Combine(WorkingDirectory, "Launcher", "Settings.json");

        public Guid LastSelectedProfileId { get; set; }

        public string LastSelectedLocalRealmId { get; set; }

        public string LastSelectedServerRealmId { get; set; }

        public void SaveSettings()
        {
            try
            {
                var doc = new JsonObject()
                {
                    ["game_dir"] = GameDirectory,
                    ["last_selected_profile_id"] = LastSelectedProfileId,
                    ["last_selected_local_realm_id"] = LastSelectedLocalRealmId,
                    ["last_selected_server_realm_id"] = LastSelectedServerRealmId,
                    ["server_address"] = ServerAddress,
                    ["server_port"] = ServerPort,
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
                    LastSelectedServerRealmId = (string)doc["last_selected_server_realm_id"];
                    ServerAddress = (string)doc["server_address"] ?? ServerAddress;
                    ServerPort = (ushort?)doc["server_port"] ?? ServerPort;
                }
            }
            catch { }
        }
    }
}
