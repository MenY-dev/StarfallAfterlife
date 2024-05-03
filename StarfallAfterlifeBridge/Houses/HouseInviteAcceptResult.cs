using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public enum HouseInviteAcceptResult : byte
    {
        Success = 0,
        MembersLimitReached = 1,
        HouseDestroyed = 2,
        AlreadyInHouse = 3,
        InviteExpired = 4,
        Unknown = 254,
    }
}
