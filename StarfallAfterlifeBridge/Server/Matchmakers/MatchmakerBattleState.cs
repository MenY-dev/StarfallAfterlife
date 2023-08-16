using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public enum MatchmakerBattleState
    {
        PendingPlayers = 0,
        PendingMatch = 1,
        Started = 2,
        Finished = 3
    }
}
