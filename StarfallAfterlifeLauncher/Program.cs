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
            StartAsLogConsole();
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

    private static void StartAsLogConsole()
    {
        try
        {
            AllocConsole();

            var nl = Environment.NewLine;
            long skipped = 0;
            var monitor = new ManualResetEvent(false);
            var bufferSize = 1000;
            var buffer = new Queue<string>();
            using var stdin = Console.OpenStandardInput();
            using var reader = new StreamReader(
                Console.OpenStandardInput(),
                Console.InputEncoding,
                false,
                2048);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        monitor.WaitOne();
                        
                        if (buffer.TryDequeue(out var line) == true &&
                            line is not null)
                        {
                            Console.WriteLine(line);
                            skipped = 0;
                        }
                    }
                }
                catch { }
            });

            while (true)
            {
                var line = reader.ReadLine();

                monitor.Reset();

                if (line is not null)
                    buffer.Enqueue(line);

                if (buffer.Count >= bufferSize)
                {
                    skipped += bufferSize;
                    buffer.Clear();
                    buffer.Enqueue($"{nl}[Skipped {skipped} lines!]{nl}");
                }

                monitor.Set();
            }
        }
        catch { }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();
}
