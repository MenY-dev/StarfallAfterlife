using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using StarfallAfterlife.Bridge.Networking;

namespace StarfallAfterlife.Launcher;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Any(i => i?.Contains("-LogConsole", StringComparison.InvariantCultureIgnoreCase) == true))
        {
            LogConsoleProgram.Start(args);
            return;
        }

        var procName = Process.GetCurrentProcess().ProcessName;

        if (Process.GetProcesses().Count(p => p.ProcessName == procName) > 1)
            return;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        NatPuncher.CloseAll().Wait(1000);
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
