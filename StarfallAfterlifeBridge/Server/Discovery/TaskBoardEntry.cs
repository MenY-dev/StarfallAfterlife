using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class TaskBoardEntry
    {
        public int QuestId;
        public int LogicId;
        public QuestLogicInfo Logic;
        public QuestReward Reward;
        public StarSystemObject Owner;
    }
}
