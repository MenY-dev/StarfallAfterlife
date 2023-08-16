using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class SfaCircleData
    {
        public int Level { get; } = 0;

        public Dictionary<int, EquipmentBlueprint> Equipments { get; } = new();

        public Dictionary<int, DiscoveryItem> DiscoveryItems { get; } = new();

        public SfaCircleData(int level)
        {
            Level = level;
        }
    }
}
