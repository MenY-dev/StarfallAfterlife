using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class SinglePlayerModePageViewModel : ViewModelBase
    {
        public SfaLauncher Launcher => AppVM?.Launcher;

        public AppViewModel AppVM { get; }

        public SinglePlayerModePageViewModel()
        {
            if (Design.IsDesignMode)
                AppVM = new AppViewModel();
        }

        public SinglePlayerModePageViewModel(AppViewModel mainWindowViewModel)
        {
            AppVM = mainWindowViewModel;
        }

        public Task CreateNewRealm()
        {
            return AppVM.ShowCreateNewRealm().ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
            {
                if (AppVM is AppViewModel app &&
                    t.Result is RealmInfoViewModel realm)
                    app.SelectedLocalRealm = realm;
            }));
        }

        public Task<bool> DeleteSelectedRealm() =>
            AppVM?.ShowDeleteRealm(AppVM?.SelectedLocalRealm) ?? Task.FromResult(false);

        public void StartGame()
        {
            var appVM = AppVM;
            var launcher = Launcher;

            if (appVM is null || launcher is null)
                return;

            var realmInfo = launcher.CurrentLocalRealm;

            if (realmInfo is null)
                return;

            Task.Run(() =>
            {
                if (appVM.MakeBaseTests(true, true).Result == false)
                    return false;

                if (launcher.Realms.Count < 1)
                    CreateNewRealm().Wait();

                if (launcher.Realms.Count < 1)
                    return false;

                if (appVM.ProcessSessionsCancellationBeforePlay(realmInfo.Realm?.Id).Result == false)
                    return false;

                return true;
            }).ContinueWith(t =>
            {
                if (t.Result != true)
                    return;

                appVM.StartLocalGame();
            });
        }
    }
}
