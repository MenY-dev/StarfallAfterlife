using Microsoft.Win32.SafeHandles;
using StarfallAfterlife.Bridge.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        public IntPtr ThreadHandle { get; private set; }

        public int ThreadId { get; private set; }

        public bool IsStarted => _isStarted;

        public bool AttachToMainProcess
        {
            get => _attachToMainProcess;
            set
            {
                lock (_processLockher)
                    if (_isStarted == false)
                        _attachToMainProcess = value;
            }
        }

        public StreamReader StandartOutput { get; private set; }

        public bool HasExited => SharpProcess?.HasExited ?? true;

        public nint MainWindowHandle => SharpProcess?.MainWindowHandle ?? nint.Zero;

        public event EventHandler<EventArgs> Exited;

        private AnonymousPipeServerStream _outputPipe;
        private static readonly object _processLockher = new();
        private bool _isStarted;
        private bool _isExitCompleted;
        private bool _disposedValue;
        private ProcessWaitHandle _waitHandle;
        private RegisteredWaitHandle _threadPoolWaitHandle;
        private bool _attachToMainProcess;
        private JobObject _job;

        public bool Start(string name, string cmd = null, StartupInfo startupInfo = default)
        {
            lock (_processLockher)
            {
                if (_isStarted == true)
                    return false;

                _isStarted = true;

                if (_disposedValue == true)
                    return false;

                ProcessInfo processInfo = default;

                Handle = IntPtr.Zero;
                Id = -1;
                ThreadHandle = IntPtr.Zero;
                ThreadId = -1;

                try
                {
                    string cmdLine = CreateCmdLine(name, cmd);
                    startupInfo.cb = (uint)Marshal.SizeOf<StartupInfo>();

                    if (startupInfo.dwFlags.HasFlag(STARTF.USESTDHANDLES))
                    {
                        _outputPipe = new(PipeDirection.In, HandleInheritability.Inheritable);
                        startupInfo.hStdOutput = _outputPipe.ClientSafePipeHandle.DangerousGetHandle();
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
                        ReleaseUnmanagedResources();
                        return false;
                    }

                    Handle = processInfo.hProcess;
                    Id = processInfo.ProcessId;
                    ThreadHandle = processInfo.hThread;
                    ThreadId = processInfo.ThreadId;
                    SharpProcess = Process.GetProcessById(Id);
                    StartWatchingForExit();

                    if (startupInfo.dwFlags.HasFlag(STARTF.USESTDHANDLES))
                    {
                        StandartOutput = new(_outputPipe, Console.OutputEncoding, true, 4096);
                    }

                    if (AttachToMainProcess == true)
                    {
                        _job = JobObject.Create(new()
                        {
                            LimitFlags = LimitFlags.KILL_ON_JOB_CLOSE,
                        });
                        
                        _job.AttachToProcess(Handle);
                    }

                }
                catch
                {
                    ReleaseUnmanagedResources();
                    return false;
                }
            }

            return true;
        }

        private void ProcessExit()
        {
            lock (_processLockher)
            {
                if (_isExitCompleted == true)
                    return;

                ReleaseUnmanagedResources();
                Exited?.Invoke(this, EventArgs.Empty);
                _isExitCompleted = true;
            }
        }

        private void ReleaseUnmanagedResources()
        {
            try
            {
                _outputPipe?.DisposeLocalCopyOfClientHandle();
                _outputPipe?.Dispose();
                StandartOutput?.Dispose();
            }
            catch { }

            try
            {
                _job?.Dispose();
                SharpProcess?.Dispose();
            }
            catch { }

            try
            {
                _waitHandle?.Dispose();
                _threadPoolWaitHandle?.Unregister(null);
            }
            catch { }
        }

        public void Kill()
        {
            lock (_processLockher)
            {
                if (_isStarted == false)
                    return;

                SharpProcess?.Kill();
                ProcessExit();
            }
        }


        public void CloseMainWindow()
        {
            lock (_processLockher)
            {
                if (_isStarted == false)
                    return;

                SharpProcess?.CloseMainWindow();
                ProcessExit();
            }
        }


        private void StartWatchingForExit()
        {
            try
            {
                var handle = Handle;

                if (handle == nint.Zero)
                    return;

                _waitHandle = new ProcessWaitHandle(Handle);
                _threadPoolWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                    _waitHandle, (s, t) => ProcessExit(), _waitHandle, -1, true);
            }
            catch (Exception)
            {

            }
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
            if (!_disposedValue)
            {
                if (_isStarted == true)
                    Kill();

                if (disposing)
                {

                }

                ReleaseUnmanagedResources();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        private class ProcessWaitHandle : WaitHandle
        {
            public ProcessWaitHandle(IntPtr processHandle)
            {
                SafeWaitHandle = new SafeWaitHandle(processHandle, false);
            }
        }

    }
}
