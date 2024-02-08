using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public enum RankedLobbyUserStatus : byte
    {
        None = 0,
        Invited = 1,
        Joined = 2,
        Ready = 3,
    }
}
