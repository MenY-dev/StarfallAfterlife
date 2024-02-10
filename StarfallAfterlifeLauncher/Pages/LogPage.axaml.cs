using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace StarfallAfterlife.Launcher.Pages
{
    public partial class LogPage : SidebarPage
    {
        public static readonly StyledProperty<SfaDebugMsgStorage> DebugMsgStorageProperty =
            AvaloniaProperty.Register<LogPage, SfaDebugMsgStorage>(nameof(DebugMsgStorage), new());

        public static readonly StyledProperty<bool> IsNeedAutoscrollProperty =
            AvaloniaProperty.Register<LogPage, bool>(nameof(IsNeedAutoscroll), false);

        public SfaDebugMsgStorage DebugMsgStorage => GetValue(DebugMsgStorageProperty);

        public bool IsNeedAutoscroll
        {
            get => GetValue(IsNeedAutoscrollProperty);
            set => SetValue(IsNeedAutoscrollProperty, value);
        }

        protected override Type StyleKeyOverride => typeof(SidebarPage);

        private ConcurrentQueue<SfaDebugMsgViewModel> _queue = new();
        private object _locker = new();
        private bool _releseStarted = false;
        private VirtualizingStackPanel _virtualizingPanel;


        public LogPage()
        {
            DataContext = this;

            InitializeComponent();

            SfaDebug.Update += OnSfaDebugUpdate;
        }

        public void ScrollDown()
        {
            _virtualizingPanel ??= Output.FindDescendantOfType<VirtualizingStackPanel>(true);

            if (_virtualizingPanel is null)
                Output.ScrollIntoView(DebugMsgStorage.Count - 1);
            else
            {
                for (int i = 0; i < 100; i++)
                {
                    Output.ScrollIntoView(DebugMsgStorage.Count - 1);

                    if (_virtualizingPanel.LastRealizedIndex >= (DebugMsgStorage.Count - 1))
                        break;
                }
            }
        }

        public void SaveLog()
        {
            var sb = new StringBuilder();
            var nl = Environment.NewLine;

            foreach (var item in DebugMsgStorage)
            {
                if (item is null)
                    continue;

                sb.Append($"[{item.Time:T}][{item.Channel ?? "Log"}]");

                if (item.Count > 1)
                {
                    sb.Append('[');
                    sb.Append(item.Count);
                    sb.Append('}');
                }

                sb.Append(' ');
                sb.Append(item.Msg ?? "");
                sb.Append(nl);
            }

            App.MainWindow?.StorageProvider.SaveFilePickerAsync(new()
            {
                DefaultExtension = ".txt",
                SuggestedFileName = "log",
                FileTypeChoices = new[] { new FilePickerFileType("TXT") { Patterns = new[] { "*.txt" } }  }
            }).ContinueWith((Task<IStorageFile> t) =>
            {
                try
                {
                    var file = t.Result;
                    using var stream = file?.OpenWriteAsync().Result;

                    if (stream is not null)
                    {
                        using var writer = new StreamWriter(stream, Encoding.UTF8);
                        writer.Write(sb.ToString());
                    }
                }
                catch (Exception e)
                {
                    SfaDebug.Log(e.ToString());
                }
            });
        }

        private void OnSfaDebugUpdate(string msg, string channel, DateTime time)
        {
            _queue.Enqueue(new() { Msg = msg, Channel = channel, Time = time });
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

            var result = new List<SfaDebugMsgViewModel>(10);

            Task.Factory.StartNew(() =>
            {
                int appendCount = 0;

                do
                {
                    while (_queue.TryDequeue(out var item))
                    {
                        appendCount++;
                        result.Add(item);
                    }

                    Task.Delay(10).Wait();

                } while (_queue.Count > 0 && appendCount < 10);

                lock (_locker)
                {
                    _releseStarted = false;
                }

                try
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (DebugMsgStorage is SfaDebugMsgStorage output)
                        {
                            foreach (var item in result)
                                output.Add(item);
                        }
                    });
                }
                catch { }
            });
        }
    }
}
