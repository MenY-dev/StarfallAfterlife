using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum FriendChannelServerAction : byte
    {
        AcceptNewFriend = 0,
        AddToFriendsRequest = 1,
        RemoveFromFriends = 2,
        FriendStatus = 3,
        ReciveFriendRequestResult = 4
    }
}
