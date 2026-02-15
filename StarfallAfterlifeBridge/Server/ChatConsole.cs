using StarfallAfterlife.Bridge.Networking.Channels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StarfallAfterlife.Bridge.Mathematics.Triangulator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server
{
    public struct ChatConsoleContext
    {
        public string Input { get; }

        public string Channel { get; }

        public ChatConsole Console { get; }

        public SfaServerClient Client => Console?.Client;

        public bool TryParce<T>(out T result)
            where T : IParsable<T> =>
            T.TryParse(Input, CultureInfo.InvariantCulture, out result);

        public Nullable<T> Parce<T>() where T : struct, IParsable<T> =>
            TryParce<T>(out var result) ? result : null;

        public (T1, T2)? Parce<T1, T2>()
            where T1 : IParsable<T1>
            where T2 : IParsable<T2>
        {
            var value = Input?.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var format = CultureInfo.InvariantCulture;

            if (value is null) return null;

            var result = (
                default(T1),
                default(T2));

            if (T1.TryParse(value.ElementAtOrDefault(0), format, out result.Item1) == false ||
                T2.TryParse(value.ElementAtOrDefault(1), format, out result.Item2) == false)
                return null;

            return result;
        }

        public (T1, T2, T3)? Parce<T1, T2, T3>()
            where T1 : IParsable<T1>
            where T2 : IParsable<T2>
            where T3 : IParsable<T3>
        {
            var value = Input?.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var format = CultureInfo.InvariantCulture;

            if (value is null) return null;

            var result = (
                default(T1),
                default(T2),
                default(T3));

            if (T1.TryParse(value.ElementAtOrDefault(0), format, out result.Item1) == false ||
                T2.TryParse(value.ElementAtOrDefault(1), format, out result.Item2) == false ||
                T3.TryParse(value.ElementAtOrDefault(2), format, out result.Item3) == false)
                return null;

            return result;
        }

        public (T1, T2, T3, T4)? Parce<T1, T2, T3, T4>()
            where T1 : IParsable<T1>
            where T2 : IParsable<T2>
            where T3 : IParsable<T3>
            where T4 : IParsable<T4>
        {
            var value = Input?.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var format = CultureInfo.InvariantCulture;

            if (value is null) return null;

            var result = (
                default(T1),
                default(T2),
                default(T3),
                default(T4));

            if (T1.TryParse(value.ElementAtOrDefault(0), format, out result.Item1) == false ||
                T2.TryParse(value.ElementAtOrDefault(1), format, out result.Item2) == false ||
                T3.TryParse(value.ElementAtOrDefault(2), format, out result.Item3) == false ||
                T4.TryParse(value.ElementAtOrDefault(3), format, out result.Item4) == false)
                return null;

            return result;
        }

        public ChatConsoleContext(string input, string channel, ChatConsole console)
        {
            Input = input;
            Channel = channel;
            Console = console;
        }

        public void Print(object obj, string label = ">") => Console?.Print(obj, Channel, label);

        public void Print(string text, string label = ">") => Console.Print(text, Channel, label);

        public void PrintParametersError() => Console.Print("Invalid parameters!", Channel);

        public void PrintInternalError() => Console.Print("Internal error!", Channel);
    }

    public class ChatConsole
    {
        public delegate void CommandHandler(ChatConsoleContext context);

        public SfaServerClient Client;

        protected DebugCommandNode Nodes { get; } = new DebugCommandNode() { Name = "" };

        public ChatConsole(SfaServerClient client)
        {
            Client = client;
        }

        public void Exec(string command, string channel)
        {
            command = command?.TrimStart('\\');

            if (string.IsNullOrWhiteSpace(command) || Client == null)
                return;

            if (Client.IsChatConsoleAvailable == false)
            {
                Print("The console is unavailable.", channel, "SERVER");
                return;
            }

            Nodes.Exec(new ChatConsoleContext(command, channel, this));
        }

        public void Print(object obj, string channel, string label = ">") =>
            Print(obj?.ToString() ?? "NULL", channel, label);

        public void Print(string text, string channel, string label = ">")
        {
            Client?.SendToChat(channel, label, text);
        }

        public void AddHandler(string command, CommandHandler handler)
        {
            Nodes.AddHandler(command, handler);
        }

        protected class DebugCommandNode
        {
            public string Name;

            public List<DebugCommandNode> ChildNodes;

            public List<CommandHandler> Handlers;

            public void Exec(ChatConsoleContext context)
            {
                if (context.Input is null)
                    return;

                var input = context.Input.TrimStart();
                var data = string.Empty;
                var spaceIndex = input.IndexOf(' ');

                if (spaceIndex > 0)
                {
                    data = input[spaceIndex..].TrimStart();
                    input = input[..spaceIndex].TrimEnd();
                }
                else
                {
                    input = input.TrimEnd();
                }

                if (string.IsNullOrWhiteSpace(input) == false)
                {
                    foreach (var node in (ChildNodes ??= new()).ToArray())
                    {
                        if (node.Name?.Equals(input, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            node.Exec(new(data, context.Channel, context.Console));
                            return;
                        }
                    }
                }


                foreach (var handler in (Handlers ??= new()).ToArray())
                {
                    context.Client?.Invoke(c =>
                    {
                        try
                        {
                            handler.Invoke(context);
                        }
                        catch (Exception e)
                        {
                            context.Print(e);
                        }
                    });
                }
            }

            public void AddHandler(string command, CommandHandler handler)
            {
                if (handler is null)
                    return;

                if (string.IsNullOrWhiteSpace(command) == true)
                {
                    (Handlers ??= new()).Add(handler);
                    return;
                }

                command = command.Trim();

                var spaceIndex = command.IndexOf(' ');
                var data = spaceIndex > -1 ? command[spaceIndex..].TrimStart() : null;
                command = spaceIndex > -1 ? command[..spaceIndex].TrimEnd() : command;

                foreach (var node in (ChildNodes ??= new()).ToArray())
                {
                    if (node.Name?.Equals(command, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        node.AddHandler(data, handler);
                        return;
                    }
                }

                var newNode = new DebugCommandNode { Name = command };
                newNode.AddHandler(data, handler);
                (ChildNodes ??= new()).Add(newNode);
            }

            public void RemoveHandler(CommandHandler handler)
            {
                Handlers?.Remove(handler);

                foreach (var node in (ChildNodes ??= new()).ToArray())
                    node.RemoveHandler(handler);
            }
        }
    }
}
