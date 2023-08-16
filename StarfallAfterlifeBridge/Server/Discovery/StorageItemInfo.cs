using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public struct StorageItemInfo
    {
        public SfaItem Item { get; }
        public int Count { get; }
        public int Delta { get; }

        public StorageItemInfo(SfaItem item, int count, int delta)
        {
            Item = item;
            Count = count;
            Delta = delta;
        }
    }
}
