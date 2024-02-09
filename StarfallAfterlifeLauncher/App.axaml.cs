using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Utilities;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Launcher.ViewModels;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StarfallAfterlife.Launcher;

public partial class App : Application
{
    public static SfaLauncher Launcher { get; protected set; }

    public static MainWindow MainWindow { get; protected set; }

    public static IResourceDictionary Localizations { get; protected set; }

    public static string CurrentLocalization
    {
        get => _currentLocalization;
        set => SetLocalization(value);
    }

    private static string _currentLocalization;

    public override void Initialize()
    {
        LoadLocalizations();
        SetSystemLocalization();

        Launcher ??= new SfaLauncher()
        {
            Database = SfaDatabase.Instance,
            WorkingDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                    "My Games",
                    "StarfallAfterlife"),
        };

        Launcher.Load();

        if ((string)Launcher.SettingsStorage["localization"] is string currentLoc)
            CurrentLocalization = currentLoc;
        else
            Launcher.SettingsStorage["localization"] = CurrentLocalization;

        AvaloniaXamlLoader.Load(this);
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

    private static void LoadLocalizations()
    {
        try
        {
            var asemblyName = typeof(App).Assembly.GetName().Name;

            Localizations = new ResourceInclude(default(Uri))
            {
                Source = new Uri($"avares://{asemblyName}/Assets/Strings/Localizations.axaml")
            }?.Loaded;
        }
        catch { }

        Localizations ??= new ResourceDictionary();
    }

    public static void SetLocalization(string alias)
    {
        var app = Current as App;

        if (app is null || alias is null)
            return;

        var currentLoc = app.Resources.MergedDictionaries.FirstOrDefault(r =>
            Localizations?.Values.Contains(r) == true);

        if (Localizations.TryGetResource(alias, app.ActualThemeVariant, out var newLoc) == true &&
            newLoc is ResourceDictionary newLocDictionary &&
            currentLoc != newLocDictionary)
        {
            _currentLocalization = alias;
            app.Resources.MergedDictionaries.Add(newLocDictionary);
            app.Resources.MergedDictionaries.Remove(currentLoc);
        }
    }

    public static void SetSystemLocalization()
    {
        if (CultureInfo.CurrentUICulture?.Name is string name)
        {
            CurrentLocalization = Localizations?.FirstOrDefault(l => l.Key?.Equals(name) == true).Key as string;
        }

        if (CurrentLocalization is null &&
            CultureInfo.CurrentUICulture?.Parent?.Name is string parentName)
        {
            CurrentLocalization = Localizations?.FirstOrDefault(l => l.Key?.Equals(parentName) == true).Key as string;
        }

        if (CurrentLocalization is null)
        {
            CurrentLocalization ??= Localizations?.FirstOrDefault().Key as string;
        }
    }

    public static string GetString(string key)
    {
        if (Current.TryGetResource(key, out var resource) == true)
            return resource as string;

        return null;
    }
}
