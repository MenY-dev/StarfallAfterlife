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

        public string Localization { get; set; }

        public void SaveSettings()
        {
            try
            {
                var doc = JsonHelpers.ParseNodeFromFileUnbuffered(SettingsFile)?
                    .AsObjectSelf() ?? new JsonObject();

                doc.Override(new Dictionary<string, JsonNode>()
                {
                    ["game_dir"] = GameDirectory,
                    ["last_selected_profile_id"] = LastSelectedProfileId,
                    ["last_selected_local_realm_id"] = LastSelectedLocalRealmId,
                    ["localization"] = Localization,
                });

                doc.WriteToFileUnbuffered(SettingsFile, new() { WriteIndented = true });
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
                if (SettingsFile is string path &&
                    File.Exists(path) == true &&
                    File.ReadAllText(path) is string text &&
                    JsonHelpers.ParseNodeUnbuffered(text) is JsonObject doc)
                {
                    GameDirectory = (string)doc["game_dir"];
                    LastSelectedProfileId = (Guid?)doc["last_selected_profile_id"] ?? Guid.Empty;
                    LastSelectedLocalRealmId = (string)doc["last_selected_local_realm_id"];
                    Localization = (string)doc["localization"];
                }
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
