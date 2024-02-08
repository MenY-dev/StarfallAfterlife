using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum MatchmakerChannelLobbyServerAction : byte
    {
        LobbyUpdated = 0,
        InviteToLobby = 1,
        InviteResponse = 2,
        MatchReady = 3,
    }
}
