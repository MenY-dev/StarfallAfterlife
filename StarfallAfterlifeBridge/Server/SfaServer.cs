using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Tasks;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Matchmakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using StarfallAfterlife.Bridge.Instances;
using System.IO;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Characters;
using System.Text.Json.Nodes;
using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Profiles;
using System.Reflection;
using System.Security.Cryptography;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Houses;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer : MessagingServer<SfaServerClient>, ISfaObject
    {
        //public string RealmId { get; set; } = "3af8cc9c641f452fbf69b9c99d6f2569";

        public SfaRealm Realm => RealmInfo?.Realm;

        public SfaRealmVariable Variable => Realm?.Variable;

        public SfaRealmInfo RealmInfo { get; set; }

        public string Password { get; set; }

        public DiscoveryGalaxy Galaxy { get; set; }

        public SfaMatchmaker Matchmaker { get; protected set; }

        public List<SfaServerClient> Clients { get; } = new();

        public IdCollection<SfaServerClient> Players { get; } = new() { StartId = 1 };

        public IdCollection<ServerCharacter> Characters { get; } = new() { StartId = 1 };

        public IdCollection<CharacterParty> Parties { get; } = new() { StartId = 1 };

        public Uri InstanceManagerAddress { get; set; }

        public bool UsePortForwarding { get; set; } = false;

        public Task Task => CompletionSource?.Task ?? Task.CompletedTask;

        public event EventHandler<PlayerStatusInfoEventArgs> PlayerStatusUpdated;

        public ShopsMap ShopsMap { get; protected set; }

        public ShopsGenerator ShopsGenerator{ get; protected set; }

        protected TaskCompletionSource CompletionSource { get; set; }

        protected ActionBuffer ActionBuffer { get; } = new();

        protected object ClientsLockher { get; } = new();

        public static Version Version => AssemblyVersion.Value;

        private static Lazy<Version> AssemblyVersion { get; } = new(
            () => Assembly.GetAssembly(typeof(SfaServer)).GetName().Version);

        public SfaServer()
        {

        }

        public SfaServer(SfaRealmInfo realm)
        {
            RealmInfo = realm;
        }

        protected override SfaServerClient CreateNewClient()
        {
            var client = base.CreateNewClient();
            client.Server = this;
            client.Galaxy = Galaxy;
            client.State = SfaClientState.PendingLogin;
            UseClients(clients => clients.Add(client));
            return client;
        }

        public bool CheckPassword(string inputPassword)
        {
            var serverPassword = Password;

            if (string.IsNullOrWhiteSpace(serverPassword) == true)
                return true;

            if (inputPassword is not null &&
                serverPassword == inputPassword)
                return true;

            return false;
        }

        protected override void HandleClientDisconnect(SfaServerClient client)
        {
            base.HandleClientDisconnect(client);

            UseClients(clients =>
            {
                if (client.IsPlayer == false)
                {
                    RemoveClient(client);
                }
                else
                {
                    Matchmaker?.RankedGameMode?.GetLobby(client.PlayerId)?.RemovePlayer(client.PlayerId);

                    if (client?.DiscoveryClient?.CurrentCharacter is ServerCharacter character)
                    {
                        character.Party?.RemoveMember(character.UniqueId);
                        character.SetOnlineStatus(false);
                    }
                }

                var time = DateTime.Now;
                var toRemove = clients.Where(c => (time - c.LastInput).TotalHours > 4).ToList();

                foreach (var item in toRemove)
                {
                    RemoveClient(item);
                }

                ProcessNewUserStatus(client, UserInGameStatus.None);
            });
        }

        public SfaServerClient GetClient(string auth)
        {
            lock (ClientsLockher)
                return Clients.FirstOrDefault(c => c.Auth == auth);
        }

        public SfaServerClient GetClient(Guid profileId)
        {
            if (profileId == Guid.Empty)
                return null;

            lock (ClientsLockher)
                return Clients.FirstOrDefault(c => c.ProfileId == profileId);
        }

        public void RemoveClient(SfaServerClient client)
        {
            if (client is null)
                return;

            try
            {
                UseClients(clients =>
                {
                    clients.Remove(client);
                    Players.Remove(client);

                    var clientCharacters = client.DiscoveryClient?.Characters;

                    if (clientCharacters is not null)
                    {
                        foreach (var item in clientCharacters)
                        {
                            if (item is ServerCharacter character)
                            {
                                character.Party?.RemoveMember(character.UniqueId);
                                Characters.RemoveId(character.UniqueId);
                            }
                        }
                    }

                    client.Close();
                    client.Dispose();
                });
            }
            catch { }
        }

        public void UseClients(Action<List<SfaServerClient>> handler)
        {
            lock (ClientsLockher)
                handler?.Invoke(Clients);
        }

        public virtual int RegisterPlayer(SfaServerClient player)
        {
            lock (ClientsLockher)
            {
                if (player is null || player.PlayerId > -1 ||
                    Players.ContainsId(player.PlayerId) == true)
                    return -1;

                player.UniqueName = CreateUnicuePlayerName(player.Name);
                return player.PlayerId = Players.Add(player);
            }
        }

        public virtual bool TravelPlayer(SfaServerClient from, SfaServerClient to)
        {
            if (from is null || to is null)
                return false;

            lock (ClientsLockher)
            {
                var id = Players.IdOf(from);

                if (id < 0)
                    return false;

                Players[id] = to;
                to.PlayerId = id;
                to.IsPlayer = from.IsPlayer;
                from.TravelToClient(to);
                RemoveClient(from);
                return true;
            }
        }

        public SfaServerClient GetPlayer(int id)
        {
            SfaServerClient player = null;
            UseClients(c => player = Players.FirstOrDefault(p => p?.IsPlayer == true && p.PlayerId == id));
            return player;
        }

        public SfaServerClient GetPlayer(Guid id)
        {
            SfaServerClient player = null;
            UseClients(c => player = Players.FirstOrDefault(p => p?.IsPlayer == true && p.ProfileId == id));
            return player;
        }

        public SfaServerClient GetPlayer(string name)
        {
            SfaServerClient player = null;
            UseClients(c => player = Players.FirstOrDefault(p => p?.IsPlayer == true && p.UniqueName == name));
            return player;
        }

        public string CreateUnicuePlayerName(string baseName)
        {
            string name = baseName ?? "RenamedPlayer";
            int index = 0;

            while (GetPlayer(name) != null)
            {
                index++;
                name = $"{baseName}_{index}";
            }

            return name;
        }

        public virtual int RegisterCharacter(ServerCharacter character)
        {
            lock (ClientsLockher)
            {
                if (character is null || character.UniqueId > -1 ||
                    Characters.ContainsId(character.UniqueId) == true)
                    return -1;

                character.UniqueName = CreateUnicueCharacterName(character.Name);
                return character.UniqueId = Characters.Add(character);
            }
        }

        public string CreateUnicueCharacterName(string baseName)
        {
            string name = baseName ?? "RenamedCharacter";
            int index = 0;

            while (GetCharacter(name) != null)
            {
                index++;
                name = $"{baseName}_{index}";
            }

            return name;
        }

        public ServerCharacter GetCharacter(string name)
        {
            lock (ClientsLockher)
                return Characters.FirstOrDefault(c => c.UniqueName == name);
        }

        public ServerCharacter GetCharacter(int id)
        {
            lock (ClientsLockher)
                return Characters[id];
        }

        public ServerCharacter GetCharacter(Guid profileId, Guid charId)
        {
            lock (ClientsLockher)
                return Characters.FirstOrDefault(
                    c => c?.Guid == charId && c.ProfileId == profileId);
        }

        public ServerCharacter GetCharacter(HouseMember member)
        {
            if (member is null)
                return null;

            lock (ClientsLockher)
                return Characters.FirstOrDefault(
                    c => c?.Guid == member.CharacterId && c.ProfileId == member.PlayerId);
        }

        public ServerCharacter GetCharacter(DiscoveryFleet fleet)
        {
            if (fleet is null)
                return null;

            lock (ClientsLockher)
                return Characters.FirstOrDefault(c => c.Fleet == fleet);
        }


        public List<SfaServerClient> GetClientsInDiscovery()
        {
            lock (ClientsLockher)
            {
                return Players
                    .Where(c => c.State == SfaClientState.InDiscoveryMod)
                    .ToList();
            }
        }

        public List<SfaServerClient> GetClientsInRankedMode()
        {
            lock (ClientsLockher)
            {
                return Players
                    .Where(c => c.State == SfaClientState.InRankedMode)
                    .ToList();
            }
        }

        public void ProcessNewUserStatus(SfaServerClient client, UserInGameStatus status)
        {
            if (client is null ||
                client.IsPlayer == false)
                return;

            client.UserStatus = status;

            if (client.State != SfaClientState.InRankedMode &&
                status is UserInGameStatus.RankedMainMenu or
                          UserInGameStatus.RankedSearchingForGame or
                          UserInGameStatus.RankedInBattle)
            {
                client.State = SfaClientState.InRankedMode;
                client.CurrentSystemName = null;
                client.CurrentSystemId = -1;
                client.CurrentCharacter?.SetOnlineStatus(false);
            }

            OnUserStatusChanged(client, status);

            UseClients(_ =>
            {
                foreach (var item in Players)
                {
                    if (item != client &&
                        item.IsConnected == true)
                    {
                        item.Invoke(c =>
                        {
                            c.SendAcceptNewFriend($"@{client.UniqueName}", status);
                            c.SendUserStatus($"@{client.UniqueName}", status);
                        });
                    }
                }

                if (client.CurrentCharacter is ServerCharacter character)
                {
                    foreach (var item in Players)
                    {
                        if (item != client &&
                            item.IsConnected == true)
                        {
                            item.Invoke(c =>
                            {
                                c.SendAcceptNewFriend(character.UniqueName, status);
                                c.SendUserStatus(character.UniqueName, status);
                            });
                        }
                    }
                }
            });
        }

        public void OnUserStatusChanged(SfaServerClient client, UserInGameStatus status)
        {
            if (client is null)
                return;

            var character = client.CurrentCharacter;

            PlayerStatusUpdated?.Invoke(this, new(new()
            {
                Auth = client.Auth,
                Name = client.UniqueName,
                ProfileId = client.ProfileId,
                CharacterId = character?.UniqueId ?? -1,
                CharacterName = character?.UniqueName,
                CharacterFaction = character?.Faction ?? Faction.None,
                CurrentSystemId = client.CurrentSystemId,
                CurrentSystemName = client.CurrentSystemName,
                Status = status,
            }));
        }

        public ObjectShops GetObjectShops(int objectId, GalaxyMapObjectType objectType)
        {
            var generator = ShopsGenerator ??= new(Realm);
            var map = ShopsMap ??= new();
            var shops = map.GetObjectShops(objectId, objectType);

            if (shops is null)
            {
                var system = Realm?.GalaxyMap?.GetSystem(objectType, objectId);
                shops = generator.GenerateObjectShops(system, system?.GetObject(objectType, objectId), Realm?.Seed ?? 0);

                if (shops is not null)
                    map.SetObjectShops(shops);
            }

            return shops;
        }

        public virtual void Invoke(Action action) => ActionBuffer?.Invoke(action);

        public override void Start()
        {
            Stop();

            try
            {
                Galaxy = new DiscoveryGalaxy(Realm);
                Galaxy.Listeners.Add(this);
                Galaxy.Start();
                Matchmaker = new();
                Matchmaker.Server = this;
                Matchmaker.InstanceManagerAddress = InstanceManagerAddress;
                Matchmaker.Start();
                base.Start();
                CompletionSource = new TaskCompletionSource();

                if (UsePortForwarding == true &&
                    Address?.Port is int port &&
                    port > 0 && port <= ushort.MaxValue)
                {
                    NatPuncher.Create(System.Net.Sockets.ProtocolType.Tcp, port, port, 0);
                }
            }
            catch (Exception e)
            {
                CompletionSource = new TaskCompletionSource();
                CompletionSource.TrySetResult();
                SfaDebug.Print(e, GetType().Name);
            }
        }

        public override void Stop()
        {
            Galaxy?.Stop();
            Galaxy?.Listeners.Remove(this);
            Matchmaker?.Stop();
            Clients?.Clear();
            base.Stop();

            CompletionSource?.TrySetResult();
        }

        void ISfaObject.Init() { }

        public virtual void LoadFromJson(JsonNode doc)
        {
            if (doc is null)
                return;
        }

        public virtual JsonNode ToJson()
        {
            return new JsonObject
            {

            };
        }

        public static bool IsVersionCompatible(Version target)
        {
            if (target is null ||
                target.Major != Version.Major ||
                target.Minor != Version.Minor)
                return false;

            return true;
        }

        public static string CreatePasswordHash(string password)
        {
            try
            {
                var bytes = Encoding.Unicode.GetBytes(password);
                var salt = SHA384.HashData(bytes);
                var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 12735, HashAlgorithmName.SHA256, 25);
                return Convert.ToHexString(hash);
            }
            catch { }

            return null;
        }
    }
}
