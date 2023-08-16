using StarfallAfterlife.Bridge.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MatchmakerGameMode
    {
        public SfaMatchmaker Matchmaker { get; protected set; }

        public SfaServer Server => Matchmaker?.Server;

        public InstanceManagerClient InstanceManager => Matchmaker?.InstanceManager;

        public virtual void Init(SfaMatchmaker matchmaker)
        {
            Matchmaker = matchmaker;
        }
    }
}
