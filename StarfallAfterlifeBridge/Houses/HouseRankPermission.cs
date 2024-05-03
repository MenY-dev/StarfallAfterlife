using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    [Flags]
    public enum HouseRankPermission : int
    {
        None = 0,
        Invite = 1 << 1,
        Kick = 1 << 2,
        ChangeRank = 1 << 3,
        ChangeMessageOfTheDay = 1 << 4,
        OpenUpgrade = 1 << 5,
        TakeTasks = 1 << 6,
        DropTasks = 1 << 7,
        StartDoctrine = 1 << 8,
        All = -1,
    }
}
