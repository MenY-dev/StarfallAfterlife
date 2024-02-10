using StarfallAfterlife.Bridge.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class SfaDebugMsgStorage : ICollection<SfaDebugMsgViewModel>, IList, INotifyCollectionChanged
    {
        public int Count => _innerList.Count;

        public bool IsReadOnly => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => _syncRoot;

        object IList.this[int index] { get => _innerList[index]; set => _innerList[index] = (SfaDebugMsgViewModel)value; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private FixedSizePooll<SfaDebugMsgViewModel> _innerList = new(1000);

        private int _blockReentrancyCount;

        private readonly object _syncRoot = new object();

        public void Add(SfaDebugMsgViewModel item)
        {
            CheckReentrancy();

            if (item is null)
                return;

            var lastItem = _innerList.LastOrDefault();

            if (lastItem?.IsStackable(item) == true)
            {
                lastItem.Count++;
                lastItem.Time = item.Time;
            }
            else
            {
                if (_innerList.Push(item) is SfaDebugMsgViewModel toRemove)
                {
                    OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, toRemove, 0));
                }
                else
                {
                    OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, item, _innerList.Count - 1));
                }
            }
        }


        public void Clear()
        {
            CheckReentrancy();
            _innerList.Clear();
            OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;

            if (handler != null)
            {
                _blockReentrancyCount++;

                try
                {
                    handler(this, e);
                }
                finally
                {
                    _blockReentrancyCount--;
                }
            }
        }

        protected void CheckReentrancy()
        {
            if (_blockReentrancyCount > 0)
            {
                if (CollectionChanged?.GetInvocationList().Length > 1)
                    throw new InvalidOperationException();
            }
        }

        public int IndexOf(SfaDebugMsgViewModel item) => _innerList.IndexOf(item);

        public bool Contains(SfaDebugMsgViewModel item) => _innerList.Contains(item);

        public void CopyTo(SfaDebugMsgViewModel[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

        bool ICollection<SfaDebugMsgViewModel>.Remove(SfaDebugMsgViewModel item) => throw new NotImplementedException();

        public IEnumerator<SfaDebugMsgViewModel> GetEnumerator() => _innerList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();

        int IList.Add(object value)
        {
            Add((SfaDebugMsgViewModel)value);
            return Count - 1;
        }

        bool IList.Contains(object value) => _innerList.Contains((SfaDebugMsgViewModel)value);

        int IList.IndexOf(object value) => _innerList.IndexOf((SfaDebugMsgViewModel)value);

        void IList.Insert(int index, object value) => throw new NotImplementedException();

        void IList.Remove(object value) => throw new NotImplementedException();

        void IList.RemoveAt(int index) => throw new NotImplementedException();

        void ICollection.CopyTo(Array array, int index) => CopyTo((SfaDebugMsgViewModel[])array, index);
    }
}
