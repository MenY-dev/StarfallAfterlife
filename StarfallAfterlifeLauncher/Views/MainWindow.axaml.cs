using StarfallAfterlife.Launcher.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using StarfallAfterlife.Launcher.ViewModels;
using StarfallAfterlife.Launcher.MapEditor;
using StarfallAfterlife.Launcher.MobsEditor;

namespace StarfallAfterlife.Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
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

    public override void Show()
    {
        base.Show();
        (DataContext as AppViewModel)?.CheckUpdates();
    }
}
