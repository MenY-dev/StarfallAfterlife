using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class GalacticChannel : GameChannel
    {
        public GalacticChannel(string name, int id, SfaGame game) : base(name, id, game)
        {

        }

        public override void Input(ChannelClient client, byte[] data)
        {
            base.Input(client, data);
            Game?.SfaClient.Send(data, SfaServerAction.GalacticChannel);
        }

        public virtual void EnterToGalaxy(int systemId, float x = 0, float y = 0)
        {
            byte[] data = new byte[13];

            using (SfWriter writer = new(data))
            {
                writer.WriteByte(2);     // Cmd
                writer.WriteInt32(systemId);    // SystemId
                writer.WriteSingle(x);   // PosX
                writer.WriteSingle(y);   // PosY
            }

            Send(data);
            SfaDebug.Print($"EnterToGalaxy", Name);
        }

        public void SendCharacterCurrencyUpdate()
        {
            Game?.Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character)
                {
                    SendGalaxyMessage(
                        DiscoveryServerGalaxyAction.CharacterCurrencyUpdate,
                        writer =>
                        {
                            writer.WriteInt32(character.IGC);
                        });
                }
            });
        }

        public void SendCharacterXpUpdate()
        {
            Game?.Profile?.Use(p =>
            {
                if (p.GameProfile?.CurrentCharacter is Character character)
                {
                    SendGalaxyMessage(
                        DiscoveryServerGalaxyAction.CharacterXpUpdate,
                        writer =>
                        {
                            writer.WriteInt32(character.Xp);
                            writer.WriteInt32(character.AccessLevel);
                        });
                }
            });
        }

        public void SendGalaxyMessage(DiscoveryServerGalaxyAction action, Action<SfWriter> writeAction = null)
        {
            using var writer = new SfWriter();
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);
            Send(writer.ToArray());
        }
    }
}
