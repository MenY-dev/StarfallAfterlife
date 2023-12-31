﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;

namespace StarfallAfterlife.Bridge.Native.Windows
{
    public static class Win32
    {
        public static bool IsAvailable
        {
            get
            {
                lock (locker)
                {
                    if (isVerified == true)
                        return isAvailable;

                    try
                    {
                        Marshal.PrelinkAll(typeof(Win32));
                    }
                    catch
                    {
                        isVerified = true;
                        return isAvailable = false;
                    }

                    isVerified = true;
                    return isAvailable = true;
                }
            }
        }

        private static bool isVerified = false;
        private static bool isAvailable = false;
        private static object locker = new();

        public sealed class JobObject : IDisposable
        {
            private JOBOBJECT_EXTENDED_LIMIT_INFORMATION _info;
            private bool _disposedValue;
            private nint _handle;

            private JobObject() { }

            public static JobObject Create(JOBOBJECT_BASIC_LIMIT_INFORMATION info) => CreateExtended(new() { BasicLimitInformation = info });

            public static JobObject CreateExtended(JOBOBJECT_EXTENDED_LIMIT_INFORMATION info) => new() { _info = info };

            public bool AttachToProcess(int processId) => AttachToProcess(Process.GetProcessById(processId));

            public bool AttachToProcess(Process process) => process is null ? false : AttachToProcess(process.Handle);

            public bool AttachToProcess(nint processHandle)
            {
                if (_disposedValue == true || processHandle == nint.Zero)
                    return false;

                if (_handle == nint.Zero)
                    CreateNewHandle();

                return AssignProcessToJobObject(_handle, processHandle);
            }

            private bool CreateNewHandle()
            {
                _handle = CreateJobObjectW(nint.Zero, null);

                var length = Marshal.SizeOf(_info);
                var infoPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(_info, infoPtr, false);

                var result = SetInformationJobObject(_handle, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, infoPtr, (uint)length);
                Marshal.FreeHGlobal(infoPtr);
                return result;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing) { }

                    CloseHandle(_handle);
                    _disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct ProcessInfo
        {
            public nint hProcess;
            public nint hThread;
            public int ProcessId;
            public int ThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public LimitFlags LimitFlags;
            public nuint MinimumWorkingSetSize;
            public nuint MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public nuint Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public nuint ProcessMemoryLimit;
            public nuint JobMemoryLimit;
            public nuint PeakProcessMemoryUsed;
            public nuint PeakJobMemoryUsed;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SecurityAttributes
        {
            public int length;
            public nint lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        public enum WindowLongParam : int
        {
            GWL_WNDPROC = -4,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_EXSTYLE = -20,
            GWL_USERDATA = -21,
            DWLP_MSGRESULT = 0,
            DWLP_USER = 8,
            DWLP_DLGPROC = 4
        }

        [Flags()]
        public enum WindowStyles : uint
        {
            BORDER = 0x800000,
            CAPTION = 0xc00000,
            CHILD = 0x40000000,
            CLIPCHILDREN = 0x2000000,
            CLIPSIBLINGS = 0x4000000,
            DISABLED = 0x8000000,
            DLGFRAME = 0x400000,
            GROUP = 0x20000,
            HSCROLL = 0x100000,
            MAXIMIZE = 0x1000000,
            MAXIMIZEBOX = 0x10000,
            MINIMIZE = 0x20000000,
            MINIMIZEBOX = 0x20000,
            OVERLAPPED = 0x0,
            OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | SIZEFRAME | MINIMIZEBOX | MAXIMIZEBOX,
            POPUP = 0x80000000u,
            POPUPWINDOW = POPUP | BORDER | SYSMENU,
            SIZEFRAME = 0x40000,
            SYSMENU = 0x80000,
            TABSTOP = 0x10000,
            VISIBLE = 0x10000000,
            VSCROLL = 0x200000
        }

        [Flags]
        public enum WindowStylesEx : uint
        {
            ACCEPTFILES = 0x00000010,
            APPWINDOW = 0x00040000,
            CLIENTEDGE = 0x00000200,
            COMPOSITED = 0x02000000,
            CONTEXTHELP = 0x00000400,
            CONTROLPARENT = 0x00010000,
            DLGMODALFRAME = 0x00000001,
            LAYERED = 0x00080000,
            LAYOUTRTL = 0x00400000,
            LEFT = 0x00000000,
            LEFTSCROLLBAR = 0x00004000,
            LTRREADING = 0x00000000,
            MDICHILD = 0x00000040,
            NOACTIVATE = 0x08000000,
            NOINHERITLAYOUT = 0x00100000,
            NOPARENTNOTIFY = 0x00000004,
            NOREDIRECTIONBITMAP = 0x00200000,
            OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,
            PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,
            RIGHT = 0x00001000,
            RIGHTSCROLLBAR = 0x00000000,
            RTLREADING = 0x00002000,
            STATICEDGE = 0x00020000,
            TOOLWINDOW = 0x00000080,
            TOPMOST = 0x00000008,
            TRANSPARENT = 0x00000020,
            WINDOWEDGE = 0x00000100
        }
        [Flags]
        public enum LimitFlags : uint
        {
            ACTIVE_PROCESS = 0x00000008,
            AFFINITY = 0x00000010,
            BREAKAWAY_OK = 0x00000800,
            DIE_ON_UNHANDLED_EXCEPTION = 0x00000400,
            JOB_MEMORY = 0x00000200,
            JOB_TIME = 0x00000004,
            KILL_ON_JOB_CLOSE = 0x00002000,
            PRESERVE_JOB_TIME = 0x00000040,
            PRIORITY_CLASS = 0x00000020,
            PROCESS_MEMORY = 0x00000100,
            PROCESS_TIME = 0x00000002,
            SCHEDULING_CLASS = 0x00000080,
            SILENT_BREAKAWAY_OK = 0x00001000,
            WORKINGSET = 0x00000001,
        }

        public enum JOBOBJECTINFOCLASS
        {
            JobObjectBasicAccountingInformation = 1,
            JobObjectBasicLimitInformation = 2,
            JobObjectBasicProcessIdList = 3,
            JobObjectBasicUIRestrictions = 4,
            JobObjectSecurityLimitInformation = 5,
            JobObjectEndOfJobTimeInformation = 6,
            JobObjectAssociateCompletionPortInformation = 7,
            JobObjectBasicAndIoAccountingInformation = 8,
            JobObjectExtendedLimitInformation = 9,
            JobObjectJobSetInformation = 10,
            JobObjectGroupInformation = 11,
            JobObjectNotificationLimitInformation = 12,
            JobObjectLimitViolationInformation = 13,
            JobObjectGroupInformationEx = 14,
            JobObjectCpuRateControlInformation = 15,
            JobObjectCompletionFilter = 16,
            JobObjectCompletionCounter = 17,
            JobObjectFreezeInformation = 18,
            JobObjectExtendedAccountingInformation = 19,
            JobObjectWakeInformation = 20,
            JobObjectBackgroundInformation = 21,
            JobObjectSchedulingRankBiasInformation = 22,
            JobObjectTimerVirtualizationInformation = 23,
            JobObjectCycleTimeNotification = 24,
            JobObjectClearEvent = 25,
            JobObjectInterferenceInformation = 26,
            JobObjectClearPeakJobMemoryUsed = 27,
            JobObjectMemoryUsageInformation = 28,
            JobObjectSharedCommit = 29,
            JobObjectContainerId = 30,
            JobObjectIoRateControlInformation = 31,
            JobObjectSiloRootDirectory = 37,
            JobObjectServerSiloBasicInformation = 38,
            JobObjectServerSiloUserSharedData = 39,
            JobObjectServerSiloInitialize = 40,
            JobObjectServerSiloRunningState = 41,
            JobObjectIoAttribution = 42,
            JobObjectMemoryPartitionInformation = 43,
            JobObjectContainerTelemetryId = 44,
            JobObjectSiloSystemRoot = 45,
            JobObjectEnergyTrackingState = 46,
            JobObjectThreadImpersonationInformation = 47,
            JobObjectNetRateControlInformation = 32,
            JobObjectNotificationLimitInformation2 = 33,
            JobObjectLimitViolationInformation2 = 34,
            JobObjectCreateSilo = 35,
            MaxJobObjectInfoClass = 48
        }

        [Flags]
        public enum STARTF : uint
        {
            USESHOWWINDOW = 0x00000001,
            USESIZE = 0x00000002,
            USEPOSITION = 0x00000004,
            USECOUNTCHARS = 0x00000008,
            USEFILLATTRIBUTE = 0x00000010,
            RUNFULLSCREEN = 0x00000020,
            FORCEONFEEDBACK = 0x00000040,
            FORCEOFFFEEDBACK = 0x00000080,
            USESTDHANDLES = 0x00000100,
            USEHOTKEY = 0x00000200,
            TITLEISLINKNAME = 0x00000800,
            TITLEISAPPID = 0x00001000,
            PREVENTPINNING = 0x00002000,
            UNTRUSTEDSOURCE = 0x00008000,
        }

        public enum ShowWindowType : short
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            NORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            MAXIMIZE = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
            FORCEMINIMIZE = 11,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct StartupInfo
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public STARTF dwFlags;
            public ShowWindowType wShowWindow;
            public short cbReserved2;
            public nint lpReserved2;
            public nint hStdInput;
            public nint hStdOutput;
            public nint hStdError;
        }

        public enum UDP_TABLE_CLASS : byte
        {
            BASIC = 0,
            OWNER_PID = 1,
            OWNER_MODULE = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPROW_OWNER_PID
        {
            public uint localAddr;
            public uint localPort;
            public uint owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] localAddr;
            public uint localScopeId;
            public uint localPort;
            public uint owningPid;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(nint hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            nint lpProcessAttributes,
            nint lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            nint lpEnvironment,
            string lpCurrentDirectory,
            [In] ref StartupInfo lpStartupInfo,
            ref ProcessInfo lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(nint hWnd, WindowLongParam nIndex, uint dwNewLong);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateJobObjectW(nint jobAttributes, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool SetInformationJobObject(nint hJob, JOBOBJECTINFOCLASS jobObjectInfoClass, nint lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll")]
        public static extern bool AssignProcessToJobObject(nint hJob, nint hProcess);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedUdpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            UDP_TABLE_CLASS tblClass,
            uint reserved = 0);
    }
}
