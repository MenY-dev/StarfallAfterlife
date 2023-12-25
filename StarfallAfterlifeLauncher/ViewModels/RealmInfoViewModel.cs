using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class RealmInfoViewModel : ViewModelBase
    {
        public string Name => RealmInfo?.Realm?.Name;

        public bool IsActiveSession { get; set; }

        public SfaRealmInfo RealmInfo
        {
            get => _realmInfo;
            set => SetAndRaise(ref _realmInfo, value);
        }

        public AppViewModel AppVM { get; set; }

        private SfaRealmInfo _realmInfo;

        public RealmInfoViewModel(SfaRealmInfo realm)
        {
            RealmInfo = realm;
        }

        public RealmInfoViewModel(AppViewModel appVM, SfaRealmInfo realm) : this(realm)
        {
            AppVM = appVM;
            RealmInfo = realm;
            IsActiveSession = appVM.Launcher.CurrentProfile?.GetSession(realm?.Realm?.Id) is not null;
        }

        public void Update()
        {
            RealmInfo = RealmInfo;
        }
    }
}
