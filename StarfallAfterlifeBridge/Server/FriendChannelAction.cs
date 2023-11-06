using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum FriendChannelAction : byte
    {
        AddToFriends = 0,
        AcceptNewFriend = 1,
        DeclineNewFriend = 2,
        RemoveFromFriends = 3,
        Status = 4,
    }
}
