using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DiscoveryQuestLineStage
    {
        public int Position { get; set; } = 0;

        public List<DiscoveryQuestLineEntry> Entries { get; set; } = new();
    }
}
