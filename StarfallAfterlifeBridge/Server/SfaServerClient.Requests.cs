using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
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
            var status = UserStatus = (UserInGameStatus)reader.ReadByte();
            Server?.OnUserStatusChanged(this, status);

            Server?.UseClients(_ =>
            {
                foreach (var item in Server.Players)
                {
                    if (item != this &&
                        item.IsConnected == true)
                    {
                        item.SendAcceptNewFriend(UniqueName, status);
                        item.SendUserStatus(UniqueName, status);
                    }
                }
            });
        }


        public void SendServerPlayerStatuses()
        {
            Server?.UseClients(_ =>
            {
                foreach (var item in Server.Players)
                {
                    if (item != this)
                    {
                        SendAcceptNewFriend(item.UniqueName, item.UserStatus);
                        SendUserStatus(item.UniqueName, item.UserStatus);
                    }
                }
            });
        }

        public void SendUserStatus(string friend, UserInGameStatus status, bool isCharChannel = false)
        {
            if (friend is null)
                return;

            SendFriendChannelMessage(isCharChannel, FriendChannelServerAction.FriendStatus, writer =>
            {
                writer.WriteShortString(friend, -1, true, Encoding.UTF8);
                writer.WriteByte((byte)(status != UserInGameStatus.None ? 1 : 0));
                writer.WriteByte((byte)status);
            });
        }

        public void SendAcceptNewFriend(string friend, UserInGameStatus status, bool isCharChannel = false)
        {
            if (friend is null)
                return;

            SendFriendChannelMessage(isCharChannel, FriendChannelServerAction.AcceptNewFriend, writer =>
            {
                writer.WriteShortString(friend, -1, true, Encoding.UTF8);
                writer.WriteByte((byte)(status != UserInGameStatus.None ? 1 : 0));
                writer.WriteByte((byte)status);
            });
        }

        public void SendFriendChannelMessage(bool isCharChannel, FriendChannelServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToFriendChannel(isCharChannel, writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public void SendToFriendChannel(bool isCharChannel, Action<SfWriter> writeAction = null)
        {
            if (writeAction is null)
                return;

            using SfWriter writer = new();
            writeAction?.Invoke(writer);

            Send(writer.ToArray(), isCharChannel ?
                SfaServerAction.CharacterFriendChannel :
                SfaServerAction.UserFriendChannel);
        }
    }
}
