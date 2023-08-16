using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceAuthReadyEventArgs : EventArgs
    {
        public InstanceInfo Instance { get; }
        public int CharacterId { get; }
        public string Auth { get; }

        public InstanceAuthReadyEventArgs(InstanceInfo instance, int characterId, string auth)
        {
            Instance = instance;
            CharacterId = characterId;
            Auth = auth;
        }
    }
}
