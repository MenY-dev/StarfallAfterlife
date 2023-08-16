using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;

namespace StarfallAfterlife.Bridge.Native.Windows
{
    public sealed class WindowsProcess : IDisposable
    {
        public static bool IsAvailable => Win32.IsAvailable;

        public Process SharpProcess { get; private set; }

        public ProcessInfo Info { get; private set; }

        public IntPtr Handle { get; private set; }

        public int Id { get; private set; }

        public IntPtr TheadHandle { get; private set; }

        public int TheadId { get; private set; }

        public bool IsStarted { get => isStarted; }

        public StreamReader StandartOutput { get; private set; }

        private AnonymousPipeServerStream outputPipe;
        private static readonly object startLockher = new();
        private bool isStarted;
        private bool disposedValue;

        public bool Start(string name, string cmd = null, StartupInfo startupInfo = default)
        {
            lock (startLockher)
            {
                if (isStarted == true)
                    return false;

                isStarted = true;

                if (disposedValue == true)
                    return false;

                ProcessInfo processInfo = default;

                Handle = IntPtr.Zero;
                Id = -1;
                TheadHandle = IntPtr.Zero;
                TheadId = -1;

                try
                {
                    string cmdLine = CreateCmdLine(name, cmd);
                    startupInfo.cb = (uint)Marshal.SizeOf<StartupInfo>();

                    if (startupInfo.dwFlags.HasFlag(STARTF.USESTDHANDLES))
                    {
                        outputPipe = new(PipeDirection.In, HandleInheritability.Inheritable);
                        startupInfo.hStdOutput = outputPipe.ClientSafePipeHandle.DangerousGetHandle();
                    }

                    bool result = CreateProcess(
                       null,
                       cmdLine,
                       nint.Zero,
                       nint.Zero,
                       true,
                       0,
                       nint.Zero,
                       null,
                       ref startupInfo,
                       ref processInfo);

                    if (result == false ||
                        processInfo.hProcess is 0 or -1 ||
                        processInfo.hThread is 0 or -1)
                    {
                        CloseOutput();
                        return false;
                    }

                    Handle = processInfo.hProcess;
                    Id = processInfo.ProcessId;
                    TheadHandle = processInfo.hThread;
                    TheadId = processInfo.ThreadId;
                    SharpProcess = Process.GetProcessById(Id);
                    SharpProcess.EnableRaisingEvents = true;
                    SharpProcess.Exited += ProcessExited;

                    if (startupInfo.dwFlags.HasFlag(STARTF.USESTDHANDLES))
                    {
                        StandartOutput = new(outputPipe, Console.OutputEncoding, true, 4096);
                    }
                }
                catch
                {
                    CloseOutput();
                    return false;
                }
            }

            return true;
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            CloseOutput();
        }

        private void CloseOutput()
        {
            outputPipe?.DisposeLocalCopyOfClientHandle();
            outputPipe?.Dispose();
            StandartOutput?.Dispose();
        }

        public void Kill()
        {
            SharpProcess?.Kill();
        }


        public void CloseMainWindow()
        {
            SharpProcess?.CloseMainWindow();
        }

        private string CreateCmdLine(string fileName, string cmd)
        {
            cmd ??= string.Empty;

            if (string.IsNullOrWhiteSpace(fileName))
                return cmd;

            StringBuilder result = new(fileName.Length + cmd.Length + 6);

            if (fileName.StartsWith('"') == false)
                result.Append('"');

            result.Append(fileName);

            if (fileName.EndsWith('"') == false)
                result.Append('"');

            result.Append(' ');
            result.Append(cmd);

            return result.ToString();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SharpProcess?.Dispose();
                    CloseOutput();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }
    }
}
