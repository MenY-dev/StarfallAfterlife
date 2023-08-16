using StarfallAfterlife.Bridge.Native.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Environment
{
    public partial class SfaProcess
    {
        public void StartWin32Process()
        {
            var process = new WindowsProcess();
            var startupInfo = new Win32.StartupInfo();

            //if (Listen == true)
            //{
            //    startupInfo.dwFlags |= Win32.STARTF.USESTDHANDLES;
            //    startupInfo.dwFlags |= Win32.STARTF.USESHOWWINDOW;
            //    startupInfo.wShowWindow |= Win32.ShowWindowType.SHOWMINNOACTIVE;
            //};

            process.Start(null, $"{Executable} {BuildArguments()}", startupInfo);
            InnerProcess = process.SharpProcess;

            if (Listen == true && process.StandartOutput is StreamReader output)
            {
                BeginReadingOutput(output);
                //HideWin32Window();
            }
        }

        public void HideWin32Window()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (InnerProcess.WaitForInputIdle(TimeSpan.FromMinutes(1)) == false)
                        return;

                    nint handle = InnerProcess.MainWindowHandle;

                    if (handle == 0 || InnerProcess.HasExited == true)
                        return;

                    Win32.SetWindowLong(
                        handle,
                        Win32.WindowLongParam.GWL_STYLE,
                        (uint)(Win32.WindowStyles.POPUPWINDOW | Win32.WindowStyles.MINIMIZE));

                    Win32.SetWindowLong(
                        handle,
                        Win32.WindowLongParam.GWL_EXSTYLE,
                        (uint)(Win32.WindowStylesEx.TOOLWINDOW | Win32.WindowStylesEx.NOACTIVATE));
                }
                catch
                {
                    return;
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
