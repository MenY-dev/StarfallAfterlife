using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Services
{
    public class GameFinder
    {
        public static GameFinder Instance => _instance.Value;

        private static Lazy<GameFinder> _instance = new(() =>
        {
            if (OperatingSystem.IsWindows() == true)
                return new WindowsGameFinder();

            return new GameFinder();
        });

        public virtual string FindGameDirectory()
        {
            return null;
        }
    }

    public class WindowsGameFinder : GameFinder
    {
        public override string FindGameDirectory()
        {
            try
            {
                var path = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Starfall Online", "InstallDir", null) as string ??
                           Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Starfall Online", "InstallDir", null) as string;

                if (path is not null &&
                    Directory.Exists(path))
                    return path;
            }
            catch { }


            static string FindInSteam(RegistryKey root)
            {
                try
                {
                    var keys = root.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\");

                    if (keys is null)
                        return null;

                    foreach (var subKeyName in keys.GetSubKeyNames())
                    {
                        if (subKeyName is null || subKeyName.StartsWith("Steam App ") == false)
                            continue;

                        try
                        {
                            var subKey = keys.OpenSubKey(subKeyName);

                            if (subKey is null || (subKey.GetValue("DisplayName") as string) != "Starfall Online")
                                continue;

                            var path = subKey.GetValue("InstallLocation") as string;

                            if (path is not null &&
                                File.Exists(Path.Combine(path, "Msk", "starfall_game", "Starfall.exe")) == true)
                            {
                                return path;
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                return null;
            }

            if ((FindInSteam(Registry.LocalMachine) ?? FindInSteam(Registry.CurrentUser)) is string steamPath)
                return steamPath;

            return null;
        }
    }
}
