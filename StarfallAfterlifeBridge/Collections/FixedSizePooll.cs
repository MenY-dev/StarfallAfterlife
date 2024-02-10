using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Collections
{
    public class FixedSizePooll<T> : IList<T>
    {
        public int MaxCapacity { get; protected set; } = 1;

        public int Count => _size;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _array[(index + _pos) % _size];
            set => _array[(index + _pos) % _size] = value;
        }

        private int _size = 0;
        private int _pos = 0;
        private T[] _array = new T[1];

        public FixedSizePooll(int maxSize)
        {
            MaxCapacity = Math.Max(1, maxSize);
        }

        public T Push(T item)
        {
            if (_size == _array.Length)
            {
                Grow();
            }

            T oldItem;

            if (_size == MaxCapacity)
            {
                oldItem = _array[_pos];
                _array[_pos] = item;
            }
            else
            {
                oldItem = default;
                _array[_size] = item;
            }

            if (_size >= _array.Length)
            {
                Roll();
            }
            else
            {
                _size++;
            }

            return oldItem;
        }

        private void Roll()
        {
            int tmp = _pos + 1;

            if (tmp >= _array.Length)
                tmp = 0;

            _pos = tmp;
        }

        private void Grow()
        {
            int newCapacity = _array.Length * 2;

            if ((uint)newCapacity > Array.MaxLength)
                newCapacity = Array.MaxLength;

            newCapacity = Math.Min(newCapacity, MaxCapacity);

            if (_array.Length != newCapacity)
                SetCapacity(newCapacity);
        }

        private void SetCapacity(int capacity)
        {
            T[] newArray = new T[capacity];

            if (_size > 0)
            {

                if (_pos == 0)
                {
                    Array.Copy(_array, 0, newArray, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _pos, newArray, 0, _size - _pos);
                    Array.Copy(_array, 0, newArray, _size - _pos, _pos);
                }
            }

            _array = newArray;
            _pos = 0;
        }

        public int IndexOf(T item)
        {
            var index = 0;

            for (int i = _pos; i < _size; i++)
            {
                if (_array[i]?.Equals(item) == true)
                    return index;

                index++;
            }

            for (int i = 0; i < _pos; i++)
            {
                if (_array[i]?.Equals(item) == true)
                    return index;

                index++;
            }

            return -1;
        }

        public void Clear()
        {
            _array = new T[1];
            _pos = 0;
            _size = 0;
        }

        public bool Contains(T item) => _array.Any(i => i?.Equals(item) == true);

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_pos == 0)
            {
                Array.Copy(_array, 0, array, arrayIndex, _size);
            }
            else
            {
                Array.Copy(_array, _pos, array, arrayIndex, _size - _pos);
                Array.Copy(_array, 0, array, arrayIndex + _size - _pos, _pos);
            }
        }

        void ICollection<T>.Add(T item) => Push(item);

        void IList<T>.Insert(int index, T item) => throw new NotImplementedException();

        void IList<T>.RemoveAt(int index) => throw new NotImplementedException();

        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _pos; i < _size; i++)
                yield return _array[i];

            for (int i = 0; i < _pos; i++)
                yield return _array[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
