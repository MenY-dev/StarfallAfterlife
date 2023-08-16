using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class SystemObjectsDictionary<TValue> : IDictionary<int, ICollection<TValue>>
    {
        protected Dictionary<int, ICollection<TValue>> Dictionary = new();

        public ICollection<TValue> this[int system]
        {
            get => TryGetValue(system, out var value) == true ? value : default;
            set => Dictionary[system] = value;
        }

        public ICollection<int> Keys => Dictionary.Keys;

        public ICollection<ICollection<TValue>> Values => Dictionary.Values;

        public int Count => Dictionary.Count;

        public bool IsReadOnly => false;

        public void Add(int system, TValue value)
        {
            if (Dictionary.TryGetValue(system, out ICollection<TValue> collection) == true)
            {
                if (collection == null)
                    Dictionary.Add(system, new HashSet<TValue> { value });
                else
                    collection.Add(value);
            }
            else
            {
                Dictionary.Add(system, new HashSet<TValue> { value });
            }
        }

        public void Add(int system, ICollection<TValue> values)
        {
            if (Dictionary.TryGetValue(system, out ICollection<TValue> collection) == true)
            {
                if (collection == null)
                    Dictionary.Add(system, new HashSet<TValue>(values));
                else
                    foreach (var item in values)
                        collection.Add(item);
            }
            else
            {
                Dictionary.Add(system, new HashSet<TValue>(values));
            }
        }

        public void Add(KeyValuePair<int, ICollection<TValue>> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Dictionary.Clear();
        }

        public bool Contains(KeyValuePair<int, ICollection<TValue>> item)
        {
            return Dictionary.Contains(item);
        }

        public bool ContainsKey(int system)
        {
            return Dictionary.ContainsKey(system);
        }

        void ICollection<KeyValuePair<int, ICollection<TValue>>>.CopyTo(KeyValuePair<int, ICollection<TValue>>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<int, ICollection<TValue>>>)Dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<int, ICollection<TValue>>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public bool RemoveSystem(int system)
        {
            return Dictionary.Remove(system);
        }

        public bool RemoveSystem(int system, out ICollection<TValue> values)
        {
            return Dictionary.Remove(system, out values);
        }

        public bool Remove(int system, TValue value)
        {
            if (Dictionary.TryGetValue(system, out var values) && values is not null)
                return values.Remove(value);

            return false;
        }

        public bool Remove(TValue value)
        {
            var result = false;
            var values = Dictionary.Values.FirstOrDefault(v => v.Contains(value));

            if (values is null)
                return result;

            result = values.Remove(value);

            if (values.Count < 1)
                Dictionary.Remove(Dictionary.FirstOrDefault(i => i.Value == values).Key);

            return result;
        }

        bool IDictionary<int, ICollection<TValue>>.Remove(int key)
        {
            return ((IDictionary<int, ICollection<TValue>>)Dictionary).Remove(key);
        }

        bool ICollection<KeyValuePair<int, ICollection<TValue>>>.Remove(KeyValuePair<int, ICollection<TValue>> item)
        {
            return ((ICollection<KeyValuePair<int, ICollection<TValue>>>)Dictionary).Remove(item);
        }

        public bool TryGetValue(int system, [MaybeNullWhen(false)] out ICollection<TValue> values)
        {
            return Dictionary.TryGetValue(system, out values);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Dictionary).GetEnumerator();
        }
    }
}
