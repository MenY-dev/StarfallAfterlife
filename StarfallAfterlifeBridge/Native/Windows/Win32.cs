using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct ProcessInfo
        {
            public nint hProcess;
            public nint hThread;
            public int ProcessId;
            public int ThreadId;
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
