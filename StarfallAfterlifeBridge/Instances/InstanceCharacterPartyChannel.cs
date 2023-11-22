using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceCharacterPartyChannel : InstanceChannel
    {
        public InstanceCharacterPartyChannel(string name, int id, InstanceManagerServerClient owner, SfaInstance instance, ChannelClient client) :
            base(name, id, owner, instance, client)
        {
        }

        public void SendPartyMembers(int partyId, List<CharacterPartyMember> members)
        {
            SendCharacterPartyMessage(CharacterPartyServerAction.MemberList, writer =>
            {
                writer.WriteInt32(partyId);
                writer.WriteUInt16((ushort)members.Count);

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
            using var writer = new SfWriter();
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);
            Send(Client, writer.ToArray());
        }
    }
}
