using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class PlayerStatusInfoViewModel : ViewModelBase
    {
        public string Auth => Info.Auth;

        public string Name => Info.Name;

        public int CharacterId => Info.CharacterId;

        public string CharacterName => Info.CharacterName;

        public Faction CharacterFaction => Info.CharacterFaction;

        public int CurrentSystemId => Info.CurrentSystemId;

        public string CurrentSystemName => Info.CurrentSystemName;

        public UserInGameStatus Status => Info.Status;

        public bool IsOnline => Info.Status != UserInGameStatus.None;

        public PlayerStatusInfo Info
        {
            get => _info;
            set
            {
                SetAndRaise(ref _info, value);
                RaisePropertyUpdate(Auth, nameof(Auth));
                RaisePropertyUpdate(Name, nameof(Name));
                RaisePropertyUpdate(CharacterId, nameof(CharacterId));
                RaisePropertyUpdate(CharacterName, nameof(CharacterName));
                RaisePropertyUpdate(CharacterFaction, nameof(CharacterFaction));
                RaisePropertyUpdate(CurrentSystemId, nameof(CurrentSystemId));
                RaisePropertyUpdate(CurrentSystemName, nameof(CurrentSystemName));
                RaisePropertyUpdate(Status, nameof(Status));
                RaisePropertyUpdate(IsOnline, nameof(IsOnline));
            }
        }

        private PlayerStatusInfo _info;

        public PlayerStatusInfoViewModel() { }

        public PlayerStatusInfoViewModel(PlayerStatusInfo info)
        {
            Info = info;
        }
    }
}
