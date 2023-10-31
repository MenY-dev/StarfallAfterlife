using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public enum MatchmakerBattleState
    {
        Created = 0,
        PendingPlayers = 1,
        PendingMatch = 2,
        Started = 3,
        Finished = 4
    }
}
