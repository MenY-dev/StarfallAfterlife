using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum MatchmakerChannelAction : byte
    {
        ReadyToPlay = 0,
        Cancel = 1,
        InviteToParty = 2,
        AcceptInvite = 3,
        DeclineInvite = 4,
        LeaveParty = 5,
        ReadyToChallenge = 6,
        CreateLobby = 7,
    }
}
