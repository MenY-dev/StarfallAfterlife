using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarfallAfterlife
{
    public class Settings
    {

        public static Settings Current
        {
            get
            {
                if (current is null)
                    current = new Settings();

                return current;
            }
            protected set => current = value is null ? new Settings() : value;
        }

        public string GameDirectory { get; set; }

        private static Settings current;

        public static void Save()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Settings.json");
                File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(Current, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        public static void Load()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Settings.json");

                if (File.Exists(path) == false)
                    return;

                Current = JsonSerializer.Deserialize<Settings>(File.ReadAllText(path));
            }
            catch { }
        }
    }
}
