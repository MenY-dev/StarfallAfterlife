using StarfallAfterlife.Bridge.Native.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;

namespace StarfallAfterlife.Bridge.Environment
{
    public partial class SfaProcess
    {
        public void StartWin32Process()
        {
            WinProcess = new WindowsProcess() { AttachToMainProcess = true };
            var startupInfo = new StartupInfo();

            if (Listen == true)
            {
                startupInfo.dwFlags |= STARTF.USESTDHANDLES;
                startupInfo.dwFlags |= STARTF.USESHOWWINDOW;
                startupInfo.wShowWindow |= ShowWindowType.SHOWMINNOACTIVE;
            };

            WinProcess.Start(null, $"{Executable} {BuildArguments()}", startupInfo);

            if (Listen == true)
            {
                if (WinProcess.StandartOutput is StreamReader output)
                    BeginReadingOutput(output);

                HideWin32Window();
            }
        }

        public void HideWin32Window()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    nint handle = nint.Zero;

                    while ((handle = WinProcess.MainWindowHandle) == nint.Zero)
                    {
                        if (WinProcess is null or { HasExited: true })
                            return;

                        Thread.Sleep(100);
                    }

                    SetWindowLong(
                        handle,
                        WindowLongParam.GWL_STYLE,
                        (uint)(WindowStyles.POPUPWINDOW | WindowStyles.MINIMIZE));

                    SetWindowLong(
                        handle,
                        WindowLongParam.GWL_EXSTYLE,
                        (uint)(WindowStylesEx.TOOLWINDOW | WindowStylesEx.NOACTIVATE));
                }
                catch
                {
                    return;
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
