using Microsoft.Win32.SafeHandles;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Native.Windows;
using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Environment
{
    public partial class SfaProcess
    {
        public string Executable { get; set; }

        public string Map { get; set; }

        public string OutputDirectory { get; set; }

        public string Username { get; set; }

        public string Auth { get; set; }

        public string MgrUrl { get; set; }

        public bool Listen { get; set; }

        public bool Windowed { get; set; }

        public bool DisableSplashScreen { get; set; }

        public bool DisableLoadingScreen { get; set; }

        public bool EnableLog { get; set; }

        public SfaProcessSandbox Sandbox { get; set; }

        public List<string> ProcessArguments { get; set; } = new List<string>();

        public List<string> MapArguments { get; set; } = new List<string>();

        public List<string> ConsoleCommands { get; } = new List<string>();

        public WindowsProcess WinProcess { get; private set; }

        public event EventHandler<SfaProcessOutputEventArgs> OutputUpdated;

        public event EventHandler<EventArgs> Exited;

        public SfaProcess Start()
        {
            Sandbox?.Deploy();

            if (Win32.IsAvailable == true)
                StartWin32Process();

            if  (WinProcess is WindowsProcess process)
            {
                process.Exited -= OnProcessExited;
                process.Exited += OnProcessExited;
            }

            return this;
        }

        public void Close()
        {
            WinProcess?.Close();
        }

        protected virtual void BeginReadingOutput(StreamReader output)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (WinProcess.HasExited == false)
                    {
                        string line = output.ReadLine();
                        OutputUpdated?.Invoke(this, new(line));
#if INSTANCES_DEBUG
                        Debug.WriteLine("> " + line);
#endif
                    }
                }
                catch { }
            }, TaskCreationOptions.LongRunning);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            SfaDebug.Print($"Process Exited ({(sender as Process)?.Id})", GetType().Name);
            Exited?.Invoke(this, EventArgs.Empty);
        }

        protected virtual string BuildArguments()
        {
            StringBuilder sb = new();

            if (string.IsNullOrWhiteSpace(Map) == false)
            {
                sb.Append(Map);

                if (Listen == true)
                {
                    sb.Append("?listen");
                }

                sb.Append("?NodeInstanceIndentity=q1w2e3r4");

                if (MapArguments is not null)
                {
                    foreach (var item in MapArguments)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false)
                        {
                            sb.Append('?');
                            sb.Append(item);
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(MgrUrl) == false)
                sb.Append($" MgrUrl=\"{MgrUrl}\"");

            if (string.IsNullOrWhiteSpace(Username) == false) 
                sb.Append($" LauncherUsername=\"{Username}\"");

            if (string.IsNullOrWhiteSpace(Auth) == false)
                sb.Append($" LauncherAuth=\"{Auth}\"");

            if (string.IsNullOrWhiteSpace(MgrUrl) == false)
                sb.Append($" StartTime=\"{DateTime.Now.ToFileTime()}\"");

            //sb.Append(" -renderoffscreen");
            //sb.Append(" -nullrhi");
            //sb.Append(" -unattended");
            //sb.Append(" -DefaultViewportMouseCaptureMode=CaptureDuringMouseDown");
            //sb.Append(" -simmobile");
            //sb.Append(" -faketouches");
            //sb.Append(" -NoGamepad");

            //sb.Append(" -ExecCmds=\"DebugRemovePlayer 0\"");


            //sb.Append(" -stdout");
            //sb.Append(" -FullStdOutLogOutput");
            sb.Append(" -unattended");
            sb.Append(" -nopause");


            //sb.Append(" -forcelogflush");
            //sb.Append(" -dx10");
            //sb.Append(" graphicsadapter=1");
            //sb.Append(" -ResX=0");
            //sb.Append(" -ResY=0");
            //sb.Append(" -ForceRes");
            //sb.Append(" -fps=2");
            //sb.Append(" -AllowSoftwareRendering");

            if (ConsoleCommands is not null && ConsoleCommands.Count > 0)
                sb.Append($" -ExecCmds=\"{string.Join("; ", ConsoleCommands)}\"");

            //if (EnableLog == true && Listen == false)
            //    sb.Append(" -log");

            //sb.Append(" -NOWRITE");

            if (Windowed == true)
                sb.Append(" -windowed");

            if (DisableSplashScreen == true)
                sb.Append(" -nosplash");

            if (DisableLoadingScreen == true)
                sb.Append(" -noloadingscreen");

            //if (string.IsNullOrWhiteSpace(OutputDirectory) == false)
            //    sb.Append($" -OutputDir=\"{OutputDirectory}\"");

            if (Listen == true)
            {
                sb.Append(" -nosound");
                sb.Append(" -ResX=2");
                sb.Append(" -ResY=2");
                sb.Append(" -ForceRes");
                sb.Append(" -fps=1");
            }

            if (Sandbox is SfaProcessSandbox sandbox)
            {
                if (string.IsNullOrWhiteSpace(sandbox.OutputLocation) == false)
                    sb.Append($" -OutputDir=\"{sandbox.OutputLocation}\"");

                if (sandbox.EngineIni is not null && string.IsNullOrWhiteSpace(sandbox.EngineIniLocation) == false)
                    sb.Append($" -ENGINEINI=\"{sandbox.EngineIniLocation}\"");

                if (sandbox.GameIni is not null && string.IsNullOrWhiteSpace(sandbox.GameIniLocation) == false)
                    sb.Append($" -GAMEINI=\"{sandbox.GameIniLocation}\"");
            }


            if (ProcessArguments is not null)
            {
                foreach (var item in ProcessArguments)
                {
                    if (string.IsNullOrWhiteSpace(item) == false)
                    {
                        sb.Append(' ');
                        sb.Append(item);
                    }
                }
            }

            return sb.ToString().Trim();
        }

        public virtual int GetServerListeningPort()
        {
            int processId = WinProcess.Id;
            return NetUtils.GetUdpListenersInfo()
                           .FirstOrDefault(i => i.ProcessId == processId)
                           .LocalEndPoint?.Port ?? -1;
        }
    }
}
