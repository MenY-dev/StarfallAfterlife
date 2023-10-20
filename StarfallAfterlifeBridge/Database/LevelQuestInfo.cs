using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class LevelQuestInfo
    {
        public int Level {  get; set; }

        public Faction Faction {  get; set; }

        public List<int> Logics { get; set; } = new();
    }
}
