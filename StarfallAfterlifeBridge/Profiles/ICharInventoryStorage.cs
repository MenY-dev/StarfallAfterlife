using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public interface ICharInventoryStorage
    {
        InventoryItem this[int itemId] { get; }

        InventoryItem Add(InventoryItem item, int count = 1);

        int Remove(int itemId, int count = 1);
    }
}
