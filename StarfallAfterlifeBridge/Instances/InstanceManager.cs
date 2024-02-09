using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Networking.Messaging;

namespace StarfallAfterlife.Bridge.Instances
{
    public partial class InstanceManager : MessagingServer<InstanceManagerServerClient>
    {
        public string GameDirectory { get; set; }

        public string GameExeLocation => Path.Combine(GameDirectory, "Msk", "starfall_game", "Starfall", "Binaries", "Win64", "Starfall.exe");

        public string WorkingDirectory { get; set; }

        public string InstancesDirectory { get; protected set; }

        protected Dictionary<string, SfaInstance> Instances { get; } = new();

        protected object InstancesLockher { get; } = new object();

        public override void Start()
        {
            Init();
            base.Start();
        }

        public virtual SfaInstance CreateNewInstance(InstanceManagerServerClient owner, InstanceInfo info)
        {
            var auth = CreateInstanceAuth();

            var instance = SfaInstance.Create(info);
            instance.Auth = auth;
            instance.Directory = CreateInstanceDirectory(auth);
            Instances.Add(auth, instance);
            instance.Init(owner);

            return instance;
        }

        public virtual SfaInstance StartInstance(string id)
        {
            if (Instances.TryGetValue(id, out var instance) == true)
            {
                if (instance.Start() == true)
                    return instance;
            }

            return null;
        }

        public SfaInstance GetInstance(string auth)
        {
            lock (InstancesLockher)
            {
                if (Instances.TryGetValue(auth, out SfaInstance instance))
                    return instance;

                return null;
            }
        }

        protected override void HandleClientDisconnect(InstanceManagerServerClient client)
        {
            lock (InstancesLockher)
            {
                try
                {
                    foreach (var instance in client.GetInstancesSnapshot())
                        instance.Stop();
                }
                catch { }
            }
            
            base.HandleClientDisconnect(client);
        }

        protected virtual void Init()
        {
            InstancesDirectory = Path.Combine(WorkingDirectory, "Instances");
        }

        protected string CreateInstanceDirectory(string auth)
        {
            return Path.Combine(InstancesDirectory, auth);
        }

        protected string CreateInstanceAuth()
        {
            return Guid.NewGuid().ToString("N");
        }

    }
}
