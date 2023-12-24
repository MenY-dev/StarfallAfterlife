using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Launcher.ViewModels;
using StarfallAfterlife.Launcher.Views;
using System;
using System.IO;

namespace StarfallAfterlife.Launcher;

public partial class App : Application
{
    public static SfaLauncher Launcher { get; protected set; }

    public static MainWindow MainWindow { get; protected set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Launcher ??= new SfaLauncher()
        {
            Database = SfaDatabase.Instance,
            //GameDirectory = "V:\\StarfallAssets\\Starfall Online",
            WorkingDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "My Games",
                    "StarfallAfterlife"),
        };

        Launcher.Load();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new AppViewModel(Launcher);
            MainWindow = new MainWindow() { DataContext = viewModel };
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
