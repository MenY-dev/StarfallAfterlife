using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Launcher.Controls;
using StarfallAfterlife.Launcher.MapEditor;
using StarfallAfterlife.Launcher.MobsEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
                Launcher.CurrentServerRealm = value?.RealmInfo;
            }
        }

        private RealmInfoViewModel _selectedLocalRealm;
        private RealmInfoViewModel _selectedServerRealm;
        private bool _isGameStarted;


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
                    RaisePropertyChanged(oldValue, value);
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
                    GameDirectory = gameDir;
                    Launcher?.SaveSettings();
                }
            });
        }

        public Task CreateNewProfile()
        {
            return Dispatcher.UIThread.Invoke(() => new CreateProfilePopup().ShowDialog().ContinueWith(t =>
            {

                if (t.Result is CreateProfilePopup result &&
                    result.IsDone == true &&
                    result.Text is not null)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (Launcher is SfaLauncher launcher)
                        {
                            var profile = Launcher?.CreateNewProfile(result.Text);

                            if (launcher.CurrentProfile is null && profile is not null)
                                SelectProfile(profile);
                        }
                    });
                }
            }));
        }

        public bool IsSessionsCancellationRequired(string newRealmId) =>
            Launcher.CurrentProfile is SfaProfile profile &&
            profile.Sessions?.All(s => s?.RealmId is null || s.RealmId == newRealmId) == false;

        public Task<bool> CheckSessionsCancellation(string newRealmId)
        {
            return Dispatcher.UIThread.Invoke(
                () => SfaMessageBox.ShowDialog(
                    "Active sessions have been found in other realms. " +
                    "These sessions must be dropped to continue. " +
                    "The ships will be sent for repairs, and the contents of their holds will be lost. " +
                    "\r\n\r\nDrop sessions?",
                    "Drop active sessions?",
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


        public Task<RealmInfoViewModel> AddRealm(string realmName)
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
                        var generator = new VanillaRealmGenerator(realmInfo.Realm, Launcher?.Database);
                        generator.ProgressUpdated += (o, e) => SfaDebug.Print(e.Status, e.Task?.GetType().Name);
                        generator.Run().Wait();
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
                var selectedServerRealm = launcher.CurrentServerRealm;
                Realms?.Clear();

                foreach (var item in launcher.Realms ?? new())
                {
                    Realms.Add(new(this, item));
                }

                SelectedLocalRealm =
                    Realms.FirstOrDefault(r => r.RealmInfo == selectedLocalRealm) ??
                    Realms.FirstOrDefault();

                SelectedServerRealm =
                    Realms.FirstOrDefault(r => r.RealmInfo == selectedServerRealm) ??
                    Realms.FirstOrDefault();
            }
        }

        public SfaSession StartLocalGame()
        {
            IsGameStarted = true;

            if (Launcher is SfaLauncher launcher &&
                launcher.StartLocalGame() is SfaSession session)
            {
                Sessions.Add(session);

                session.Game?.Task?.ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                {
                    Sessions.Remove(session);
                    IsGameStarted = Sessions.Count > 0;
                    UpdateRealms();
                }));

                return session;
            }

            IsGameStarted = Sessions.Count > 0;
            return null;
        }

        public void ShowMobsEditor()
        {
            new MobsEditorWindow().ShowDialog(App.MainWindow);
        }

        public Task<RealmInfoViewModel> ShowCreateNewRealm()
        {
            return Dispatcher.UIThread.InvokeAsync(() => new CreateRealmPopup()
                .ShowDialog("NewRealm")
                .ContinueWith(t =>
                {
                    if (t.Result is CreateRealmPopup result &&
                        result.IsDone == true &&
                        result.Text is not null)
                    {
                        return AddRealm(result.Text).Result;
                    }

                    return null;
                }));
        }

        public Task<bool> ShowDeleteRealm(RealmInfoViewModel realm) =>
            ShowDeleteRealm(realm?.RealmInfo);

        public Task<bool> ShowDeleteRealm(SfaRealmInfo realm)
        {
            if (realm is not null)
            {
                var name = realm.Realm?.Name ?? "Realm";

                return SfaMessageBox.ShowDialog(
                    name + " will be removed. This action cannot be undone!",
                    "DELETE REALM",
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
    }
}
