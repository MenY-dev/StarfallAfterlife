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

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer : MessagingServer<SfaServerClient>, ISfaObject
    {
        //public string RealmId { get; set; } = "3af8cc9c641f452fbf69b9c99d6f2569";

        public SfaRealm Realm { get; set; }

        public DiscoveryGalaxy Galaxy { get; set; }

        public SfaMatchmaker Matchmaker { get; protected set; }

        public List<SfaServerClient> Clients { get; } = new();

        public SortedList<int, ServerCharacter> Characters { get; } = new();

        public Stack<int> FreePlayersIds { get; } = new();

        public Uri InstanceManagerAddress { get; set; }

        public bool UseExternalInstanceManager { get; set; }

        public InstanceManager InstanceManager { get; set; }

        public Task Task => CompletionSource?.Task ?? Task.CompletedTask;

        protected TaskCompletionSource CompletionSource { get; set; }

        protected ActionBuffer ActionBuffer { get; } = new();

        protected object ClientsLockher { get; } = new();

        public SfaServer()
        {

        }

        public SfaServer(SfaRealm realm)
        {
            Realm = realm;
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

        protected override void HandleClientDisconnect(SfaServerClient client)
        {
            base.HandleClientDisconnect(client);

            UseClients(clients =>
            {
                var clientCharacters = client.DiscoveryClient?.Characters;

                if (clientCharacters is not null)
                    foreach (var item in clientCharacters)
                        if (item is ServerCharacter character)
                            Characters.Remove(character.UniqueId);

                clients.Remove(client);
                client.Dispose();
            });
        }

        public void UseClients(Action<List<SfaServerClient>> handler)
        {
            lock (ClientsLockher)
                handler?.Invoke(Clients);
        }

        public virtual void RegisterCharacter(ServerCharacter character)
        {
            lock (ClientsLockher)
            {
                if (character is null || character.UniqueId > -1 ||
                Characters.IndexOfKey(character.UniqueId) > 0)
                    return;

                if (FreePlayersIds.Count > 0)
                {
                    character.UniqueId = FreePlayersIds.Pop();
                }
                else
                {
                    if (Characters.Count > 0)
                        character.UniqueId = Characters.GetKeyAtIndex(Characters.Count - 1) + 1;
                    else
                        character.UniqueId = 1;
                }

                character.UniqueName = CreateUnicueCharacterName(character.Name);
                Characters.Add(character.UniqueId, character);
            }
        }

        public string CreateUnicueCharacterName(string baseName)
        {
            string name = baseName ?? "RenamedCharacter";
            int index = 0;

            while (GetCharacter(name) != null)
            {
                index++;
                name = $"{baseName}({index})";
            }

            return name;
        }

        public ServerCharacter GetCharacter(string name)
        {
            lock (ClientsLockher)
                return Characters.Values.FirstOrDefault(c => c.Name == name);
        }

        public ServerCharacter GetCharacter(int id)
        {
            lock (ClientsLockher)
                return Characters.Values.FirstOrDefault(c => c.UniqueId == id);
        }

        public ServerCharacter GetCharacter(DiscoveryFleet fleet)
        {
            if (fleet is null)
                return null;

            lock (ClientsLockher)
                return Characters.Values.FirstOrDefault(c => c.Fleet == fleet);
        }


        public List<SfaServerClient> GetClientsInDiscovery()
        {
            lock (ClientsLockher)
            {
                return Clients
                    .Where(c => c.State == SfaClientState.InDiscoveryMod)
                    .ToList();
            }
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
            }
            catch
            {
                CompletionSource = new TaskCompletionSource();
                CompletionSource.TrySetResult();
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

        public void Save(string directory)
        {
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            string configPath = Path.Combine(directory, "Config.json");
            File.WriteAllText(configPath, ToJson()?.ToJsonString() ?? "", Encoding.UTF8);

            string realmDirectory = Path.Combine(directory, "Realm");
            Realm?.Save(realmDirectory);
        }

        public void Load(string directory)
        {
            string configPath = Path.Combine(directory, "Config.json");
            LoadFromJson(JsonNode.Parse(File.ReadAllText(configPath)));
            (Realm ??= new()).Load(Path.Combine(directory, "Realm"));
        }
    }
}
