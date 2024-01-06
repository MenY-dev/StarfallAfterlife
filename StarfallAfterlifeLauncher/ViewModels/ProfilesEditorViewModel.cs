using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class ProfilesEditorViewModel : ViewModelBase
    {
        public SfaLauncher Launcher => MainWindow?.Launcher;

        public AppViewModel MainWindow { get; }

        public ObservableCollection<ProfileInfoViewModel> Profiles { get; } = new();

        public ProfileInfoViewModel SelectedProfile
        {
            get => selectedProfile;
            set
            {
                SetAndRaise(ref selectedProfile, value);
                OnSelectionChanged();
            }
        }

        public bool ShowEditor
        {
            get => showEditor;
            set
            {
                var oldValue = showEditor;
                SetAndRaise(ref showEditor, value);

                if (value == true && oldValue != value)
                {
                    UpdateProfiles();
                    SelectCurrentProfile();
                }
            }
        }

        private bool showEditor = false;
        private ProfileInfoViewModel selectedProfile;

        public ProfilesEditorViewModel()
        {
            if (Design.IsDesignMode)
            {
                ShowEditor = false;
                MainWindow = new AppViewModel();
            }

            UpdateProfiles();
        }

        public ProfilesEditorViewModel(AppViewModel mainWindowViewModel)
        {
            MainWindow = mainWindowViewModel;
            UpdateProfiles();
        }

        public void UpdateProfiles()
        {
            var selectedProfile = SelectedProfile?.Profile;
            Profiles?.Clear();

            foreach (var item in Launcher?.Profiles ?? new())
                Profiles.Add(new(item));

            SelectedProfile = Profiles.FirstOrDefault(p => p.Profile == selectedProfile);
        }

        public void OnSelectionChanged()
        {
            
        }

        public void SelectProfile()
        {
            ShowEditor = false;
            MainWindow.SelectProfile(SelectedProfile?.Profile);
        }

        public void CancellSelection()
        {
            ShowEditor = false;
        }

        public void CreateNewProfile()
        {
            MainWindow.CreateNewProfile().ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
            {
                UpdateProfiles();
                SelectCurrentProfile();
            }));
        }

        public void DeleteProfile()
        {
            var profile = SelectedProfile?.Profile;

            if (profile is not null)
            {
                var name = profile.GameProfile?.Nickname ?? "Profile";

                SfaMessageBox.ShowDialog(
                    string.Format(App.GetString("s_dialog_delete_profile_label") ?? string.Empty, name),
                    App.GetString("s_dialog_delete_profile_title"),
                    MessageBoxButton.Cancell |
                    MessageBoxButton.Delete).
                    ContinueWith(t =>
                    {
                        if (t.Result == MessageBoxButton.Delete)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                MainWindow?.DeleteProfile(profile);
                                UpdateProfiles();
                                SelectCurrentProfile();
                            });
                        }
                    });
            }
        }

        public void SelectCurrentProfile()
        {
            SelectedProfile = Profiles.FirstOrDefault(p => p.Profile == Launcher?.CurrentProfile);
        }
    }
}
