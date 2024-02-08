using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum RankedLobbyInviteResponse : byte
    {
        Accepted = 0,
        Declined = 1,
        AllreadyInLobby = 2,
    }
}
