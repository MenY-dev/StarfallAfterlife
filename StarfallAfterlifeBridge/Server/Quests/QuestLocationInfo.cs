using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class QuestLocationInfo
    {
        public string Type { get; set; }

        public int QuestId { get; set; }

        public string ConditionId { get; set; }

        public DiscoveryObjectType ObjectType { get; set; }

        public int ObjectId { get; set; }

        public int SystemId { get; set; }
    }
}
