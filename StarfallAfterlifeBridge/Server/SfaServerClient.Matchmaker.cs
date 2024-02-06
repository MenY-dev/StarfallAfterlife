using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
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

        }

        private void HandleRankedReadyToPlay(SfReader reader)
        {
            SelectedRankedFleet = reader.ReadInt32();
            SendRankedMatchMakingStage(MatchMakingStage.SearchingOpponents);
            Server?.Matchmaker?.RankedGameMode?.AddToQueue(this);
        }

        private void HandleRankedCancel(SfReader reader)
        {
            Server?.Matchmaker?.RankedGameMode?.RemoveFromQueue(this);
        }

        private void HandleRankedReadyToChallenge(SfReader reader)
        {

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
