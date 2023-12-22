using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Generators;
using System;
using System.Diagnostics;

namespace StarfallAfterlife.Launcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //DefaultGalaxyMapGenerator.Build();

            //AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            //{
            //    if (e.Exception is not InvalidOperationException)
            //        return;

            //    SfaDebug.Print(e.Exception, "Exception");
            //};

            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            Background = null;

            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change is not null &&
                change.Property == OffScreenMarginProperty &&
                change.NewValue is Thickness padding)
            {
                Padding = padding;
            }
        }

        public void OnCloseBtnClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void OnMaxMinBtnClicked(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
        }

        public void OnHideBtnClicked(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void OnWindowDragStarted(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnMaxMinBtnClicked(sender, e);
            }

            BeginMoveDrag(e);
        }
    }
}