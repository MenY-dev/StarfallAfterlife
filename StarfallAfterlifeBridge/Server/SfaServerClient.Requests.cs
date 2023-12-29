using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient
    {
        public void InputFromFriendChannel(SfReader reader, bool isCharChannel)
        {
            var action = (FriendChannelAction)reader.ReadByte();

            switch (action)
            {
                case FriendChannelAction.AddToFriends:
                    break;
                case FriendChannelAction.AcceptNewFriend:
                    break;
                case FriendChannelAction.DeclineNewFriend:
                    break;
                case FriendChannelAction.RemoveFromFriends:
                    break;
                case FriendChannelAction.Status:
                    if (isCharChannel == false)
                        HandleUserStatus(isCharChannel, reader);
                    break;
                default:
                    break;
            }
        }

        private void HandleUserStatus(bool isCharChannel, SfReader reader)
        {
            var status = (UserInGameStatus)reader.ReadByte();
            Server?.OnUserStatusChanged(this, status);
        }
    }
}
