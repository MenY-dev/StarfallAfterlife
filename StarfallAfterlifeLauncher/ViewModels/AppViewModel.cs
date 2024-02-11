using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.MapEditor;
using StarfallAfterlife.Launcher.MobsEditor;
using StarfallAfterlife.Launcher.Services;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        public SfaLauncher Launcher { get; protected set; }

        public ObservableCollection<RealmInfoViewModel> Realms { get; } = new();

        public ObservableCollection<SfaSession> Sessions { get; } = new();

        public bool IsGameStarted
        {
            get => _isGameStarted;
            protected set => SetAndRaise(ref _isGameStarted, value);
        }

        public RealmInfoViewModel SelectedLocalRealm
        {
            get => _selectedLocalRealm;
            set
            {
                SetAndRaise(ref _selectedLocalRealm, value);
                Launcher.CurrentLocalRealm = value?.RealmInfo;
            }
        }

        public RealmInfoViewModel SelectedServerRealm
        {
            get => _selectedServerRealm;
            set
            {
                SetAndRaise(ref _selectedServerRealm, value);
                Launcher.ServerRealm = value?.RealmInfo;
            }
        }

        public bool ForceGameWindowed
        {
            get => Launcher?.ForceGameWindowed ?? false;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.ForceGameWindowed, value, v => launcher.ForceGameWindowed = v);
            }
        }

        public bool ShowGameLog
        {
            get => Launcher?.ShowGameLog ?? false;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.ShowGameLog, value, v => launcher.ShowGameLog = v);
            }
        }

        public bool HideGameSplashScreen
        {
            get => Launcher?.HideGameSplashScreen ?? false;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.HideGameSplashScreen, value, v => launcher.HideGameSplashScreen = v);
            }
        }

        public bool HideGameLoadingScreen
        {
            get => Launcher?.HideGameLoadingScreen ?? false;
            set
            {
                if (Launcher is SfaLauncher launcher)
                    SetAndRaise(launcher.HideGameLoadingScreen, value, v => launcher.HideGameLoadingScreen = v);
            }
        }

        public bool UseAutoUpdate
        {
            get => (bool?)Launcher?.SettingsStorage["launcher_use_auto_update"] ?? true;
            set
            {
                if (Launcher is SfaLauncher launcher)
                {
                    SetAndRaise(UseAutoUpdate, value,
                        v => launcher.SettingsStorage["launcher_use_auto_update"] = v);

                    launcher.SaveSettings();
                }
            }
        }

        public bool IsUpdatePanelVisible
        {
            get => _isUpdatePanelVisible;
            set
            {
                SetAndRaise(ref _isUpdatePanelVisible, value);
            }
        }

        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                SetAndRaise(ref _isUpdateAvailable, value);
            }
        }

        public string CurrentFullVersion
        {
            get => $"v{SfaServer.Version.ToString(3)}";
        }

        public string CurrentVersion
        {
            get => $"v{SfaServer.Version.ToString(2)}";
        }

        public string CurrentBuild
        {
            get => SfaServer.Version.Build.ToString();
        }

        public Updater.Relese LatestRelese
        {
            get => _latestRelese;
            set
            {
                var relese = value;
                SetAndRaise(ref _latestRelese, value);

                IsUpdateAvailable = relese is not null &&
                                    relese.Version is not null &&
                                    SfaServer.Version < relese.Version;
            }
        }

        public ICollection<object> Localizations => App.Localizations.Keys;

        public string CurrentLocalization
        {
            get => (string)Launcher?.SettingsStorage["localization"];
            set
            {
                if (Launcher is SfaLauncher launcher)
                {
                    var localization = (string)launcher.SettingsStorage["localization"];

                    if (localization == value &&
                        App.CurrentLocalization == value)
                        return;

                    SetAndRaise(localization, value,
                        v => launcher.SettingsStorage["localization"] = App.CurrentLocalization = v);

                    launcher.SaveSettings();
                }
            }
        }

        private RealmInfoViewModel _selectedLocalRealm;
        private RealmInfoViewModel _selectedServerRealm;
        private bool _isGameStarted;
        private bool _isUpdatePanelVisible = false;
        private bool _isUpdateAvailable = false;
        private bool _isAutoUpdateCheckCompleted = false;
        private Updater.Relese _latestRelese;


        public string CurrentProfileName { get => currentProfileName; protected set => SetAndRaise(ref currentProfileName, value); }

        public string GameDirectory
        {
            get => Launcher?.GameDirectory;
            protected set
            {
                if (Launcher is SfaLauncher launcher)
                {
                    var oldValue = launcher.GameDirectory;
                    launcher.GameDirectory = value;
                    RaisePropertyChanged(oldValue, value, nameof(GameDirectory));
                }
            }
        }

        public SinglePlayerModePageViewModel SinglePlayerPageViewModel { get; }

        public FindServerPageViewModel FindServerPageViewModel { get; }

        public CreateServerPageViewModel CreateServerPageViewModel { get; }

        public ProfilesEditorViewModel ProfilesEditorViewModel { get; }

        private string currentProfileName;

        public AppViewModel()
        {
            if (Design.IsDesignMode)
            {
                ProfilesEditorViewModel = new ProfilesEditorViewModel(this);
            }
        }

        public AppViewModel(SfaLauncher launcher)
        {
            Launcher = launcher;
            CurrentProfileName = launcher?.CurrentProfile?.GameProfile?.Nickname;
            SinglePlayerPageViewModel = new SinglePlayerModePageViewModel(this);
            FindServerPageViewModel = new FindServerPageViewModel(this);
            CreateServerPageViewModel = new CreateServerPageViewModel(this);
            ProfilesEditorViewModel = new ProfilesEditorViewModel(this);
            UpdateRealms();
        }

        public void ShowProfileSelector()
        {
            ProfilesEditorViewModel.ShowEditor = true;
        }

        public void SelectProfile(SfaProfile profile)
        {
            if (Launcher is not null &&
                Launcher.Profiles?.Contains(profile) == true &&
                profile.IsSupported == true)
            {
                Launcher.CurrentProfile = profile;
                CurrentProfileName = Launcher?.CurrentProfile?.GameProfile?.Nickname;
                UpdateRealms();
            }
        }

        public Task ShowGameDirSelector()
        {
            var window = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;

            IStorageFolder GetCurrentFolder()
            {
                try
                {
                    return window.StorageProvider
                        .TryGetFolderFromPathAsync(GameDirectory ?? string.Empty)
                        .Result;
                }
                catch{ }

                return null;
            }

            return window.StorageProvider.OpenFolderPickerAsync(new()
            {
                AllowMultiple = false,
                Title = "Select game directory",
                SuggestedStartLocation = GetCurrentFolder()
            }).ContinueWith(t =>
            {
                if (t.Result?.FirstOrDefault()?.Path.LocalPath is string gameDir)
                {
                    if (Launcher is SfaLauncher launcher &&
                        launcher.TestGameDirectory(gameDir) == false)
                    {
                        try
                        {
                            if (Directory.Exists(gameDir) == true &&
                                FileHelpers.EnumerateDirectoriesSelf(gameDir) is IEnumerable<string> dirs)
                            {
                                dirs = dirs.Concat(dirs.SelectMany(FileHelpers.EnumerateDirectoriesSelf));
                                gameDir = dirs.FirstOrDefault(launcher.TestGameDirectory) ?? gameDir;
                            }
                        }
                        catch {}
                    }

                    GameDirectory = gameDir;
                    Launcher?.SaveSettings();
                }
            });
        }

        public Task<bool> MakeBaseTests(bool gameDirTest, bool profileTest)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var launcher = Launcher;

                    if (launcher is null)
                        return false;

                    if (gameDirTest == true)
                    {
                        if (launcher.TestGameDirectory() == false)
                            ShowGameDirSelector().Wait();

                        if (launcher.TestGameDirectory() == false)
                            return false;
                    }

                    if (profileTest == true)
                    {
                        if (launcher.Profiles.Count < 1)
                            CreateNewProfile().Wait();

                        if (launcher.Profiles.Count < 1)
                            return false;
                    }
                }
                catch
                {
                    return false;
                }
                
                return true;
            });
        }

        public Task CreateNewProfile()
        {
            return Dispatcher.UIThread.Invoke(() => new CreateProfilePopup().ShowDialog().ContinueWith(t =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (Launcher is SfaLauncher launcher &&
                        t.Result is CreateProfilePopup result &&
                        result.IsDone == true &&
                        result.IsValid == true &&
                        result.Text is not null)
                    {
                        var profile = Launcher?.CreateNewProfile(result.Text);

                        if (launcher.CurrentProfile is null && profile is not null)
                            SelectProfile(profile);
                    }

                    UpdateRealms();
                });
            }));
        }

        public bool IsSessionsCancellationRequired(string newRealmId) =>
            Launcher.CurrentProfile is SfaProfile profile &&
            profile.Sessions?.All(s => s?.RealmId is null || s.RealmId == newRealmId) == false;

        public Task<bool> CheckSessionsCancellation(string newRealmId)
        {
            return Dispatcher.UIThread.Invoke(
                () => SfaMessageBox.ShowDialog(
                    App.GetString("s_dialog_active_session_msg"),
                    App.GetString("s_dialog_active_session_title"),
                    MessageBoxButton.Yes | MessageBoxButton.Cancell)
                .ContinueWith(t => t.Result == MessageBoxButton.Yes));
        }

        public Task<bool> ProcessSessionsCancellationBeforePlay(string realmId)
        {
            if (IsSessionsCancellationRequired(realmId) == false)
                return Task.FromResult(true);

            return Task.Factory.StartNew(() =>
            {
                if (CheckSessionsCancellation(realmId).Result == false)
                    return false;

                if (Launcher.CurrentProfile is SfaProfile profile)
                {
                    var sessions = profile.Sessions?.ToList() ?? new();

                    foreach (var item in sessions)
                    {
                        if (item.RealmId != realmId)
                            profile.FinishSession(item, true);
                    }
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    UpdateRealms();
                });

                return true;
            });
        }

        public void DeleteProfile(SfaProfile profile)
        {
            if (Launcher is SfaLauncher launcher)
            {
                launcher.DeleteProfile(profile);

                if (launcher.CurrentProfile is null)
                    SelectProfile(launcher.Profiles.FirstOrDefault(p => p?.IsSupported == true));
            }
        }


        public Task<RealmInfoViewModel> AddRealm(string realmName, int? seed = default, string description = default)
        {
            if (Launcher is SfaLauncher launcher)
            {
                SFAWaitingPopup waitingPopup = Dispatcher.UIThread.Invoke(() => new SFAWaitingPopup());

                return Task<RealmInfoViewModel>.Factory.StartNew(() =>
                {
                    SfaRealmInfo realmInfo = null;

                    try
                    {
                        Dispatcher.UIThread.Invoke(waitingPopup.ShowDialog);

                        realmInfo = Launcher?.CreateNewRealm(realmName);
                        realmInfo.Realm.Description = description;

                        var realm = realmInfo.Realm;
                        var generator = new VanillaRealmGenerator(
                            realm,
                            Launcher?.Database,
                            MobsDatabase.Instance,
                            seed: seed ?? new Random128().Next());

                        generator.ProgressUpdated += (o, e) => SfaDebug.Print(e.Status, e.Task?.GetType().Name);
                        generator.Run().Wait();

                        realm.Description = description;
                        realmInfo.Save();
                    }
                    catch
                    {
                        Launcher?.DeleteRealm(realmInfo);
                        realmInfo = null;
                    }
                    finally
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            waitingPopup.Close();
                            UpdateRealms();
                        });
                    }

                    return Realms.FirstOrDefault(r => r.RealmInfo == realmInfo);
                }, TaskCreationOptions.LongRunning);
            }

            return Task.FromResult<RealmInfoViewModel>(null);
        }

        public void DeleteRealm(SfaRealmInfo realmInfo)
        {
            if (Launcher is SfaLauncher launcher)
            {
                launcher.DeleteRealm(realmInfo);
                UpdateRealms();
            }
        }

        public void UpdateRealms()
        {
            if (Launcher is SfaLauncher launcher)
            {
                var selectedLocalRealm = launcher.CurrentLocalRealm;
                var selectedServerRealm = launcher.ServerRealm;
                Realms?.Clear();

                foreach (var item in launcher.Realms ?? new())
                {
                    Realms.Add(new(this, item));
                }

                Realms.SortBy(x => x.Name);

                SelectedLocalRealm =
                    Realms.FirstOrDefault(r => r.RealmInfo == selectedLocalRealm) ??
                    Realms.FirstOrDefault();

                SelectedServerRealm =
                    Realms.FirstOrDefault(r => r.RealmInfo == selectedServerRealm) ??
                    Realms.FirstOrDefault();
            }
        }

        public void UpdateServers()
        {
            FindServerPageViewModel?.UpdateStatuses();
        }

        public SfaSession StartLocalGame()
        {
            IsGameStarted = true;

            var gameLoadingPopup = Dispatcher.UIThread.InvokeAsync(() =>
            {
                var popup = new SfaMessageBox()
                {
                    Title = App.GetString("s_dialog_game_starting_title"),
                    Text = App.GetString("s_dialog_game_starting_loading"),
                    Buttons = MessageBoxButton.Undefined,
                };

                popup.ShowDialog();
                return popup;
            }).Result;

            var progress = new Progress<string>(s =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    gameLoadingPopup.Text = App.GetString("s_dialog_game_starting_" + s);
                });
            });

            if (Launcher is SfaLauncher launcher &&
                launcher.StartLocalGame() is SfaSession session)
            {
                Sessions.Add(session);

                session.Game?.StartingTask?.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    Sessions.Remove(session);
                    IsGameStarted = Sessions.Count > 0;
                    UpdateRealms();
                    UpdateServers();
                    CheckUpdates();
                }));

                if (session.StartingTask is null)
                    gameLoadingPopup.Close();

                session.StartingTask.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    gameLoadingPopup.Close();
                    UpdateRealms();
                }));

                return session;
            }

            IsGameStarted = Sessions.Count > 0;
            return null;
        }


        public Task<SfaSession> StartGame(SfaProfile currentProfile, string address, Func<string> writePasswordDialog)
        {
            IsGameStarted = true;

            var gameLoadingPopup = Dispatcher.UIThread.InvokeAsync(() =>
            {
                var popup = new SfaMessageBox()
                {
                    Title = App.GetString("s_dialog_game_starting_title"),
                    Text = App.GetString("s_dialog_game_starting_loading"),
                    Buttons = MessageBoxButton.Undefined,
                };

                popup.ShowDialog();
                return popup;
            }).Result;

            var progress = new Progress<string>(s =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    gameLoadingPopup.Text = App.GetString("s_dialog_game_starting_" + s);
                });
            });

            if (Launcher is SfaLauncher launcher &&
                launcher.CreateGame(currentProfile, writePasswordDialog) is SfaSession session)
            {
                Sessions.Add(session);

                var task = session.StartGame(address, progress)?.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    Sessions.Remove(session);
                    IsGameStarted = Sessions.Count > 0;
                    UpdateRealms();
                    UpdateServers();
                    CheckUpdates();
                    return session;
                }));

                if (session.StartingTask is null)
                    gameLoadingPopup.Close();

                session.StartingTask.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    gameLoadingPopup.Close();
                    UpdateRealms();
                }));

                return task;
            }

            IsGameStarted = Sessions.Count > 0;
            return Task.FromResult<SfaSession>(null);
        }

        public void ShowMobsEditor()
        {
            new MobsEditorWindow().ShowDialog(App.MainWindow);
        }

        public Task<RealmInfoViewModel> ShowCreateNewRealm()
        {
            var dialog = new CreateRealmPopup()
            {
                RealmName = "NewRealm",
                RealmSeed = new Random128().Next()
            };

            return dialog.ShowDialog().ContinueWith(t =>
            {
                return Dispatcher.UIThread.Invoke(() =>
                {
                    if (dialog.IsDone == true &&
                        dialog.RealmName is not null)
                    {
                        return AddRealm(
                            dialog.RealmName,
                            dialog.RealmSeed,
                            dialog.RealmDescription);
                    }

                    return Task.FromResult<RealmInfoViewModel>(null);
                }).Result;
            });
        }


        public Task<SfaRealmInfo> ShowEditRealm(SfaRealmInfo realmInfo)
        {
            var realm = realmInfo?.Realm;

            if (realm is null)
                return Task.FromResult(realmInfo);

            var dialog = new CreateRealmPopup()
            {
                RealmName = realm.Name,
                RealmDescription = realm.Description,
                EditRealm = true,
                Title = string.Format(App.GetString("s_dialog_create_realm_edit_title") ?? string.Empty, realm.Name ?? string.Empty),
            };

            return dialog.ShowDialog().ContinueWith(t =>
            {
                return Dispatcher.UIThread.Invoke(() =>
                {
                    if (dialog.IsDone == true)
                    {
                        realm.Name = dialog.RealmName;
                        realm.Description = dialog.RealmDescription;
                    }

                    realmInfo?.SaveInfo();
                    UpdateRealms();

                    return Task.FromResult(realmInfo);
                }).Result;
            });
        }


        public Task<bool> ShowDeleteRealm(RealmInfoViewModel realm) =>
            ShowDeleteRealm(realm?.RealmInfo);

        public Task<bool> ShowDeleteRealm(SfaRealmInfo realm)
        {
            if (realm is not null)
            {
                var name = realm.Realm?.Name ?? "Realm";

                return SfaMessageBox.ShowDialog(
                    string.Format(App.GetString("s_dialog_delete_realm_msg") ?? string.Empty, name ?? string.Empty),
                    string.Format(App.GetString("s_dialog_delete_realm_title") ?? string.Empty, name ?? string.Empty),
                    MessageBoxButton.Cancell |
                    MessageBoxButton.Delete).
                    ContinueWith(t =>
                    {
                        if (t.Result != MessageBoxButton.Delete)
                            return false;

                        Dispatcher.UIThread.Invoke(() =>
                        {
                            DeleteRealm(realm);
                        });

                        return true;
                    });
            }

            return Task.FromResult(false);
        }

        public void ShowMapEditor()
        {
            new MapEditorWindow().ShowDialog(App.MainWindow);
        }

        public void ToggleUpdatePanel()
        {
            if (IsUpdatePanelVisible == false)
            {
                CheckUpdates();
            }

            IsUpdatePanelVisible = !IsUpdatePanelVisible;
        }

        public void CheckUpdates()
        {
            if (Design.IsDesignMode == true)
                return;

            Updater.GetLatestRelese().ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
            {
                LatestRelese = t.Result;

                if (LatestRelese is not null &&
                    LatestRelese.Version > SfaServer.Version &&
                    UseAutoUpdate == true &&
                    _isAutoUpdateCheckCompleted == false)
                {
#if !DEBUG
                    InstallLatestRelese();
#endif
                }

                _isAutoUpdateCheckCompleted = true;
            }));
        }

        public void InstallLatestRelese()
        {
            InstallReleasePopup.InstallRelese(LatestRelese);
        }

        public void ShowLicenses()
        {
            new LicensesWindow().Show(App.MainWindow);
        }
    }
}
