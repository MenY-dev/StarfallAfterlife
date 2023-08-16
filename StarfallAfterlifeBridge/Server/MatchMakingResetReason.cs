using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum MatchMakingResetReason : byte
    {
        Unknown = 0,
        LostMember = 1,
        Cancelation = 2,
        ServerError = 3,
    }
}
