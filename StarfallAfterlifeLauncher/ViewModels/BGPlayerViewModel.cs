using StarfallAfterlife.Bridge.Server.Matchmakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class BGPlayerViewModel : ViewModelBase
    {
        public BGRoomTeam Team
        {
            get => _team;
            set
            {
                SetAndRaise(ref _team, value);
                Room?.Data?.SetCharacterTeam(Info?.CharacterId ?? -1, value);
            }
        }

        public PlayerStatusInfoViewModel Info
        {
            get => _info;
            set
            {
                SetAndRaise(ref _info, value);
            }
        }


        public BGRoomViewModel Room
        {
            get => _room;
            set
            {
                SetAndRaise(ref _room, value);
            }
        }

        public BGRoomTeam[] TeamVariants => Enum.GetValues<BGRoomTeam>();

        private BGRoomTeam _team;
        private PlayerStatusInfoViewModel _info;
        private BGRoomViewModel _room;

        public BGPlayerViewModel() { }

        public BGPlayerViewModel(BGRoomViewModel room, PlayerStatusInfoViewModel info, BGRoomTeam team)
        {
            Room = room;
            Info = info;
            Team = team;
        }

        public void TeamSelected(object obj)
        {
            if (obj is BGRoomTeam team)
                Team = team;
        }
    }
}
