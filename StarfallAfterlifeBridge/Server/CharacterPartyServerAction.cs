using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum CharacterPartyServerAction
    {
        Invite = 0,
        InviteResponsed = 1,
        MemberList = 2,
        InviteError = 3,
        LeaveMember = 4,
        MemberListForServer = 5,
    }
}
