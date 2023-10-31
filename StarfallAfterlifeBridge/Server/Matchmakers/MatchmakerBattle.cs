using StarfallAfterlife.Bridge.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MatchmakerBattle
    {
        public MatchmakerGameMode GameMode { get; set; }

        public SfaMatchmaker Matchmaker => GameMode?.Matchmaker;

        public SfaServer Server => GameMode?.Server;

        public MatchmakerBattleState State { get; set; } = MatchmakerBattleState.Created;

        public InstanceInfo InstanceInfo { get; } = new();

        public virtual void Init()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void InstanceStateChanged(InstanceState state)
        {

        }
    }
}
