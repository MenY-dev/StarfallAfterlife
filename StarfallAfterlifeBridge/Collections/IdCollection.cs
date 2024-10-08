﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Collections
{
    public sealed class IdCollection<T> : ICollection<T>
    {
        private SortedList<int, T> _innerList = new();
        private Queue<int> _freeIds = new();
        private int _maxId;

        public int StartId { get; init; } = 0;

        public int IdsBufferThreshold { get; init; } = 1000;

        public int Count => _innerList.Count;

        public bool IsReadOnly => false;

        public T this[int id]
        {
            get
            {
                var index = _innerList.IndexOfKey(id);

                if (index < 0)
                    return default;

                return _innerList.GetValueAtIndex(index);
            }
            set
            {
                if (id < StartId)
                    return;

                _innerList[id] = value;
            }
        }

        void ICollection<T>.Add(T item) => Add(item);

        public int Add(T item)
        {
            var currentIndex = _innerList.IndexOfValue(item);

            if (currentIndex > -1)
                return _innerList.GetKeyAtIndex(currentIndex);

            int id;

            if (_freeIds.Count >= IdsBufferThreshold)
                id = _freeIds.Dequeue();
            else
                id = ++_maxId;

            _innerList.Add(id, item);
            return id;
        }

        public bool Remove(T item) => RemoveAt(_innerList.IndexOfValue(item));

        public bool RemoveId(int id) => RemoveAt(_innerList.IndexOfKey(id));

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _innerList.Count)
                return false;

            if (index < (_innerList.Count - 1))
                _freeIds.Enqueue(_innerList.GetKeyAtIndex(index));

            _innerList.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            _innerList.Clear();
            _freeIds.Clear();
            _maxId = StartId - 1;
        }

        public bool Contains(T item) => _innerList.ContainsValue(item);

        public bool ContainsId(int id) => _innerList.ContainsKey(id);

        public int IdOf(T item)
        {
            var index = _innerList.IndexOfValue(item);

            if (index < 0)
                return -1;

            return _innerList.GetKeyAtIndex(index);
        }

        public void CopyTo(T[] array, int arrayIndex) => _innerList.Values.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _innerList.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerList.Values.GetEnumerator();
    }
}
