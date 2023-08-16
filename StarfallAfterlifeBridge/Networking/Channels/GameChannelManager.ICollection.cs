using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public partial class GameChannelManager : ICollection<GameChannel>
    {
        protected List<GameChannel> Channels { get; } = new();

        public GameChannel this[int id]
        {
            get
            {
                lock (ClientsLocker)
                {
                    foreach (var item in Channels)
                        if (item.Id == id)
                            return item;

                    return null;
                }
            }
        }

        public GameChannel this[string name]
        {
            get
            {
                lock (ClientsLocker)
                {
                    foreach (var item in Channels)
                        if (item.Name == name)
                            return item;

                    return null;
                }
            }
        }

        public int Count => Channels.Count;

        public bool IsReadOnly => false;

        public void Add(GameChannel item)
        {
            lock (ClientsLocker)
            {
                if (item is null)
                    return;

                if (Contains(item))
                    Remove(item);

                var old = this[item.Id];

                if (old is not null)
                    Remove(old);

                Channels.Add(item);
            }
        }

        public void AddRange(IEnumerable<GameChannel> items)
        {
            lock (ClientsLocker)
            {
                if (items is null)
                    return;

                foreach (var item in items)
                {
                    if (Contains(item))
                        Remove(item);

                    var old = this[item.Id];

                    if (old is not null)
                        Remove(old);

                    Channels.Add(item);
                }
            }
        }

        public bool Remove(GameChannel item)
        {
            lock (ClientsLocker)
                return Channels.Remove(item);
        }

        public void Clear()
        {
            lock (ClientsLocker)
                Channels.Clear();
        }

        public bool Contains(GameChannel item)
        {
            lock (ClientsLocker)
                return Channels.Contains(item);
        }

        public void CopyTo(GameChannel[] array, int arrayIndex)
        {
            lock (ClientsLocker)
                Channels.CopyTo(array, arrayIndex);
        }

        public IEnumerator<GameChannel> GetEnumerator() => Channels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Channels.GetEnumerator();
    }
}
