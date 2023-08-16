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

        protected object Lokher = new();

        protected Task QueueTask { get; set; }

        protected CancellationTokenSource WaitingCancellation { get; set; }

        public void Invoke(Action action)
        {
            if (action is null)
                return;

            lock (Lokher)
            {
                Queue.Enqueue(action);
                WaitingCancellation?.Cancel();

                if (QueueTask == null)
                    ProcessActions();
            }
        }

        protected virtual void ProcessActions()
        {
            if (Queue.Count == 0)
                return;

            lock (Lokher)
            {
                QueueTask = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Action action;

                            lock (Lokher)
                                action = Queue.Dequeue();

                            action?.Invoke();
                        }
                        catch { }

                        if (Queue.Count == 0)
                        {
                            lock (Lokher)
                                WaitingCancellation = new CancellationTokenSource();

                            Task.Delay((int)(WaitTime * 1000), WaitingCancellation.Token)
                                .ContinueWith(t => { WaitingCancellation = null; })
                                .Wait();
                        }

                        lock (Lokher)
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

        public void Dispose()
        {
            lock (Lokher)
            {
                Queue?.Clear();
                WaitingCancellation?.Dispose();
            }
        }
    }
}
