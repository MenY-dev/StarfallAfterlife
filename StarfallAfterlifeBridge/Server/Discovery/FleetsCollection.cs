using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class FleetsCollection : ICollection<DiscoveryFleet>
    {
        public int Count => ((ICollection<DiscoveryFleet>)InternalList).Count;

        public bool IsReadOnly => true;

        protected List<DiscoveryFleet> InternalList { get; } = new();

        protected List<int> FreeIDs { get; } = new();

        public void Add(DiscoveryFleet item)
        {
            int id;

            if (FreeIDs.Count > 0)
            {
                id = FreeIDs.Last();
                item.Id = id;
                InternalList[id] = item;
            }
            else
            {
                id = InternalList.Count + 4000000;
                item.Id = id;
                InternalList.Add(item);
            }
        }

        public bool Remove(DiscoveryFleet item)
        {
            return Remove(item.Id);
        }

        public bool Remove(int id)
        {
            bool result = true;

            if (id - 4000000 == InternalList.Count - 1)
            {
                InternalList.RemoveAt(InternalList.Count - 1);
            }
            else
            {
                InternalList[id - 4000000] = null;
                FreeIDs.Add(id);

                for (int i = InternalList.Count - 1; i > -1; i--)
                {
                    if (InternalList[i] != null)
                        break;

                    InternalList.RemoveAt(i);

                    if (FreeIDs.Contains(i))
                        FreeIDs.Remove(i);
                }
            }

            return result;
        }

        public void Clear()
        {
            InternalList.Clear();
            FreeIDs.Clear();
        }

        public bool Contains(DiscoveryFleet item) => InternalList.Contains(item);

        public void CopyTo(DiscoveryFleet[] array, int arrayIndex) => InternalList.CopyTo(array, arrayIndex);

        public IEnumerator<DiscoveryFleet> GetEnumerator() => InternalList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => InternalList.GetEnumerator();
    }
}
