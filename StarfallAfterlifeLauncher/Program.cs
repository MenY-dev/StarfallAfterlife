using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;

namespace StarfallAfterlife.Launcher;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var procName = Process.GetCurrentProcess().ProcessName;

        if (Process.GetProcesses().Count(p => p.ProcessName == procName) > 1)
            return;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
