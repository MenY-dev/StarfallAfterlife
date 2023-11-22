using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public void InputFromCharacterPartyChannel(SfReader reader)
        {
            var action = (CharacterPartyAction)reader.ReadByte();

            SfaDebug.Print($"InputFromCharacterPartyChannel (Action = {action})", GetType().Name);

            switch (action)
            {
                case CharacterPartyAction.InviteToParty:
                    HandleInviteToParty(reader); break;

                case CharacterPartyAction.AcceptInvite:
                    HandleAcceptInviteToParty(reader); break;

                case CharacterPartyAction.DeclineInvite:
                    HandleDeclineInviteToParty(reader); break;

                case CharacterPartyAction.LeaveParty:
                    HandleLeaveParty(reader); break;

                case CharacterPartyAction.KickMember:
                    HandleKickPartyMember(reader); break;
            }
        }

        private void HandleInviteToParty(SfReader reader)
        {
            var userName = reader.ReadShortString(Encoding.UTF8);

            Invoke(c =>
            {
                var character = CurrentCharacter;

                if (character is null)
                    return;

                var party = CurrentCharacter?.Party; 

                if (party is null)
                {
                    character.Party = party = CharacterParty.Create(Server, character.UniqueId);
                }

                var newMember = c.Server?.GetCharacter(userName);

                if (newMember is null)
                {
                    SendPartyInviteErrorCode(userName, PartyInviteErrorCode.NoCharacter);
                    return;
                }

                if (newMember.Party is not null)
                {
                    SendPartyInviteErrorCode(userName, PartyInviteErrorCode.AllreadyInParty);
                    return;
                }

                if (party.CreateMembersSnapshot().Length > 2)
                {
                    SendPartyInviteErrorCode(userName, PartyInviteErrorCode.MemberLimit);
                    return;
                }

                SendPartyInviteErrorCode(userName, PartyInviteErrorCode.Success);
                newMember?.DiscoveryClient?.Invoke(c => c.SendInvitedToParty(character.UniqueName));
            });
        }

        private void HandleAcceptInviteToParty(SfReader reader)
        {
            var ownerName = reader.ReadShortString(Encoding.UTF8);

            Invoke(c =>
            {
                var character = CurrentCharacter;
                var ownerCharacter = Server?.GetCharacter(ownerName);
                var party = ownerCharacter?.Party;

                if (character is null || party is null)
                    return;

                if (party.CreateMembersSnapshot().Length > 2)
                    return;

                party.AddMember(character.UniqueId, PartyMemberStatus.Joined);
                ownerCharacter.DiscoveryClient?.Invoke(c => c.SendInvitedToPartyResponse(character.UniqueName, true));
            });
        }

        private void HandleDeclineInviteToParty(SfReader reader)
        {
            var ownerName = reader.ReadShortString(Encoding.UTF8);

            Invoke(c =>
            {
                var character = CurrentCharacter;
                var ownerCharacter = Server?.GetCharacter(ownerName);

                if (character is null)
                    return;

                ownerCharacter?.DiscoveryClient?.Invoke(c => c.SendInvitedToPartyResponse(character.UniqueName, false));
            });
        }

        private void HandleLeaveParty(SfReader reader)
        {
            Invoke(c =>
            {
                var character = CurrentCharacter;

                if (character is null)
                    return;

                character.Party?.RemoveMember(character.UniqueId);
            });
        }

        private void HandleKickPartyMember(SfReader reader)
        {
            var memberName = reader.ReadShortString(Encoding.UTF8);

            Invoke(c =>
            {
                var character = Server?.GetCharacter(memberName);

                if (character is null)
                    return;

                character.Party?.RemoveMember(character.UniqueId);
            });
        }

        public void SendPartyInviteErrorCode(string userName, PartyInviteErrorCode inviteResult)
        {
            SendCharacterPartyMessage(CharacterPartyServerAction.InviteError, writer =>
            {
                writer.WriteShortString(userName ?? "", -1, true, Encoding.UTF8);
                writer.WriteByte((byte)inviteResult);
            });
        }

        public void SendInvitedToParty(string ownerName)
        {
            SendCharacterPartyMessage(CharacterPartyServerAction.Invite, writer =>
            {
                writer.WriteShortString(ownerName ?? "", -1, true, Encoding.UTF8);
            });
        }

        public void SendInvitedToPartyResponse(string userName, bool accepted)
        {
            SendCharacterPartyMessage(CharacterPartyServerAction.InviteResponsed, writer =>
            {
                writer.WriteShortString(userName ?? "", -1, true, Encoding.UTF8);
                writer.WriteBoolean(accepted);
            });
        }

        public void SendPartyMembers()
        {
            var members = CurrentCharacter?.Party?.CreateMembersSnapshot();

            SendCharacterPartyMessage(CharacterPartyServerAction.MemberList, writer =>
            {
                writer.WriteUInt16((ushort)members.Length);

                foreach (var member in members)
                {
                    writer.WriteInt32(member.Id);
                    writer.WriteShortString(member.Name ?? "", -1, true, Encoding.UTF8);
                    writer.WriteByte((byte)member.Status);
                    writer.WriteInt32(member.CurrentStarSystem);
                }
            });
        }

        public virtual void SendCharacterPartyMessage(CharacterPartyServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToCharacterPartyChannel(writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public void SendToCharacterPartyChannel(Action<SfWriter> writeAction)
        {
            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Client?.Send(writer.ToArray(), SfaServerAction.CharacterPartyChannel);
        }
    }
}
