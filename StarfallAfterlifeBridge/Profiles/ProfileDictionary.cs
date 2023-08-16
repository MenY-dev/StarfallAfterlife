using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ProfileDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public virtual TValue this[TKey key] { get => GetValue(key); set => SetValue(key, value, true); }

        public virtual ICollection<TKey> Keys => InternalDictionary.Keys;

        public virtual ICollection<TValue> Values => InternalDictionary.Values;

        public virtual int Count => InternalDictionary.Count;

        public virtual bool IsReadOnly => false;

        protected virtual Dictionary<TKey, TValue> InternalDictionary { get; } = new();

        protected virtual void SetValue(TKey key, TValue value, bool addNew = true)
        {
            if (InternalDictionary.ContainsKey(key))
                InternalDictionary[key] = value;
            else if (addNew == true)
                InternalDictionary.Add(key, value);
        }

        protected virtual TValue GetValue(TKey key)
        {
            if (InternalDictionary.ContainsKey(key))
                return InternalDictionary[key];

            return default;
        }

        public virtual void Add(TKey key, TValue value) => SetValue(key, value);

        public virtual void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public virtual void Clear()
        {
            foreach (var item in InternalDictionary)
                Remove(item);
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item) => InternalDictionary.Contains(item);

        public virtual bool ContainsKey(TKey key) => InternalDictionary.ContainsKey(key);

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)InternalDictionary).CopyTo(array, arrayIndex);
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => InternalDictionary.GetEnumerator();

        public virtual bool Remove(TKey key)
        {
            if (InternalDictionary.ContainsKey(key))
            {
                InternalDictionary.Remove(key);
                return true;
            }

            return false;
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public virtual bool TryGetValue(TKey key, out TValue value) => InternalDictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => InternalDictionary.GetEnumerator();
    }
}
