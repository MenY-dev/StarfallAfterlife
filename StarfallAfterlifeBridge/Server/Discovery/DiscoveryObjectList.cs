using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryObjectList : DiscoveryObjectList<DiscoveryObject> { }

    public class DiscoveryObjectList<T> : ICollection<T> where T : DiscoveryObject
    {
        public T this[int id] { get => InnerList[id]; }

        public int Count => InnerList.Count;

        public bool IsReadOnly => false;

        protected SortedList<int, T> InnerList { get; } = new();

        public void Add(T item)
        {
            if (item is null || item.Id < 0 || InnerList.IndexOfKey(item.Id) > -1)
                return;

            InnerList.Add(item.Id, item);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public bool Contains(T item)
        {
            return InnerList.ContainsValue(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerList.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InnerList.Values.GetEnumerator();
        }

        public bool Remove(T item)
        {
            if (item is null || item.Id < 0)
                return false;

            int index = InnerList.IndexOfKey(item.Id);

            if (item.Id < 0 || InnerList.GetValueAtIndex(index) != item)
                return false;

            InnerList.RemoveAt(index);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerList.Values.GetEnumerator();
        }
    }
}
