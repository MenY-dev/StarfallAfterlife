using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Database;
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
                Launcher.Profiles?.Contains(profile) == true)
            {
                Launcher.CurrentProfile = profile;
                CurrentProfileName = Launcher?.CurrentProfile?.GameProfile?.Nickname;
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

        public void DeleteProfile(SfaProfile profile)
        {
            if (Launcher is SfaLauncher launcher)
            {
                launcher.DeleteProfile(profile);

                if (launcher.CurrentProfile is null)
                    SelectProfile(launcher.Profiles.FirstOrDefault());
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
                        realmInfo.Realm.Database = Launcher?.Database;
                        realmInfo.Realm.MobsDatabase = MobsDatabase.Instance;
                        realmInfo.Realm.GalaxyMap = new TestGalaxyMapBuilder().Create();
                        realmInfo.Realm.GalaxyMapHash = realmInfo.Realm.GalaxyMap.Hash;
                        realmInfo.Realm.MobsMap = new MobsMapGenerator(realmInfo.Realm).Build();
                        realmInfo.Realm.ShopsMap = new ShopsGenerator(realmInfo.Realm).Build();
                        realmInfo.Realm.QuestsDatabase = new QuestsGenerator(realmInfo.Realm).Build();
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
                    Realms.Add(new(item));

                SelectedLocalRealm =
                    Realms.FirstOrDefault(r => r.RealmInfo == selectedLocalRealm) ??
                    Realms.FirstOrDefault();

                SelectedServerRealm =
                    Realms.FirstOrDefault(r => r.RealmInfo == selectedServerRealm) ??
                    Realms.FirstOrDefault();
            }
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
