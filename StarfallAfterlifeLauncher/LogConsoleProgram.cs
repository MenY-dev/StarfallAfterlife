using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher
{
    public static class LogConsoleProgram
    {
        public static void Start(string[] args)
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

                            while (buffer.TryDequeue(out var line) == true)
                            {
                                if (line is not null)
                                {
                                    Console.WriteLine(line);
                                    skipped = 0;
                                }
                            }
                        }
                    }
                    catch { }
                });

                while (true)
                {
                    monitor.Reset();

                    var line = reader.ReadLine();

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
}
