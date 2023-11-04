using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public void InputFromQuickMatchChannel(SfReader reader)
        {
            var action = (QuickMatchAction)reader.ReadByte();

            SfaDebug.Print($"InputFromQuickMatchChannel (Action = {action})", GetType().Name);

            switch (action)
            {
                case QuickMatchAction.ReadyToPlay:
                    HandleQuickMatchReadyToPlay(reader);
                    break;

                case QuickMatchAction.Cancel:
                    HandleQuickMatchCancel(reader);
                    break;
            }
        }

        public void HandleQuickMatchReadyToPlay(SfReader reader)
        {
            string gameMode = reader.ReadShortString(Encoding.UTF8);
            byte difficulty = reader.ReadByte();

            Invoke(() =>
            {
                var battle = Server.Matchmaker.QuickMatchGameMode.CreateStationAttackMatch(difficulty, CurrentCharacter);
                battle.Start();
            });

            SfaDebug.Print($"QuickMatchReadyToPlay (GameMode = {gameMode}, Difficulty = {difficulty})", GetType().Name);
        }

        public void HandleQuickMatchCancel(SfReader reader)
        {
            SfaDebug.Print($"QuickMatchCancel", GetType().Name);
        }

        public void SendQuickMatchState(
            MatchMakingStage stage = MatchMakingStage.Nothing,
            MatchMakingResetReason resetReason = MatchMakingResetReason.Unknown)
        {
            SendQuickMatchMessage(
                QuickMatchServerAction.StageUpdated,
                writer =>
                {
                    writer.WriteByte((byte)stage);
                    writer.WriteByte((byte)resetReason);
                });
        }

        public virtual void SendQuickMatchMessage(QuickMatchServerAction action, Action<SfWriter> writeAction = null)
        {
            SendToQuickMatchChannel(writer =>
            {
                writer.WriteByte((byte)action);
                writeAction?.Invoke(writer);
            });
        }

        public void SendToQuickMatchChannel(Action<SfWriter> writeAction)
        {
            using SfWriter writer = new();
            writeAction?.Invoke(writer);
            Client?.Send(writer.ToArray(), SfaServerAction.QuickMatchChannel);
        }
    }
}
