using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class CharacterEffectsCollection : IDictionary<int, DateTime>
    {
        public double this[int key] { get => Get(key); }

        DateTime IDictionary<int, DateTime>.this[int key] { get => _inner[key]; set => _inner[key] = value; }

        ICollection<int> IDictionary<int, DateTime>.Keys => _inner.Keys;

        ICollection<DateTime> IDictionary<int, DateTime>.Values =>_inner.Values;

        int ICollection<KeyValuePair<int, DateTime>>.Count => _inner.Count;

        bool ICollection<KeyValuePair<int, DateTime>>.IsReadOnly => false;

        private readonly Dictionary<int, DateTime> _inner = new();

        public void Add(int id, double duration)
        {
            DateTime currentTime = DateTime.Now;
            DateTime endTime;

            if (_inner.TryGetValue(id, out endTime) == false ||
                endTime < currentTime)
                endTime = currentTime;

            _inner[id] = endTime.AddHours(duration);
        }

        public double Get(int id)
        {
            var currentTime = DateTime.Now;
            var endTime = _inner.GetValueOrDefault(id);

            return Math.Max(0, (endTime - currentTime).TotalMinutes);
        }

        public bool Contains(int id) => Get(id) > 0;

        void IDictionary<int, DateTime>.Add(int key, DateTime value) => _inner.Add(key, value);

        void ICollection<KeyValuePair<int, DateTime>>.Add(KeyValuePair<int, DateTime> item) =>
            ((ICollection<KeyValuePair<int, DateTime>>)_inner).Add(item);

        void ICollection<KeyValuePair<int, DateTime>>.Clear() => _inner.Clear();

        bool ICollection<KeyValuePair<int, DateTime>>.Contains(KeyValuePair<int, DateTime> item) =>
            ((ICollection<KeyValuePair<int, DateTime>>)_inner).Contains(item);

        bool IDictionary<int, DateTime>.ContainsKey(int key) => _inner.ContainsKey(key);

        void ICollection<KeyValuePair<int, DateTime>>.CopyTo(KeyValuePair<int, DateTime>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<int, DateTime>>)_inner).CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<int, DateTime>> IEnumerable<KeyValuePair<int, DateTime>>.GetEnumerator() =>
            ((ICollection<KeyValuePair<int, DateTime>>)_inner).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

        bool IDictionary<int, DateTime>.Remove(int key) => _inner.Remove(key);

        bool ICollection<KeyValuePair<int, DateTime>>.Remove(KeyValuePair<int, DateTime> item) =>
            ((ICollection<KeyValuePair<int, DateTime>>)_inner).Remove(item);

        bool IDictionary<int, DateTime>.TryGetValue(int key, out DateTime value) =>
            _inner.TryGetValue(key, out value);
    }
}
