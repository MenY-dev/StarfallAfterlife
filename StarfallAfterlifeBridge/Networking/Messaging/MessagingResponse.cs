using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingResponse
    {
        public Guid Id { get; init; }

        public MessagingMethod Method { get; init; }

        public bool IsSuccess { get; protected set; }

        public bool IsCancelled { get; protected set; }

        public string Text { get; protected set; }

        public ReadOnlyMemory<byte> Data { get; protected set; }

        private HashSet<Action<MessagingResponse>> _continuationActions;
        private AutoResetEvent _completionEvent;
        private Task<MessagingResponse> _completionTask;
        private readonly object _locker = new();

        public static MessagingResponse Create(Guid id, MessagingMethod method)
        {
            return new() { Id = id, Method = method };
        }

        public static MessagingResponse Create(Guid id, string text)
        {
            return new() { Id = id, Text = text, Method = MessagingMethod.TextRequest };
        }

        public static MessagingResponse Create(Guid id, byte[] data)
        {
            return new() { Id = id, Data = data, Method = MessagingMethod.BinaryRequest };
        }

        public MessagingResponse OnSuccess(Action<MessagingResponse> handler)
        {
            lock (_locker)
            {
                if (IsSuccess == true)
                {
                    handler?.Invoke(this);
                    return this;
                }

                (_continuationActions ??= new()).Add(handler);
            }

            return this;
        }

        public Task<MessagingResponse> Wait(int timeout = -1)
        {
            lock (_locker)
            {
                if (_completionTask is not null)
                    return _completionTask;

                _completionEvent = new AutoResetEvent(false);

                return _completionTask = Task<MessagingResponse>.Factory.StartNew(() =>
                {
                    if (timeout < 0)
                        _completionEvent.WaitOne();
                    else
                        _completionEvent.WaitOne(timeout);

                    _completionEvent?.Dispose();
                    _completionEvent = null;

                    return this;
                }, TaskCreationOptions.LongRunning);
            }
        }

        public virtual void SetInput(string text)
        {
            lock (_locker)
            {
                Text = text;
                SetInput();
            }
        }

        public virtual void SetInput(ReadOnlyMemory<byte> data)
        {
            lock (_locker)
            {
                Data = data;
                SetInput();
            }
        }

        protected void SetInput()
        {
            lock (_locker)
            {
                IsSuccess = true;

                if (_continuationActions is not null)
                {
                    foreach (var action in _continuationActions)
                        action?.Invoke(this);

                    _continuationActions.Clear();
                    _continuationActions = null;
                }

                try
                {
                    _completionEvent?.Set();
                }
                catch { }
            }
        }
    }
}
