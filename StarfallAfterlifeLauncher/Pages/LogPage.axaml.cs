using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class LogPage : SidebarPage
    {
        protected override Type StyleKeyOverride => typeof(SidebarPage);

        protected SfaTextBox Output {  get; set; }

        public LogPage()
        {
            InitializeComponent();
            Output = this.Find<SfaTextBox>("LogOutput");
            Trace.Listeners.Add(new SfaTraceListener(Output));
        }

        public class SfaTraceListener : TraceListener
        {
            public SfaTextBox Output { get; }
            private ConcurrentQueue<string> _queue = new();
            private object _locker = new();
            private bool _releseStarted = false;

            public SfaTraceListener(SfaTextBox output)
            {
                Output = output;
            }

            public override void Write(string message)
            {
                _queue.Enqueue(message ?? string.Empty);
                ReleseQueue();
            }

            public override void WriteLine(string message)
            {
                _queue.Enqueue((message ?? string.Empty) + Environment.NewLine);
                ReleseQueue();
            }

            private void ReleseQueue()
            {
                lock (_locker)
                {
                    if (_releseStarted == true)
                        return;

                    _releseStarted = true;
                }

                var output = new StringBuilder();

                Task.Factory.StartNew(() =>
                {
                    int appendCount = 0;

                    do
                    {
                        while (_queue.TryDequeue(out string item))
                        {
                            appendCount++;
                            output.Append(item);
                        }

                        Task.Delay(10).Wait();

                    } while (_queue.Count > 0 && appendCount < 100);

                    lock (_locker)
                    {
                        _releseStarted = false;
                    }

                    Dispatcher.UIThread.Invoke(() => SendToOutput(output.ToString()));
                });
            }

            private void SendToOutput(string message)
            {
                if (Output != null)
                {
                    var text = Output.Text ?? string.Empty;
                    var maxLength = 100000;
                    text += message;

                    if (text.Length > maxLength)
                        text = text.Substring(text.Length - maxLength, maxLength);

                    Output.Text = text;

                    if (Output.FindDescendantOfType<TextPresenter>() is TextPresenter presenter)
                        Output.ScrollToLine(Math.Max(0, (presenter.TextLayout?.TextLines?.Count ?? 1) - 1));
                }
            }
        }
    }
}
