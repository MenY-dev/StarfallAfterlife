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

        public SfaRealmInfo RealmInfo
        {
            get => _realmInfo;
            set => SetAndRaise(ref _realmInfo, value);
        }

        private SfaRealmInfo _realmInfo;

        public RealmInfoViewModel(SfaRealmInfo realm)
        {
            RealmInfo = realm;
        }

        public void Update()
        {
            RealmInfo = RealmInfo;
        }
    }
}
