using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public string GameDirectory { get; set; }

        public string WorkingDirectory { get; set; }

        public SfaDatabase Database { get; set; }

        public string MapsCacheDirectory => Path.Combine(WorkingDirectory, "Cache", "Maps");

        public GalaxyMapCache MapsCache { get; set; }

        public void Load()
        {
            LoadSettings();

            MapsCache ??= new() { Location = MapsCacheDirectory };
            MapsCache.Init();

            Profiles.Clear();
            Profiles.AddRange(LoadProfiles());

            Realms.Clear();
            Realms.AddRange(LoadRealmsInfo());

            LoadeServerSettings();

            CurrentProfile ??= Profiles.FirstOrDefault(
                p => p?.IsSupported == true && p.GameProfile?.Id == LastSelectedProfileId);

            CurrentProfile ??= Profiles.FirstOrDefault(
                p => p?.IsSupported == true);

            CurrentLocalRealm ??= Realms.FirstOrDefault(r => r?.Realm?.Id == LastSelectedLocalRealmId);
            CurrentLocalRealm ??= Realms.FirstOrDefault();

        }

        public static DirectoryInfo GetOrCreateDirectory(string directory)
        {
            try
            {
                if (directory is not null)
                {
                    if (Directory.Exists(directory) == false)
                        return Directory.CreateDirectory(directory);
                    else
                        return new DirectoryInfo(directory);
                }
            }
            catch { }

            return null;
        }


        public bool TestGameDirectory() => TestGameDirectory(GameDirectory);

        public bool TestGameDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory) ||
                Directory.Exists(directory) == false ||
                File.Exists(Path.Combine(directory, "Msk", "starfall_game", "Starfall", "Binaries", "Win64", "Starfall.exe")) == false)
                return false;

            return true;
        }
    }
}
