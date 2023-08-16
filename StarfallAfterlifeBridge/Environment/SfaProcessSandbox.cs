using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace StarfallAfterlife.Bridge.Environment
{
    public class SfaProcessSandbox : IDisposable
    {
        public string WorkingDirectory
        {
            get => directory;
            set
            {
                directory = value;
                UpdatePaths();
            }
        }

        public string OutputLocation { get; protected set; }
        public string ConfigLocation { get; protected set; }

        public string InstanceConfig { get; set; }
        public string InstanceConfigLocation { get; protected set; }
        public string InstanceLoadResultLocation { get; protected set; }

        public event EventHandler<EventArgs> InstanceLoaded;

        public UEConfig EngineIni { get; set; }
        public string EngineIniLocation { get; protected set; }

        public UEConfig GameIni { get; set; }
        public string GameIniLocation { get; protected set; }

        protected object DeployLockher { get; } = new object();
        protected Timer InstanceLoadedWatcher { get; set; }
        protected string directory;
        private bool disposedValue;

        public SfaProcessSandbox()
        {
            UpdatePaths();
        }

        protected virtual void UpdatePaths()
        {
            var outputLocation = "Output";
            var configLocation = "Config";

            if (WorkingDirectory is string envLocation)
            {
                outputLocation = Path.Combine(envLocation, outputLocation);
                configLocation = Path.Combine(envLocation, configLocation);
            }

            OutputLocation = outputLocation;
            ConfigLocation = configLocation;

            InstanceConfigLocation = Path.Combine(outputLocation, "instance.json");
            InstanceLoadResultLocation = Path.Combine(outputLocation, "instance_loaded");

            EngineIniLocation = Path.Combine(configLocation, "Engine.ini");
            GameIniLocation = Path.Combine(configLocation, "Game.ini");
        }

        public bool Deploy()
        {
            lock (DeployLockher)
            {
                try
                {

                    if (WorkingDirectory is string dir &&
                        Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    if (EngineIni is not null)
                        WriteText(EngineIniLocation, EngineIni.ToConfigString());

                    if (GameIni is not null)
                        WriteText(GameIniLocation, GameIni.ToConfigString());

                    if (string.IsNullOrWhiteSpace(InstanceConfig) == false)
                    {
                        WriteText(InstanceConfigLocation, InstanceConfig);

                        if (InstanceLoadedWatcher is null ||
                            InstanceLoadedWatcher.Enabled == false)
                        {
                            InstanceLoadedWatcher?.Dispose();
                            InstanceLoadedWatcher = new Timer(500);
                            InstanceLoadedWatcher.Elapsed += InstanceLoadedWatcherElapsed;
                            InstanceLoadedWatcher.Start();
                        }
                    }

                }
                catch
                {
                    return false;
                }

                return true;
            }
        }

        private void InstanceLoadedWatcherElapsed(object sender, ElapsedEventArgs e)
        {
            lock (DeployLockher)
            {
                if (sender is Timer watcher &&
                    watcher.Enabled == true &&
                    File.Exists(InstanceLoadResultLocation) == true)
                {
                    watcher.Stop();
                    watcher.Dispose();

                    InstanceLoaded?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected virtual bool WriteText(string path, string text)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);

                if (System.IO.Directory.Exists(directory) == false)
                    System.IO.Directory.CreateDirectory(directory);

                File.WriteAllText(path, text);
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (DeployLockher)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        InstanceLoaded = null;
                        InstanceLoadedWatcher?.Dispose();
                    }

                    disposedValue = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }
    }
}
