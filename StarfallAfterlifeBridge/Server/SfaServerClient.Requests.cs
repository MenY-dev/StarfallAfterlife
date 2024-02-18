using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Server.Characters;
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
                    HandleAddToFriends(isCharChannel, reader);
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

        private void HandleAddToFriends(bool isCharChannel, SfReader reader)
        {
            var name = reader.ReadShortString(Encoding.UTF8);

            Invoke(c => SendFriendChannelMessage(isCharChannel, FriendChannelServerAction.ReciveFriendRequestResult, writer =>
            {
                writer.WriteByte(1);
                writer.WriteShortString(name, -1, true, Encoding.UTF8);
            }));
        }

        private void HandleUserStatus(bool isCharChannel, SfReader reader)
        {
            var status = (UserInGameStatus)reader.ReadByte();

            Invoke(c =>
            {
                Server?.ProcessNewUserStatus(this, status);
                Server?.Matchmaker?.OnUserStatusChanged(this, status);
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
                        var status = item.IsConnected == true ? item.UserStatus : UserInGameStatus.None;

                        SendAcceptNewFriend($"@{item.UniqueName}", status);
                        SendUserStatus($"@{item.UniqueName}", status);

                        if (item.CurrentCharacter is ServerCharacter character)
                        {
                            SendAcceptNewFriend(character.UniqueName, status);
                            SendUserStatus(character.UniqueName, status);
                        }
                    }
                }
            });
        }

        public void SendUserStatus(string friend, UserInGameStatus status)
        {
            SendUserStatus(friend, status, false);
            SendUserStatus(friend, status, true);
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

        public void SendAcceptNewFriend(string friend, UserInGameStatus status)
        {
            SendAcceptNewFriend(friend, status, false);
            SendAcceptNewFriend(friend, status, true);
        }

        public void SendAcceptNewFriend(string friend, UserInGameStatus status, bool isCharChannel = false)
        {
            if (friend is null)
                return;

            Invoke(c => SendFriendChannelMessage(isCharChannel, FriendChannelServerAction.AcceptNewFriend, writer =>
            {
                writer.WriteShortString(friend, -1, true, Encoding.UTF8);
                writer.WriteByte((byte)(status != UserInGameStatus.None ? 1 : 0));
                writer.WriteByte((byte)status);
            }));
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
