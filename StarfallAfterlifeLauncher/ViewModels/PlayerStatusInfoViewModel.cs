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
                RaisePropertyChanged(Auth, nameof(Auth));
                RaisePropertyChanged(Name, nameof(Name));
                RaisePropertyChanged(CharacterId, nameof(CharacterId));
                RaisePropertyChanged(CharacterName, nameof(CharacterName));
                RaisePropertyChanged(CharacterFaction, nameof(CharacterFaction));
                RaisePropertyChanged(CurrentSystemId, nameof(CurrentSystemId));
                RaisePropertyChanged(CurrentSystemName, nameof(CurrentSystemName));
                RaisePropertyChanged(Status, nameof(Status));
                RaisePropertyChanged(IsOnline, nameof(IsOnline));
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
