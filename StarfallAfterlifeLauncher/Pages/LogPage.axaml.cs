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
using StarfallAfterlife.Bridge.Tasks;
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

        public static readonly StyledProperty<bool> UseAutoscrollProperty =
            AvaloniaProperty.Register<LogPage, bool>(nameof(UseAutoscroll), true);

        public SfaDebugMsgStorage DebugMsgStorage => GetValue(DebugMsgStorageProperty);

        public bool UseAutoscroll
        {
            get => GetValue(UseAutoscrollProperty);
            set => SetValue(UseAutoscrollProperty, value);
        }

        protected override Type StyleKeyOverride => typeof(SidebarPage);

        private ConcurrentQueue<SfaDebugMsgViewModel> _queue = new();
        private object _locker = new();
        private bool _releseStarted = false;
        private VirtualizingStackPanel _virtualizingPanel;
        private ScrollViewer _scroll;
        private bool _fullScrollStarted;
        private int _fullScrollTakeCount;
        private Process _currentConsole;


        public LogPage()
        {
            DataContext = this;

            InitializeComponent();

            SfaDebug.Update += OnSfaDebugUpdate;
        }

        public void ScrollDown()
        {
            _virtualizingPanel ??= Output.FindDescendantOfType<VirtualizingStackPanel>(true);
            _scroll ??= Output.FindDescendantOfType<ScrollViewer>(true);

            try
            {
                if (_virtualizingPanel is not null &&
                    _scroll is not null &&
                    IsEffectivelyVisible == true)
                {
                    _fullScrollTakeCount = 0;
                    _fullScrollStarted = true;
                    _scroll.ScrollChanged -= OnLogScrollChanged;
                    _scroll.ScrollChanged += OnLogScrollChanged;
                    _scroll.ScrollToEnd();
                }
            }
            catch
            {
                _fullScrollTakeCount = 0;
                _fullScrollStarted = false;
            }
        }

        private void OnLogScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_fullScrollStarted == false ||
                _scroll is null)
                return;

            try
            {
                var needScroll = Math.Abs(_scroll.Offset.Y - _scroll.Extent.Height + _scroll.Viewport.Height) != 0;

                if (needScroll == true &&
                    UseAutoscroll == true &&
                    _fullScrollTakeCount < 4)
                {
                    _fullScrollTakeCount++;
                    _scroll.ScrollToEnd();
                }
                else
                {
                    _fullScrollStarted = false;
                    _fullScrollTakeCount = 0;
                    _scroll.ScrollChanged -= OnLogScrollChanged;
                }
            }
            catch
            {
                _fullScrollTakeCount = 0;
                _fullScrollStarted = false;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == UseAutoscrollProperty ||
                change.Property == IsShowProperty)
            {
                if (UseAutoscroll == true &&
                    IsShow == true)
                    Dispatcher.UIThread.Post(() => ScrollDown());
            }
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

                foreach (var item in result)
                {
                    WriteToConsole(item.ToString(300));
                    WriteToConsole(Environment.NewLine);
                }

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

                        if (UseAutoscroll == true)
                            ScrollDown();
                    });
                }
                catch { }
            });
        }

        public void SaveLog()
        {
            var sb = new StringBuilder();
            var nl = Environment.NewLine;

            foreach (var item in DebugMsgStorage)
            {
                if (item is null)
                    continue;

                sb.Append(item.ToString());
                sb.Append(nl);
            }

            App.MainWindow?.StorageProvider.SaveFilePickerAsync(new()
            {
                DefaultExtension = ".txt",
                SuggestedFileName = "log",
                FileTypeChoices = new[] { new FilePickerFileType("TXT") { Patterns = new[] { "*.txt" } } }
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

        public void WriteToConsole(string text)
        {
            try
            {
                if (_currentConsole?.HasExited == false &&
                    _currentConsole?.StandardInput is StreamWriter writer)
                    writer.Write(text);
            }
            catch { }
        }

        public void OpenConsole()
        {
            try
            {
                if (_currentConsole is not null &&
                    _currentConsole.HasExited == false)
                    return;

                var proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = Process.GetCurrentProcess()?.MainModule?.FileName,
                        Arguments = "-LogConsole",
                        UseShellExecute = false,
                        StandardInputEncoding = Encoding.Unicode,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = false,
                    },
                    EnableRaisingEvents = true,
                };

                proc.Exited += (s, e) =>
                {
                    if (_currentConsole == s)
                        _currentConsole = null;
                };

                proc.Start();
                _currentConsole = proc;
                proc.StandardInput.AutoFlush = true;
            }
            catch (Exception e)
            {
                SfaDebug.Log(e.ToString());
            }
            
        }
    }
}
