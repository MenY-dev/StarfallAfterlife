using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Events
{
    public class MulticastEvent
    {
        protected List<object> Listeners { get; } = new();

        protected object Locker { get; } = new();

        public static MulticastEvent operator +(MulticastEvent e, object listener)
        {
            e?.Add(listener);
            return e;
        }

        public static MulticastEvent operator -(MulticastEvent e, object listener)
        {
            e?.Remove(listener);
            return e;
        }

        public virtual void Add(object listener)
        {
            if (listener is null)
                return;

            lock (Locker)
            {
                Remove(listener);
                Listeners.Add(listener);
            }
        }

        public virtual void Remove(object listener)
        {
            lock (Locker)
                Listeners.Remove(listener);
        }

        public virtual void Clear()
        {
            lock (Locker)
                Listeners.Clear();
        }

        public virtual IEnumerable<TListener> Get<TListener>()
        {
            foreach (var item in GetCurrentListeners())
                if (item is TListener listener)
                    yield return listener;
        }

        protected virtual object[] GetCurrentListeners()
        {
            lock (Locker)
                return Listeners.ToArray();
        }

        public virtual bool Contains(object listener)
        {
            return Listeners.Contains(listener);
        }

        public virtual void Broadcast<TListener>(Action<TListener> action)
        {
            if (action is null)
                return;

            foreach (var listener in Get<TListener>())
                action.Invoke(listener);
        }

        public virtual void Broadcast<TListener>(Action<TListener> action, Func<TListener, bool> predicate)
        {
            if (action is null || predicate is null)
                return;

            foreach (var listener in Get<TListener>())
                if (predicate.Invoke(listener) == true)
                    action.Invoke(listener);
        }
    }
}
