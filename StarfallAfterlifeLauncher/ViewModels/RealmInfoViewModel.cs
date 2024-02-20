using StarfallAfterlife.Bridge.Launcher;
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

        public string Description => RealmInfo?.Realm?.Description;

        public int Seed => RealmInfo?.Realm?.Seed ?? -1;

        public DiscoverySession[] Sessions { get; protected set; }

        public bool IsActiveSession => Sessions?.Any(
            s => s?.RealmId is not null && s.RealmId == RealmInfo?.Realm?.Id) ?? false;

        public string[] ActiveSessionChars { get; protected set; }

        public bool LockedByServer { get; protected set; }

        public SfaRealmInfo RealmInfo
        {
            get => _realmInfo;
            set
            {
                SetAndRaise(ref _realmInfo, value);
                Update();
            }
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
        }

        public void Update()
        {
            RaisePropertyUpdate(Name, nameof(Name));
            RaisePropertyUpdate(Description, nameof(Description));
            RaisePropertyUpdate(Seed, nameof(Seed));

            if (AppVM is AppViewModel app &&
                app.Launcher is SfaLauncher launcher)
            {
                Sessions = launcher.CurrentProfile?.GetSessions(RealmInfo?.Realm?.Id);

                ActiveSessionChars = Sessions?
                    .Where(s => s is not null)
                    .Select(s => launcher.CurrentProfile?.GetCharacter(s?.CharacterId ?? -1)?.Name)
                    .Where(n => n is not null)
                    .ToArray();

                LockedByServer =
                    RealmInfo is not null &&
                    (app.CreateServerPageViewModel?.ServerStarted == true &&
                    app.SelectedServerRealm?.RealmInfo == RealmInfo);
            }
            else
            {
                Sessions = null;
                ActiveSessionChars = null;
                LockedByServer = false;
            }

            RaisePropertyUpdate(Sessions, nameof(Sessions));
            RaisePropertyUpdate(IsActiveSession, nameof(IsActiveSession));
            RaisePropertyUpdate(ActiveSessionChars, nameof(ActiveSessionChars));
            RaisePropertyUpdate(LockedByServer, nameof(LockedByServer));
        }
    }
}
