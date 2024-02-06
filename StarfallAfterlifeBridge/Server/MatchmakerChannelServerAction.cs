using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum MatchmakerChannelServerAction : byte
    {
        InstanceReady = 0,
        StageUpdated = 4,
        InviteToParty = 5,
        ResponseToInvite = 6,
        PartyMembersListChanged = 7,
        PartyFleetSelection = 8,
        LobbyCmd = 9,
        ShopUpdate = 10,
        ClientState = 11,
        RequestDataUpdate = 12,
    }
}
