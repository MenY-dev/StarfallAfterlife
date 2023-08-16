using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class QuestLineInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<QuestLineLogicInfo> Logics { get; set; } = new();
    }
}
