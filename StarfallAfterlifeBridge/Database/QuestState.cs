using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum QuestState : byte
    {
        InProgress = 0,
        Done = 1,
        Failed = 2,
        Abandoned = 3,
    }
}
