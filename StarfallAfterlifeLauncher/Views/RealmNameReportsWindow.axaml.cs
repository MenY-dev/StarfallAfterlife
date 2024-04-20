using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Timers;

namespace StarfallAfterlife.Launcher.Views
{
    public partial class RealmNameReportsWindow : SfaWindow
    {
        private Timer _updateTimer;

        public RealmNameReportsWindow()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ContentProperty)
                DataContext = Content;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            StartAutoUpdate();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            StopAutoUpdate();
        }

        private void OnUptateTimerTick(object sender, ElapsedEventArgs e) => Dispatcher.UIThread.Invoke(() =>
        {
            if (IsLoaded == false)
            {
                StopAutoUpdate();
                return;
            }

            (Content as RealmNameReportsViewModel)?.UpdateReports();
        });

        public void StartAutoUpdate()
        {
            StopAutoUpdate();

            var timer = _updateTimer = new(TimeSpan.FromSeconds(10));
            timer.Elapsed += OnUptateTimerTick;
            timer.Start();
        }

        public void StopAutoUpdate()
        {
            var timer = _updateTimer;

            if (timer is not null)
            {
                _updateTimer = null;
                timer.Stop();
                timer.Elapsed -= OnUptateTimerTick;
            }
        }
    }
}
