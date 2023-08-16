using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum QuestType : byte
    {
        Task = 0,
        MainQuestLine = 1,
        Daily = 2,
        UniqueQuestLine = 3,
        HouseTask = 4,
        HouseDoctrine = 5,
    }
}
