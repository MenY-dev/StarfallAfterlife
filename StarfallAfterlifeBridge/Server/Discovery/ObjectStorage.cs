using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class ObjectStorage : IEnumerable<KeyValuePair<SfaItem, int>>
    {
        public string Name { get; set; }

        public int Capacity { get; set; } = -1;

        public StorageType Type { get; set; } = StorageType.None;

        public StarSystemObject Owner { get; set; }

        public bool IsStatic { get; set; } = false;

        protected Dictionary<SfaItem, int> Items { get; } = new();

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        public int this[SfaItem item]
        {
            get => GetCount(item);
            set => SetCount(item, value, true);
        }

        public ObjectStorage()
        {

        }

        public ObjectStorage(StarSystemObject owner, string name,
            StorageType type = StorageType.None, bool isStatic = false, int capacity = -1)
        {
            Owner = owner;
            Name = name;
            Type = type;
            IsStatic = isStatic;
            Capacity = capacity;
        }

        public int Add(int itemId) =>
            Add(Items.Keys.FirstOrDefault(i => i.Id == itemId), 1);

        public int Add(int itemId, int count) =>
            Add(Items.Keys.FirstOrDefault(i => i.Id == itemId), count);

        public int Add(SfaItem item) => Add(item, 1);

        public int Add(SfaItem item, int count)
        {
            int newCount = 0;

            if (item is null)
                return newCount;

            count = count < 0 ? 0 : count;

            if (Items.TryGetValue(item, out int currentCount) == true)
            {
                newCount = currentCount + count;
                Items[item] = newCount;
            }
            else
            {
                newCount = count;
                Items.Add(item, newCount);
            }

            Broadcast(new[] { new StorageItemInfo(item, newCount, count) });
            return newCount;
        }

        public int GetCount(int itemId) => Items.FirstOrDefault(i => i.Key.Id == itemId).Value;

        public int GetCount(SfaItem item)
        {
            if (Items.TryGetValue(item, out var count))
                return count;

            return 0;
        }


        public int SetCount(int itemId, int count) =>
            SetCount(Items.Keys.FirstOrDefault(i => i.Id == itemId), count, false);

        public int SetCount(SfaItem item, int count, bool addIfMissing = true)
        {
            if (item is null)
                return 0;

            count = count < 0 ? 0 : count;

            if (Items.TryGetValue(item, out int currentCount) == true)
            {
                int delta = count - currentCount;
                Items[item] = count;
                Broadcast(new[] { new StorageItemInfo(item, count, delta) });
            }
            else if (addIfMissing == true)
            {
                Add(item, count);
            }

            return count;
        }

        public bool Contains(int itemId) => Items.Keys.FirstOrDefault(i => i.Id == itemId) is not null;

        public bool Contains(SfaItem item) => Items.ContainsKey(item);

        public int Remove(int itemId) => Remove(Items.Keys.FirstOrDefault(i => i.Id == itemId), 1);

        public int Remove(int itemId, int count) => Remove(Items.Keys.FirstOrDefault(i => i.Id == itemId), count);

        public int Remove(SfaItem item) => Remove(item, 1);

        public int Remove(SfaItem item, int count)
        {
            if (item is null || count < 1)
                return 0;

            if (Items.TryGetValue(item, out int currentCount) == true)
            {
                int newCount = Math.Max(0, currentCount - count);
                int delta = currentCount - newCount;
                bool needRemove = IsStatic == false && newCount == 0;

                Items[item] = newCount;

                if (needRemove)
                    Items.Remove(item);

                Broadcast(new[] { new StorageItemInfo(item, newCount, delta) });
                return delta;
            }

            return 0;
        }

        public void Clear()
        {
            var items = Items.ToArray();

            if (IsStatic == false)
            {
                Items.Clear();
            }
            else
            {
                foreach (var key in Items.Keys)
                    Items[key] = 0;
            }

            Broadcast(items.Select(i => new StorageItemInfo(i.Key, 0, -i.Value)).ToArray());
        }

        public bool TryGetItem(SfaItem item, out int count) => Items.TryGetValue(item, out count);

        public int CalculateTotalWeight()
        {
            int weight = 0;

            foreach (var item in Items)
                weight += item.Key.Cargo * item.Value;

            return weight;
        }

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        public IEnumerator<KeyValuePair<SfaItem, int>> GetEnumerator() => Items.GetEnumerator();

        public void Update() => Broadcast(Array.Empty<StorageItemInfo>());

        protected virtual void Broadcast(StorageItemInfo[] changedItems)
        {
            Owner?.Broadcast<IObjectStorageListener>(
                l => l.OnObjectStorageUpdated(Owner, this, changedItems ?? Array.Empty<StorageItemInfo>()));
        }
    }
}
