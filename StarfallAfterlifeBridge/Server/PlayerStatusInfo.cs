using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public struct PlayerStatusInfo
    {
        public string Auth;
        public string Name;
        public int CharacterId;
        public string CharacterName;
        public Faction CharacterFaction;
        public UserInGameStatus Status;
        public int CurrentSystemId;
        public string CurrentSystemName;
    }
}
