using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Tasks
{
    public class EventWaiter<TEventArgs> where TEventArgs : EventArgs
    {
        private Action<EventHandler<TEventArgs>> Subscription;

        private Action<EventHandler<TEventArgs>> Unsubscription;

        private Func<object, TEventArgs, bool> Predicate;

        public EventWaiter<TEventArgs> Subscribe(Action<EventHandler<TEventArgs>> subscription)
        {
            Subscription = subscription;
            return this;
        }

        public EventWaiter<TEventArgs> Unsubscribe(Action<EventHandler<TEventArgs>> unsubscription)
        {
            Unsubscription = unsubscription;
            return this;
        }

        public EventWaiter<TEventArgs> Where(Func<object, TEventArgs, bool> predicate)
        {
            Predicate = predicate;
            return this;
        }

        public Task<bool> Start(int millisecondsTimeout = -1)
        {
            object lockher = new();
            bool completed = false;
            AutoResetEvent autoResetEvent = new(false);

            EventHandler<TEventArgs> handler = (sender, e) =>
            {
                lock (lockher)
                {
                    if (completed == true)
                        return;

                    if (Predicate is null ||
                        Predicate.Invoke(sender, e) == true)
                    {
                        completed = true;
                        autoResetEvent.Set();
                    }
                }
            };

            Subscription?.Invoke(handler);

            return Task<bool>.Factory.StartNew(() =>
            {
                bool result = true;
                var waitingTask = Task.Factory.StartNew(
                    () => autoResetEvent.WaitOne(millisecondsTimeout),
                    TaskCreationOptions.LongRunning);

                result = waitingTask.Result;
                Unsubscription?.Invoke(handler);
                autoResetEvent.Dispose();

                return result;
            }, TaskCreationOptions.LongRunning);
        }

        public static EventWaiter<TEventArgs> Create()
        {
            return new EventWaiter<TEventArgs>();
        }
    }


}
