using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class InventoryStorage : ICollection<InventoryItem>, ICharInventoryStorage, ICloneable
    {
        public InventoryItem this[SfaItem item]
        {
            get
            {
                if (item is null)
                    return null;

                return this[item.Id];
            }
        }

        public InventoryItem this[int itemId]
        {
            get
            {
                if (Bindings.TryGetValue(itemId, out var item))
                    return item;

                return null;
            }
        }

        public int Count => Bindings.Count;

        public bool IsReadOnly => false;

        protected Dictionary<int, InventoryItem> Bindings { get; } = new();

        protected ICollection<InventoryItem> Items => Bindings.Values;

        void ICollection<InventoryItem>.Add(InventoryItem item)
        {
            if (item is null)
                return;

            Add(item.Id, item.Type, item.Count, item.IGCPrice, item.BGCPrice);
        }

        public InventoryItem Add(SfaItem item, int count = 1) =>
            item is null ? null : Add(item.Id, item.ItemType, count, item.IGC, item.BGC);

        public InventoryItem Add(InventoryItem item, int count = 1) =>
            item is null ? null : Add(item.Id, item.Type, count, item.IGCPrice, item.BGCPrice);

        protected InventoryItem Add(int itemId, InventoryItemType itemType, int count, int igc = -1, int bgc = -1)
        {
            if (Bindings.TryGetValue(itemId, out var item))
            {
                item.Count += Math.Max(0, count);

                if (igc > -1)
                    item.IGCPrice = igc;

                if (bgc > -1)
                    item.BGCPrice = bgc;

                return item;
            }

            var newItem = new InventoryItem()
            {
                Id = itemId,
                Type = itemType,
                Count = Math.Max(0, count),
                IGCPrice = igc,
                BGCPrice = bgc
            };

            Bindings.Add(itemId, newItem);
            return newItem;
        }

        bool ICollection<InventoryItem>.Remove(InventoryItem item)
        {
            if (item is null)
                return false;

            return Bindings.Remove(item.Id);
        }

        public int Remove(SfaItem item, int count = 1) =>
            item is null ? 0 : Remove(item.Id, count);

        public int Remove(int itemId, int count = 1)
        {
            count = Math.Max(0, count);

            if (Bindings.TryGetValue(itemId, out var item) == true)
            {
                var toRemove = Math.Min(item.Count, count);
                item.Count -= toRemove;

                if (item.Count < 1)
                    Bindings.Remove(itemId);

                return toRemove;
            }

            return 0;
        }

        public void Clear()
        {
            Bindings.Clear();
        }

        public bool Contains(InventoryItem item)
        {
            if (item is null)
                return false;

            return Bindings.ContainsKey(item.Id);
        }

        public bool Contains(int itemId)
        {
            return Bindings.ContainsKey(itemId);
        }

        public void CopyTo(InventoryItem[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<InventoryItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Items).GetEnumerator();
        }

        object ICloneable.Clone() => Clone();

        public InventoryStorage Clone()
        {
            var clone = new InventoryStorage();

            foreach (var item in Bindings.Values)
                ((ICollection<InventoryItem>)clone).Add(item?.Clone());

            return clone;
        }
    }
}
