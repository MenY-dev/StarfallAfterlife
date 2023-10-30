using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class InventoryStorage : ICollection<InventoryItem>, ICharInventoryStorage, ICloneable
    {
        public InventoryItem this[InventoryItem item] => this[item.Id, item.UniqueData];

        public InventoryItem this[SfaItem item, string uniqueData = null]
        {
            get
            {
                if (item is null)
                    return InventoryItem.Empty;

                return this[item.Id, uniqueData];
            }
        }

        public InventoryItem this[int itemId, string uniqueData = null]
        {
            get
            {
                if (Bindings.TryGetValue(itemId, out var items))
                    return items.FirstOrDefault(i => i.UniqueData == uniqueData, InventoryItem.Empty);

                return InventoryItem.Empty;
            }
        }

        public int Count { get; protected set; }

        public bool IsReadOnly => false;

        protected Dictionary<int, List<InventoryItem>> Bindings { get; } = new();

        protected IEnumerable<InventoryItem> Items => Bindings.Values.SelectMany(i => i);

        public InventoryItem[] GetAll(int itemId) =>
            Bindings.TryGetValue(itemId, out var variants) ? variants.ToArray() : null;

        public InventoryItem[] GetAll(SfaItem item) =>
            item is not null && Bindings.TryGetValue(item.Id, out var variants) ? variants.ToArray() : null;

        void ICollection<InventoryItem>.Add(InventoryItem item)
        {
            if (item.IsEmpty)
                return;

            Add(item.Id, item.Type, item.Count, item.IGCPrice, item.BGCPrice, item.UniqueData);
        }

        public int Add(SfaItem item, int count = 1, string uniqueData = default) =>
            item is null ? 0 : Add(item.Id, item.ItemType, count, item.IGC, item.BGC, uniqueData);

        public int Add(InventoryItem item, int count = 1) =>
            item.IsEmpty ? 0 : Add(item.Id, item.Type, count, item.IGCPrice, item.BGCPrice, item.UniqueData);

        protected int Add(int itemId, InventoryItemType itemType, int count, int igc = 0, int bgc = 0, string uniqueData = default)
        {
            if (itemId == 0)
                return 0;

            count = Math.Max(0, count);
            List<InventoryItem> variants = null;
            InventoryItem item = InventoryItem.Empty;

            if (Bindings.TryGetValue(itemId, out variants) == false)
                Bindings[itemId] = variants = new();

            if (string.IsNullOrWhiteSpace(uniqueData))
                uniqueData = null;

            var index = variants.FindIndex(i => i.UniqueData == uniqueData);

            if (index > -1 && (item = variants[index]).IsEmpty == false)
            {
                item.Count += count;

                if (igc > 0)
                    item.IGCPrice = igc;

                if (bgc > 0)
                    item.BGCPrice = bgc;

                variants[index] = item;
            }
            else
            {
                item = new InventoryItem()
                {
                    Id = itemId,
                    Type = itemType,
                    Count = count,
                    IGCPrice = igc,
                    BGCPrice = bgc,
                    UniqueData = uniqueData
                };

                variants.Add(item);
                UpdateCount();
            }

            return count;
        }

        bool ICollection<InventoryItem>.Remove(InventoryItem item)
        {
            if (item.IsEmpty)
                return false;

            var result = Bindings.Remove(item.Id);
            UpdateCount();

            return result;
        }

        public int Remove(InventoryItem item, int count = 1) =>
            item.IsEmpty ? 0 : Remove(item.Id, count, item.UniqueData);

        public int Remove(SfaItem item, int count = 1, string uniqueData = default) =>
            item is null ? 0 : Remove(item.Id, count, uniqueData);

        public int Remove(int itemId, int count = 1, string uniqueData = default)
        {
            count = Math.Max(0, count);

            if (string.IsNullOrWhiteSpace(uniqueData))
                uniqueData = null;

            if (Bindings.TryGetValue(itemId, out var items) == true)
            {
                var index = items.FindIndex(i => i.UniqueData == uniqueData);

                if (index < 0 || items is null)
                    return 0;

                var item = items[index];
                var toRemove = Math.Min(item.Count, count);

                item.Count -= toRemove;

                if (item.Count < 1)
                {
                    items.RemoveAt(index);
                }
                else
                {
                    items[index] = item;
                }

                if (items.Count < 1)
                    Bindings.Remove(itemId);

                UpdateCount();
                return toRemove;
            }

            return 0;
        }

        public void Clear()
        {
            Bindings.Clear();
            UpdateCount();
        }

        public bool Contains(InventoryItem item)
        {
            if (item.IsEmpty)
                return false;

            return Contains(item.Id, item.UniqueData);
        }

        public bool Contains(int itemId, string uniqueData = default)
        {
            return
                Bindings.TryGetValue(itemId, out var items) == true &&
                items?.Any(i => i.UniqueData == uniqueData) == true;
        }

        public void CopyTo(InventoryItem[] array, int arrayIndex)
        {

            Items.ToArray().CopyTo(array, arrayIndex);
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

            foreach (var item in Items)
                ((ICollection<InventoryItem>)clone).Add(item.Clone());

            return clone;
        }

        protected void UpdateCount() => Count = Items.Count();
    }
}
