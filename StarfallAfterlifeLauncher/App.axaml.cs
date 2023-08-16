using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.IO;

namespace StarfallAfterlife.Launcher
{
    public partial class App : Application
    {
        public static SfaLauncher Launcher { get; protected set; }

        public static MainWindow MainWindow { get; protected set; }

        public override void Initialize()
        {
            RequestedThemeVariant = ThemeVariant.Light;
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
}