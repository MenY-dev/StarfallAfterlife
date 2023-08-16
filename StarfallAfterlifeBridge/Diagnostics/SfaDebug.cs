using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Diagnostics
{
    public class SfaDebug
    {
        public static void Print(object obj, string channel = null)
        {
            Print(obj?.ToString() ?? "NULL!", channel);
        }

        public static void Print(string msg, string channel = null)
        {
            string line = $"[{DateTime.Now:T}][{channel ?? "Log"}] {msg ?? string.Empty}";
            Console.WriteLine(line);
            Trace.WriteLine(line);
        }


        public static void Log(
            string msg = null,
            [CallerFilePath]string file = null,
            [CallerMemberName]string member = null,
            [CallerLineNumber]int line = 0)
        {
            var tag = $"{Path.GetFileName(file ?? string.Empty)}.{member}:{line}";
            var text = $"[{DateTime.Now:T}][{tag}] {msg ?? string.Empty}";
            Console.WriteLine(text);
            Trace.WriteLine(text);
        }
    }
}
