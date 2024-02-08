using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Server.Matchmakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient
    {
        public RankedGameMode RankedGameMode => Server?.Matchmaker?.RankedGameMode;

        public void InputFromMatchmakerChannel(SfReader reader)
        {
            var action = (MatchmakerChannelAction)reader.ReadByte();

            SfaDebug.Print($"InputFromMatchmakerChannel (Action = {action})", GetType().Name);

            switch (action)
            {
                case MatchmakerChannelAction.ReadyToPlay:
                    HandleRankedReadyToPlay(reader);
                    break;
                case MatchmakerChannelAction.Cancel:
                    HandleRankedCancel(reader);
                    break;
                case MatchmakerChannelAction.InviteToParty:
                    break;
                case MatchmakerChannelAction.AcceptInvite:
                    break;
                case MatchmakerChannelAction.DeclineInvite:
                    break;
                case MatchmakerChannelAction.LeaveParty:
                    break;
                case MatchmakerChannelAction.ReadyToChallenge:
                    HandleRankedReadyToChallenge(reader);
                    break;
                case MatchmakerChannelAction.CreateLobby:
                    InputFromMatchmakerLobbyChannel(reader);
                    break;
            }
        }

        public void InputFromMatchmakerLobbyChannel(SfReader reader)
        {
            var action = (MatchmakerChannelLobbyAction)reader.ReadByte();

            SfaDebug.Print($"InputFromLobbyChannel (Action = {action})", GetType().Name);

            switch (action)
            {
                case MatchmakerChannelLobbyAction.CreateLobby:
                    HandleRankedLobbyCreate(reader);
                    break;
                case MatchmakerChannelLobbyAction.SetMap:
                    HandleRankedLobbySetMap(reader);
                    break;
                case MatchmakerChannelLobbyAction.SetTeam:
                    HandleRankedLobbySetTeam(reader);
                    break;
                case MatchmakerChannelLobbyAction.SetFleet:
                    HandleRankedLobbySetFleet(reader);
                    break;
                case MatchmakerChannelLobbyAction.InviteToLobby:
                    HandleRankedLobbyInvite(reader);
                    break;
                case MatchmakerChannelLobbyAction.AcceptInvite:
                    HandleRankedLobbyInviteAccept(reader);
                    break;
                case MatchmakerChannelLobbyAction.DeclineInvite:
                    HandleRankedLobbyInviteDecline(reader);
                    break;
                case MatchmakerChannelLobbyAction.ReadyToGame:
                    HandleRankedLobbyReadyToGame(reader);
                    break;
                case MatchmakerChannelLobbyAction.StartMatch:
                    HandleRankedLobbyStartMatch(reader);
                    break;
                case MatchmakerChannelLobbyAction.LeaveLobby:
                    HandleRankedLobbyLeave(reader);
                    break;
            }
        }

        private void HandleRankedReadyToPlay(SfReader reader)
        {
            SelectedRankedFleet = reader.ReadInt32();
            SendRankedMatchMakingStage(MatchMakingStage.SearchingOpponents);
            RankedGameMode?.AddToQueue(this);
        }

        private void HandleRankedCancel(SfReader reader)
        {
            RankedGameMode?.RemoveFromQueue(this);
        }

        private void HandleRankedReadyToChallenge(SfReader reader)
        {

        }

        private void HandleRankedLobbyCreate(SfReader reader)
        {
            RankedGameMode?.CreateLobby(this);
        }

        private void HandleRankedLobbyInvite(SfReader reader)
        {
            var request = reader.ReadShortString(Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(request))
                return;

            var player = Server?
                .GetClientsInRankedMode()
                .Where(c => c?.UniqueName?.Contains(request) == true)
                .ToList()
                .OrderBy(c => c.UniqueName)
                .FirstOrDefault(p => 
                    p is not null &&
                    p.UserStatus is (UserInGameStatus.RankedMainMenu or UserInGameStatus.CharMainMenu) &&
                    p.IsPlayer == true &&
                    p.IsConnected == true &&
                    p.State is (SfaClientState.InRankedMode or SfaClientState.InDiscoveryMod));

            if (player is not null &&
                RankedGameMode?.GetLobbyByOwner(this) is RankedLobby lobby)
            {
                if (RankedGameMode?.GetLobby(player.PlayerId) is not null)
                {
                    SendRankedLobbyInviteResponse(player.UniqueName, RankedLobbyInviteResponse.AllreadyInLobby);
                    return;
                }

                var team = lobby.GetTeam(0).Length < 1 ? 0 :
                           lobby.GetTeam(1).Length < 1 ? 1 :
                           -1;

                lobby.AddPlayer(player, RankedLobbyUserStatus.Invited, team, false);
            }
        }

        private void HandleRankedLobbyInviteAccept(SfReader reader)
        {
            var ownerName = reader.ReadShortString(Encoding.UTF8)?.TrimStart('@');
            var owner = Server?.GetPlayer(ownerName);

            if (owner is not null)
                RankedGameMode.GetLobbyByOwner(owner)?.AcceptInvite(PlayerId);
        }

        private void HandleRankedLobbyInviteDecline(SfReader reader)
        {
            var ownerName = reader.ReadShortString(Encoding.UTF8)?.TrimStart('@');
            var owner = Server?.GetPlayer(ownerName);

            if (owner is not null)
                RankedGameMode.GetLobbyByOwner(owner)?.DeclineInvite(PlayerId);
        }

        private void HandleRankedLobbyLeave(SfReader reader)
        {
            var ownerId = reader.ReadInt32();

            if (RankedGameMode.GetLobby(PlayerId) is null)
                SendRankedLobbyUpdated(new());

            RankedGameMode?.LeaveTheLobby(ownerId, PlayerId);
        }

        private void HandleRankedLobbySetFleet(SfReader reader)
        {
            var fleetId = reader.ReadInt32();
            RankedGameMode?.GetLobby(PlayerId)?.SetPlayerFleet(PlayerId, fleetId);
        }

        private void HandleRankedLobbySetTeam(SfReader reader)
        {
            var targetName = reader.ReadShortString(Encoding.UTF8);
            var targetTeam = reader.ReadInt32();
            var targetPlayer = Server.GetPlayer(targetName);

            if (targetPlayer is null)
                return;

            RankedGameMode?.GetLobbyByOwner(PlayerId)?.SetPlayerTeam(targetPlayer.PlayerId, targetTeam);
        }

        private void HandleRankedLobbySetMap(SfReader reader)
        {
            var map = reader.ReadShortString(Encoding.UTF8);
            RankedGameMode?.GetLobbyByOwner(PlayerId)?.SetMap(map);
        }

        private void HandleRankedLobbyReadyToGame(SfReader reader)
        {
            bool isReady = reader.ReadByte() != 0;
            RankedGameMode?.GetLobby(PlayerId)?.SetReady(PlayerId, isReady);
        }

        private void HandleRankedLobbyStartMatch(SfReader reader)
        {
            RankedGameMode?.GetLobbyByOwner(PlayerId)?.StartMatch();
        }

        public void SendRankedMatchMakingStage(
            MatchMakingStage stage = MatchMakingStage.Nothing,
            MatchMakingResetReason resetReason = MatchMakingResetReason.Unknown)
        {
            SendMatchmakerMessage(
                MatchmakerChannelServerAction.StageUpdated,
                writer =>
                {
                    writer.WriteByte((byte)stage);
                    writer.WriteByte((byte)resetReason);
                });
        }

        public void SendRankedClientState(MatchMakingStage stage = MatchMakingStage.Nothing, int queuePos = 0)
        {
            SendMatchmakerMessage(
                MatchmakerChannelServerAction.ClientState,
                writer =>
                {
                    writer.WriteByte((byte)stage);
                    writer.WriteUInt32((uint)queuePos);
                });
        }

        public void SendRankedLobbyUpdated(RankedLobby lobby)
        {
            if (lobby is null)
                return;

            var lobbyPlayers = lobby.Players.ToArray();

            SendMatchmakerLobbyMessage(
                MatchmakerChannelLobbyServerAction.LobbyUpdated,
                writer =>
                {
                    writer.WriteUInt16((ushort)lobbyPlayers.Length);

                    foreach (var player in lobbyPlayers)
                    {
                        writer.WriteShortString(player.Player?.UniqueName ?? "", -1, true, Encoding.UTF8);
                        writer.WriteByte((byte)player.Status);
                        writer.WriteInt32(player.Team);
                        writer.WriteInt32(player.FleetId);
                    }

                    writer.WriteShortString(lobby.Map ?? "", -1, true, Encoding.UTF8);
                    writer.WriteShortString(lobby.OwnerName ?? "", -1, true, Encoding.UTF8);
                    writer.WriteInt32(lobby.OwnerId);
                });
        }

        public void SendRankedLobbyInvite(RankedLobby lobby)
        {
            if (lobby is null)
                return;

            SendMatchmakerLobbyMessage(
                MatchmakerChannelLobbyServerAction.InviteToLobby,
                writer =>
                {
                    writer.WriteShortString(lobby.OwnerName ?? "", -1, true, Encoding.UTF8);
                });
        }

        public void SendRankedLobbyInviteResponse(string playerName, RankedLobbyInviteResponse result)
        {
            if (playerName is null)
                return;

            SendMatchmakerLobbyMessage(
                MatchmakerChannelLobbyServerAction.InviteResponse,
                writer =>
                {
                    writer.WriteShortString(playerName ?? "", -1, true, Encoding.UTF8);
                    writer.WriteByte((byte)result);
                });
        }

        public void SendRankedLobbyMatchReady(bool isReady)
        {
            SendMatchmakerLobbyMessage(
                MatchmakerChannelLobbyServerAction.MatchReady,
                writer =>
                {
                    writer.WriteBoolean(isReady);
                });
        }

        public void SendMatchmakerLobbyMessage(MatchmakerChannelLobbyServerAction action, Action<SfWriter> writeAction = null)
        {
            SendMatchmakerMessage(
                MatchmakerChannelServerAction.LobbyCmd,
                writer =>
                {
                    writer.WriteByte((byte)action);
                    writeAction?.Invoke(writer);
                });
        }

        public void SendMatchmakerMessage(MatchmakerChannelServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToMatchmakerChannel(writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public void SendToMatchmakerChannel(Action<SfWriter> writeAction = null)
        {
            if (writeAction is null)
                return;

            using SfWriter writer = new();
            writeAction?.Invoke(writer);

            Send(writer.ToArray(), SfaServerAction.MatchmakerChannel);
        }
    }
}
