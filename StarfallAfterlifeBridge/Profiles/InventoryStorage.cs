using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class InventoryStorage : ICollection<InventoryItem>, ICharInventoryStorage, ICloneable
    {
        public InventoryItem this[SfaItem item, string uniqueData = null]
        {
            get
            {
                if (item is null)
                    return null;

                return this[item.Id, uniqueData];
            }
        }

        public InventoryItem this[int itemId, string uniqueData = null]
        {
            get
            {
                if (Bindings.TryGetValue(itemId, out var items))
                    return items.FirstOrDefault(i => i.UniqueData == uniqueData);

                return null;
            }
        }

        public int Count => Bindings.Count;

        public bool IsReadOnly => false;

        protected Dictionary<int, List<InventoryItem>> Bindings { get; } = new();

        protected IEnumerable<InventoryItem> Items => Bindings.Values.SelectMany(i => i);

        public InventoryItem[] GetAll(int itemId) =>
            Bindings.TryGetValue(itemId, out var variants) ? variants.ToArray() : null;

        public InventoryItem[] GetAll(SfaItem item) =>
            item is not null && Bindings.TryGetValue(item.Id, out var variants) ? variants.ToArray() : null;

        void ICollection<InventoryItem>.Add(InventoryItem item)
        {
            if (item is null)
                return;

            Add(item.Id, item.Type, item.Count, item.IGCPrice, item.BGCPrice, item.UniqueData);
        }

        public InventoryItem Add(SfaItem item, int count = 1, string uniqueData = default) =>
            item is null ? null : Add(item.Id, item.ItemType, count, item.IGC, item.BGC, uniqueData);

        public InventoryItem Add(InventoryItem item, int count = 1) =>
            item is null ? null : Add(item.Id, item.Type, count, item.IGCPrice, item.BGCPrice, item.UniqueData);

        protected InventoryItem Add(int itemId, InventoryItemType itemType, int count, int igc = -1, int bgc = -1, string uniqueData = default)
        {
            List<InventoryItem> variants = null;
            InventoryItem item = null;

            if (Bindings.TryGetValue(itemId, out variants) == false)
                Bindings[itemId] = variants = new();


            if ((item = variants.FirstOrDefault(i => i.UniqueData == uniqueData)) is not null)
            {
                item.Count += Math.Max(0, count);

                if (igc > -1)
                    item.IGCPrice = igc;

                if (bgc > -1)
                    item.BGCPrice = bgc;
            }
            else
            {
                item = new InventoryItem()
                {
                    Id = itemId,
                    Type = itemType,
                    Count = Math.Max(0, count),
                    IGCPrice = igc,
                    BGCPrice = bgc,
                    UniqueData = uniqueData
                };

                variants.Add(item);
            }

            return item;
        }

        bool ICollection<InventoryItem>.Remove(InventoryItem item)
        {
            if (item is null)
                return false;

            return Bindings.Remove(item.Id);
        }

        public int Remove(InventoryItem item, int count = 1) =>
            item is null ? 0 : Remove(item.Id, count, item?.UniqueData);

        public int Remove(SfaItem item, int count = 1, string uniqueData = default) =>
            item is null ? 0 : Remove(item.Id, count, uniqueData);

        public int Remove(int itemId, int count = 1, string uniqueData = default)
        {
            count = Math.Max(0, count);

            if (Bindings.TryGetValue(itemId, out var items) == true &&
                items.FirstOrDefault(i => i.UniqueData == uniqueData) is InventoryItem item)
            {
                var toRemove = Math.Min(item.Count, count);
                item.Count -= toRemove;

                if (item.Count < 1)
                    items.Remove(item);

                if (items.Count < 1)
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
                ((ICollection<InventoryItem>)clone).Add(item?.Clone());

            return clone;
        }
    }
}
