using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public sealed class DiscoveryLoop : IDisposable
    {
        public double FrameRate { get; set; } = 30;
        private Action callback;
        private Task loop = null;
        private CancellationTokenSource cts;
        private bool disposed;
        private readonly object locker = new();

        public DiscoveryLoop(Action callback, double frameRate = 30)
        {
            this.callback = callback;
            FrameRate = frameRate;
        }

        public void Start()
        {
            lock (locker)
            {
                if (disposed == true)
                    return;

                Stop();

                if (callback is null)
                    return;

                RunLoop();
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                if (disposed == true)
                    return;

                if (cts is not null)
                {
                    cts.Cancel();

                    try
                    {
                        loop?.Wait();
                    }
                    catch { }

                    cts.Dispose();
                    loop?.Dispose();
                    cts = null;
                    loop = null;
                }
            }
        }

        private void RunLoop()
        {
            cts = new CancellationTokenSource();
            loop = Task.Run(() =>
            {
                while (cts.Token.IsCancellationRequested == false)
                {
                    var callbackInvokeTime = DateTime.UtcNow;
                    callback.Invoke();

                    if (FrameRate > 0)
                    {
                        double targetFrameTime = 1 / FrameRate;
                        double callbackDuration = (DateTime.UtcNow - callbackInvokeTime).TotalSeconds;

                        if (targetFrameTime > callbackDuration)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(targetFrameTime - callbackDuration));
                        }
                    }

                    if (cts.Token.IsCancellationRequested)
                        return;
                }
            }, cts.Token);
        }

        private void Dispose(bool disposing)
        {
            lock (locker)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        Stop();
                        callback = null;
                    }

                    disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }
    }
}
