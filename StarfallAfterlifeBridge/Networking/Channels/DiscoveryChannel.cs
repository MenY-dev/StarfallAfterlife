using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Game;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class DiscoveryChannel : GameChannel
    {
        public int CurrentSystem { get; protected set; } = int.MaxValue;

        public DiscoveryChannel(string name, int id, SfaGame game) : base(name, id, game)
        {

        }

        public override void Input(ChannelClient client, byte[] data)
        {
            base.Input(client, data);

            using var reader = new SfReader(data);
            var systemId = reader.ReadInt32();
            var objectType = (DiscoveryObjectType)reader.ReadByte();
            var objectId = reader.ReadInt32();
            var action = (DiscoveryClientAction)reader.ReadInt32();

            if (systemId > -1 &&
                objectType == DiscoveryObjectType.UserFleet &&
                Game?.GameProfile?.CurrentCharacter?.UniqueId == objectId)
                CurrentSystem = systemId;

            if (action == DiscoveryClientAction.RequestStockContent &&
                reader.ReadShortString(Encoding.UTF8) == "inventory")
            {
                SendInventory(systemId, objectType, objectId);
            }
            else
            {
                Game?.SfaClient?.Send(data, SfaServerAction.DiscoveryChannel);
            }
        }

        public void SendInventory()
        {
            if (Game?.GameProfile?.CurrentCharacter is Character character)
                SendInventory(CurrentSystem, DiscoveryObjectType.UserFleet, character.UniqueId);
        }

        public void SendInventory(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Game?.Profile?.Use(p =>
            {
                if (p?.GameProfile?.CurrentCharacter?.Inventory is InventoryStorage inventory)
                {
                    SendDiscoveryMessage(
                        systemId, objectType, objectId,
                        DiscoveryServerAction.ObjectStockUpdated,
                        writer =>
                        {
                            writer.WriteUInt16((ushort)inventory.Count); // Count

                            foreach (var item in inventory)
                            {
                                writer.WriteByte((byte)item.Type); // ItemType
                                writer.WriteInt32(item.Id); // Id
                                writer.WriteInt32(item.Count); // Count
                                writer.WriteInt32(item.IGCPrice); // IGCPrice
                                writer.WriteInt32(item.BGCPrice); // BGCPrice
                                writer.WriteShortString(item.UniqueData ?? "", -1, true, Encoding.UTF8); // UniqueData
                            }

                            writer.WriteShortString("inventory", -1, true, Encoding.UTF8); // Stock
                        });
                }
            });
        }

        public void SendConnectToInstance(int systemId, int fleetId, string address, int port, string auth)
        {
            if (address is null || auth is null || port < 0 || port > ushort.MaxValue)
                return;

            SendDiscoveryMessage(
                systemId,
                DiscoveryObjectType.UserFleet,
                fleetId,
                DiscoveryServerAction.Instance,
                writer =>
                {
                    writer.WriteShortString(address, -1, true, Encoding.ASCII);
                    writer.WriteUInt16((ushort)port);
                    writer.WriteShortString(auth, -1, true, Encoding.ASCII);
                });
        }

        public void SendDiscoveryMessage(int systemId, DiscoveryObjectType objectType, int objectId, DiscoveryServerAction action, Action<SfWriter> writeAction = null)
        {
            using var writer = new SfWriter();

            writer.WriteInt32(systemId);
            writer.WriteByte((byte)objectType);
            writer.WriteInt32(objectId);
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);

            Send(writer.ToArray());
        }
    }
}
