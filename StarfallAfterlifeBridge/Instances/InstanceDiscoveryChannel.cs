using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceDiscoveryChannel : InstanceChannel
    {
        public InstanceDiscoveryChannel(string name, int id, InstanceManagerServerClient owner, SfaInstance instance, ChannelClient client) :
            base(name, id, owner, instance, client)
        {
        }

        public override void Input(ChannelClient client, byte[] data)
        {
            base.Input(client, data);

            var reader = new SfReader(data);
            var systemId = reader.ReadInt32();
            var objectType = (DiscoveryObjectType)reader.ReadByte();
            int objectId = reader.ReadInt32();
            var action = (DiscoveryClientAction)reader.ReadInt32();

            SfaDebug.Print($"Input (SystemId = {systemId}, ObjectType = {objectType}, ObjectId = {objectId}, Action = {action})", "InstanceDiscoveryChannel");

            switch (action)
            {
                case DiscoveryClientAction.SignalForInstanceBattle:
                    HandleInstanceBattleSignal(reader); break;

                case DiscoveryClientAction.FleetLeavesInstance:
                    HandleFleetLeavesInstance(reader, objectType, objectId); break;

                case DiscoveryClientAction.PlayerLeaveBattle:
                    HandlePlayerLeaveBattle(reader, objectType, objectId); break;

                case DiscoveryClientAction.CharacterStatsNotification: break;

                case DiscoveryClientAction.AddCharacterShipsXp:
                    HandleAddCharacterShipsXp(reader, objectType, objectId); break;
                    
                case DiscoveryClientAction.AuthForInstance:
                    HandleAuthForInstance(reader); break;

                case DiscoveryClientAction.EnemyShipDestroyedNotification:
                    HandleEnemyShipDestroyedNotification(reader, objectType, objectId); break;

                case DiscoveryClientAction.UpdatePirateOutpostAssaultStatus:
                case DiscoveryClientAction.UpdatePirateStationAssaultStatus:
                    HandleUpdatePirateAssaultStatus(reader, objectType, objectId); break;

                case DiscoveryClientAction.InstanceObjectInteractEvent:
                    HandleInstanceObjectInteractEvent(reader, objectType, objectId); break;

                case DiscoveryClientAction.SecretObjectLooted:
                    HandleSecretObjectLooted(reader, objectType, objectId); break;
            }
        }

        public virtual void HandleFleetLeavesInstance(SfReader reader, DiscoveryObjectType fleetType, int fleetId)
        {
            var hex = reader.ReadHex();
            SfaDebug.Print($"FleetLeaves (Instance = {Instance?.InstanceId} FleetType = {fleetType}, FleetId = {fleetId}, Hex = {hex})", "InstanceDiscoveryChannel");
            (Instance as DiscoveryBattleInstance)?.OnFleetLeaves(fleetType, fleetId, hex);
        }

        private void HandlePlayerLeaveBattle(SfReader reader, DiscoveryObjectType objectType, int objectId)
        {
            SfaDebug.Print($"PlayerLeaves (Id = {objectId}");
        }

        public virtual void HandleInstanceBattleSignal(SfReader reader)
        {
            var instanceId = reader.ReadInt32();
            var cmd = (DiscoveryInstanceBattleCmd)reader.ReadByte();

            if (cmd == DiscoveryInstanceBattleCmd.NoFleets)
            {
                Instance?.Stop();
            }

            SfaDebug.Print($"New Signal (Instance = {instanceId} Cmd = {cmd})", "InstanceDiscoveryChannel");
        }

        private void HandleAuthForInstance(SfReader reader)
        {
            int instanceId = reader.ReadInt32();
            int charId = reader.ReadInt32();
            string auth = reader.ReadShortString(Encoding.ASCII);

            Instance?.OnAuthForInstanceReady(charId, auth);

            SfaDebug.Print($"Auth For Instance (Instance = {instanceId}, Char = {charId}, Auth = {auth})", "InstanceDiscoveryChannel");
        }

        private void HandleEnemyShipDestroyedNotification(SfReader reader, DiscoveryObjectType objectType, int objectId)
        {
            var shipMobId = reader.ReadInt32();
            var shipObjectId = reader.ReadInt32();
            byte type = reader.ReadByte();
            byte shipFaction = reader.ReadByte();
            var shipFactionGroup = reader.ReadInt32();
            byte shipClass = reader.ReadByte();
            var shipLevel = reader.ReadInt32();
            var repEarned = reader.ReadInt32();
            var isInAttackEvent = reader.ReadInt32();
            var tagsCount = reader.ReadUInt16();
            var tags = new List<string>();

            for (int i = 0; i < tagsCount; i++)
                tags.Add(reader.ReadShortString(Encoding.UTF8));

            string data = new JsonObject
            {
                ["killer_id"] = objectId,
                ["killer_type"] = (byte)objectType,
                ["mob_id"] = shipMobId,
                ["obj_id"] = shipObjectId,
                ["type"] = type,
                ["faction"] = shipFaction,
                ["faction_group"] = shipFactionGroup,
                ["ship_class"] = shipClass,
                ["level"] = shipLevel,
                ["rep_earned"] = repEarned,
                ["is_in_attack_event"] = isInAttackEvent,
                ["tags"] = JsonHelpers.ParseNodeUnbuffered(tags),
            }.ToJsonStringUnbuffered(false);

            Owner.SendInstanceAction(Instance?.Auth, "enemy_ship_destroyed", data);

            SfaDebug.Print($"EnemyShipDestroyed (KillerId = {objectId}, KillerType = {objectType}, " +
                $"MobId = {shipMobId}, MobFaction = {shipFaction}, MobFactionGroup = {shipFactionGroup})", GetType().Name);

            SfaDebug.Print($"EnemyShipDestroyed ({data})", GetType().Name);
        }

        private void HandleUpdatePirateAssaultStatus(SfReader reader, DiscoveryObjectType objectType, int objectId)
        {
            var playersInBattle = reader.ReadInt32();
            var isDestroyed = reader.ReadInt32();
            var isFinished = reader.ReadInt32();

            Owner.SendInstanceAction(Instance?.Auth, "pirates_assault_status", new JsonObject
            {
                ["obj_type"] = (int)objectType,
                ["obj_id"] = objectId,
                ["players_in_battle"] = playersInBattle,
                ["destroyed"] = isDestroyed,
                ["finished"] = isFinished,
            }.ToJsonStringUnbuffered(false));

            SfaDebug.Print($"PirateAssaultStatus (Id = {objectId}, Players = {playersInBattle}, " +
                $"Destroyed = {isDestroyed}, Finished = {isFinished})", GetType().Name);


        }

        public void SendNewPlayerJoiningToInstance(InstanceCharacter character)
        {
            string text = JsonHelpers.SerializeUnbuffered(character);

            if (text is null)
                return;

            SendMessage(
                DiscoveryServerAction.NewPlayerJoiningToInstance,
                writer => writer.WriteShortString(text, -1, true, Encoding.UTF8));
        }


        private void HandleAddCharacterShipsXp(SfReader reader, DiscoveryObjectType objectType, int objectId)
        {
            if (objectType != DiscoveryObjectType.UserFleet ||
                objectId < 0)
                return;

            var xpSourceLevel = reader.ReadInt32();
            var shipsCount = reader.ReadUInt16();
            var ships = new List<int>();

            for (int i = 0; i < shipsCount; i++)
                ships.Add(reader.ReadInt32());

            var xpsCount = reader.ReadUInt16();
            var xps = new List<int>();

            for (int i = 0; i < xpsCount; i++)
                xps.Add(reader.ReadInt32());

            var spentBonusXp = reader.ReadInt32();
            var shipsXpsCount = Math.Min(shipsCount, xpsCount);
            var shipsXps = new Dictionary<int, int>();

            for (int i = 0; i < shipsXpsCount; i++)
                shipsXps[ships[i]] = xps[i];

            Instance?.Context?.SendAddCharacterShipsXp(objectId, shipsXps);

            SfaDebug.Print($"AddCharacterShipsXp (ObjectId = {objectId}, " +
                $"XpSourceLevel = {xpSourceLevel}, SpentBonusXp = {spentBonusXp})", GetType().Name);

            foreach (var item in shipsXps)
                SfaDebug.Print($"AddCharacterShipsXp (Ship = {item.Key}, Xp = {item.Value})", GetType().Name);
        }

        private void HandleInstanceObjectInteractEvent(SfReader reader, DiscoveryObjectType objectType, int objectId)
        {
            var eventType = reader.ReadShortString(Encoding.UTF8);
            var eventData = reader.ReadShortString(Encoding.UTF8);

            Owner.SendInstanceAction(Instance?.Auth, "object_interact_event", new JsonObject
            {
                ["obj_type"] = (int)objectType,
                ["obj_id"] = objectId,
                ["event_type"] = eventType,
                ["event_data"] = eventData,
            }.ToJsonStringUnbuffered(false));

            SfaDebug.Print($"InstanceObjectInteractEvent " +
                $"(ObjectType = {objectType}, ObjectId = {objectId}, Event = {eventType})", GetType().Name);
        }

        private void HandleSecretObjectLooted(SfReader reader, DiscoveryObjectType objectType, int objectId)
        {
            Owner.SendInstanceAction(Instance?.Auth, "secret_object_looted", new JsonObject
            {
                ["obj_type"] = (int)objectType,
                ["obj_id"] = objectId,
            }.ToJsonStringUnbuffered(false));

            SfaDebug.Print($"SecretObjectLooted " +
                $"(ObjectType = {objectType}, ObjectId = {objectId})");
        }

        public void SendMessage(DiscoveryServerAction action, Action<SfWriter> writeAction = null)
        {
            using var writer = new SfWriter();
            writer.WriteByte((byte)action);
            writeAction?.Invoke(writer);
            Client?.Send(Id, writer.ToArray());
        }
    }
}
