using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum MatchmakerChannelLobbyAction : byte
    {
        CreateLobby = 0,
        SetMap = 1,
        SetTeam = 2,
        SetFleet = 3,
        InviteToLobby = 4,
        AcceptInvite = 5,
        DeclineInvite = 6,
        ReadyToGame = 7,
        StartMatch = 8,
        LeaveLobby = 9,
    }
}
