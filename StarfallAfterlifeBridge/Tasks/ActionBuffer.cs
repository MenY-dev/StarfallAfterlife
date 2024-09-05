using StarfallAfterlife.Bridge.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Tasks
{
    public class ActionBuffer : IDisposable
    {
        public float WaitTime { get; set; } = 10;

        protected Queue<Action> Queue { get; } = new();

        protected List<DelayedAction> DelayedQueue { get; } = new();

        protected Task QueueTask { get; set; }

        protected Task DelayedQueueTask { get; set; }

        protected CancellationTokenSource WaitingCancellation { get; set; }

        protected CancellationTokenSource DelayedWaitingCancellation { get; set; }

        protected object _locker = new();
        protected object _invokeLocker = new();

        protected record class DelayedAction(Action Action, DateTime Time);

        public void Invoke(Action action)
        {
            if (action is null)
                return;

            lock (_locker)
            {
                Queue.Enqueue(action);
                WaitingCancellation?.Cancel();

                if (QueueTask == null)
                    ProcessActions();
            }
        }

        public void Invoke(Action action, int delay) =>
            Invoke(action, TimeSpan.FromMilliseconds(delay));

        public void Invoke(Action action, TimeSpan delay) =>
            Invoke(action, DateTime.Now + delay);

        protected void Invoke(Action action, DateTime time)
        {
            if (action is null)
                return;

            lock (_locker)
            {
                DelayedQueue.Add(new(action, time));
                DelayedWaitingCancellation?.Cancel();

                if (DelayedQueueTask == null)
                    ProcessDelayedActions();
            }
        }

        protected virtual void ProcessActions()
        {
            lock (_locker)
            {
                if (Queue.Count == 0)
                    return;

                QueueTask = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Action action;

                            lock (_locker)
                                action = Queue.Dequeue();

                            lock (_invokeLocker)
                                action?.Invoke();
                        }
                        catch (Exception e)
                        {
                            SfaDebug.Print(e, GetType().Name);
                        }

                        if (Queue.Count == 0)
                        {
                            lock (_locker)
                                WaitingCancellation = new CancellationTokenSource();

                            Task.Delay((int)(WaitTime * 1000), WaitingCancellation.Token)
                                .ContinueWith(t => { WaitingCancellation = null; })
                                .Wait();
                        }

                        lock (_locker)
                        {
                            if (Queue.Count == 0)
                            {
                                QueueTask = null;
                                return;
                            }
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        protected virtual void ProcessDelayedActions()
        {
            lock (_locker)
            {
                if (DelayedQueue.Count == 0)
                    return;

                DelayedQueueTask = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var time = DateTime.Now;

                        while (true)
                        {
                            try
                            {
                                DelayedAction entry;

                                lock (_locker)
                                {
                                    entry = DelayedQueue.FirstOrDefault(t => t.Time <= time);

                                    if (entry is null)
                                        break;

                                    DelayedQueue.Remove(entry);
                                }

                                lock (_invokeLocker)
                                    entry.Action?.Invoke();
                            }
                            catch { }
                        }

                        int waitingTime;

                        lock (_locker)
                        {
                            if (DelayedQueue.Count > 0)
                                waitingTime = (DelayedQueue.Min(a => a.Time) - time).Milliseconds;
                            else
                                waitingTime = (int)(WaitTime * 1000);
                        }

                        if (DelayedQueue.Count == 0)
                        {
                            lock (_locker)
                                DelayedWaitingCancellation = new CancellationTokenSource();

                            Task.Delay((int)(waitingTime), DelayedWaitingCancellation.Token)
                                .ContinueWith(t => { DelayedWaitingCancellation = null; })
                                .Wait();
                        }

                        lock (_locker)
                        {
                            if (DelayedQueue.Count == 0)
                            {
                                QueueTask = null;
                                return;
                            }
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        public void Dispose()
        {
            lock (_locker)
            {
                Queue?.Clear();
                WaitingCancellation?.Dispose();
                DelayedQueue?.Clear();
                DelayedWaitingCancellation?.Dispose();
            }
        }
    }
}
