using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class ObjectStoragesCollection : ICollection<ObjectStorage>
    {
        public int Count =>Storages.Count;

        public bool IsReadOnly => false;

        public StarSystemObject Owner { get; set; }

        public ObjectStorage this[string storageName] { get => Storages?.FirstOrDefault(s => s.Name == storageName); }

        protected List<ObjectStorage> Storages { get; } = new();

        public void Add(ObjectStorage item)
        {
            if (item is null || Storages.Contains(item) == true)
                return;

            //item.Owner = Owner;
            Storages.Add(item);
            BroadcastAdded(item);
        }

        public bool Remove(string storageName) =>
            Remove(Storages.FirstOrDefault(s => s.Name == storageName));

        public bool Remove(ObjectStorage item)
        {
            bool result = false;

            if (Storages.Contains(item) == false)
                return result;

            //item.Owner = null;
            Storages.Remove(item);
            BroadcastRemoved(item);

            return result;
        }

        public void Clear()
        {
            var currentStorages = Storages.ToArray();

            foreach (var storage in currentStorages)
            {
                //storage.Owner = null;
                BroadcastRemoved(storage);
            }
        }

        public bool Contains(string storageName) => Storages?.Any(s => s.Name == storageName) ?? false;

        public bool Contains(ObjectStorage item) => Storages.Contains(item);

        public void CopyTo(ObjectStorage[] array, int arrayIndex) => Storages.CopyTo(array, arrayIndex);

        public IEnumerator<ObjectStorage> GetEnumerator() => Storages.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Storages.GetEnumerator();

        protected virtual void BroadcastAdded(ObjectStorage storage)
        {
            Owner?.Broadcast<IObjectStorageListener>(l => l.OnObjectStorageAdded(Owner, storage));
        }

        protected virtual void BroadcastRemoved(ObjectStorage storage)
        {
            Owner?.Broadcast<IObjectStorageListener>(l => l.OnObjectStorageRemoved(Owner, storage));
        }
    }
}
