﻿using System;
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
        public delegate void NewDebugMsgDelegate(string msg, string channel, DateTime time);


        public static event NewDebugMsgDelegate Update;

        public static void Print(object obj, string channel = null)
        {
            Print(obj?.ToString() ?? "NULL!", channel);
        }

        public static void Print(string msg, string channel = null)
        {
            var time = DateTime.Now;
            channel ??= "Log";
            string line = $"[{time:T}][{channel}] {msg ?? string.Empty}";

            Console.WriteLine(line);
            Trace.WriteLine(line);
            Update?.Invoke(msg, channel, time);
        }


        public static void Log(
            string msg = null,
            [CallerFilePath]string file = null,
            [CallerMemberName]string member = null,
            [CallerLineNumber]int line = 0)
        {
            var time = DateTime.Now;
            var tag = $"{Path.GetFileName(file ?? string.Empty)}.{member}:{line}";
            var text = $"[{DateTime.Now:T}][{tag}] {msg ?? string.Empty}";

            Console.WriteLine(text);
            Trace.WriteLine(text);
            Update?.Invoke(msg, tag, time);
        }
    }
}
