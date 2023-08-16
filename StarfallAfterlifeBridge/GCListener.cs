using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge
{
    public static class GCListener
    {
        private static Dictionary<WeakReference, Action> Listeners = new();

        static GCListener()
        {
            GC.RegisterForFullGCNotification(10, 10);
            Task.Run(ListenerLoop);
        }

        private static void ListenerLoop()
        {
            while (true)
            {
                GC.WaitForFullGCApproach();
                GC.WaitForFullGCComplete();

                var toRemove = new List<WeakReference>();

                foreach (var listener in Listeners)
                {
                    if (listener.Key.Target is not null)
                        continue;

                    listener.Value?.Invoke();
                    toRemove.Add(listener.Key);
                }

                foreach (var item in toRemove)
                    Listeners.Remove(item);
            }
        }

        public static void Register(object obj, Action action)
        {
            Listeners.TryAdd(new WeakReference(obj), action);
        }
    }
}
