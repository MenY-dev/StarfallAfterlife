using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using StarfallAfterlife.Launcher.Services;
using System;
using System.Threading;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class InstallReleasePopup : SfaPopup
    {
        public static readonly StyledProperty<int> FileSizeProperty =
            AvaloniaProperty.Register<InstallReleasePopup, int>(nameof(FileSize), 0);

        public static readonly StyledProperty<int> FileProgressProperty =
            AvaloniaProperty.Register<InstallReleasePopup, int>(nameof(FileProgress), 0);

        protected override Type StyleKeyOverride => typeof(SfaPopup);

        public int FileSize
        {
            get => GetValue(FileSizeProperty);
            set => SetValue(FileSizeProperty, value);
        }

        public int FileProgress
        {
            get => GetValue(FileProgressProperty);
            set => SetValue(FileProgressProperty, value);
        }

        private CancellationTokenSource _cts = new(); 
        private Updater.Relese _relese; 

        public InstallReleasePopup()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void Install(Updater.Relese relese)
        {
            if (relese is null || _cts.IsCancellationRequested)
                return;

            _relese = relese;
            FileSize = (int)(_relese.Size / 1024);

            ShowDialog();

            var downloading = relese.Download(
                l => Dispatcher.UIThread.Invoke(() => FileSize = (int)(l / 1024)),
                new Progress<long>(p => Dispatcher.UIThread.Invoke(() => FileProgress = (int)(p / 1024))),
                _cts.Token);

            downloading.ContinueWith(t =>
            {
                if (_cts.IsCancellationRequested ||
                    t.Result == false)
                    return;

                relese.Install().ContinueWith(t => 
                {
                    if (t.Result == false)
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            SfaMessageBox.ShowDialog(
                                "Installation Error!",
                                "An error occurred during installation.");
                        });
                });
            });

            return;
        }

        public static void InstallRelese(Updater.Relese relese) => 
            new InstallReleasePopup().Install(relese);

        public void Cancell()
        {
            _cts?.Cancel();
            Close();
        }
    }
}
