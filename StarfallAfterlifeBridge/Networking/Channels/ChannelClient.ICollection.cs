using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public partial class ChannelClient : ICollection<Channel>
    {
        protected List<Channel> Channels { get; } = new();

        public Channel this[int id]
        {
            get
            {
                lock (SendLocker)
                {
                    return this.FirstOrDefault(i => i.Id == id);
                }
            }
        }

        public Channel this[string name]
        {
            get
            {
                lock (SendLocker)
                {
                    return this.FirstOrDefault(i => i.Name == name);
                }
            }
        }

        public int Count => Channels.Count;

        public bool IsReadOnly => false;

        public void Add(Channel item)
        {
            lock (SendLocker)
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

        public void AddRange(IEnumerable<Channel> items)
        {
            lock (SendLocker)
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

        public bool Remove(Channel item)
        {
            lock (SendLocker)
                return Channels.Remove(item);
        }

        public void Clear()
        {
            lock (SendLocker)
                Channels.Clear();
        }

        public bool Contains(Channel item)
        {
            lock (SendLocker)
                return Channels.Contains(item);
        }

        public void CopyTo(Channel[] array, int arrayIndex)
        {
            lock (SendLocker)
                Channels.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Channel> GetEnumerator() => Channels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Channels.GetEnumerator();
    }
}
