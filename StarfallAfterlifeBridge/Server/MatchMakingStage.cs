using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum MatchMakingStage : byte
    {
        Nothing = 0,
        WaitingPartyMembers = 1,
        SearchingOpponents = 2,
        StartingInstance = 3,
    }
}
