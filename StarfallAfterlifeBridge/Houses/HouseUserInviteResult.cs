using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public enum HouseUserInviteResult
    {
        Success = 0,
        AlreadyInHouse = 1,
        EnemyFaction = 2,
        NoRecruitRights = 3,
        UserNotFound = 4,
        UserAlreadyInvited = 5,
        Unknown = 254,
    }
}
