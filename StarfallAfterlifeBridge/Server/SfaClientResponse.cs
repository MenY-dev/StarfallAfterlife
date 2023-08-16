using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClientResponse
    {
        public SfaServerAction Action { get; protected set; }

        public string Text { get; protected set; }

        public Guid ResponseId { get; protected set; }

        public bool IsSuccess { get; protected set; } = false;

        private AutoResetEvent ResponseEvent;

        public SfaClientResponse(Guid responseId)
        {
            ResponseId = responseId;
        }


        public SfaClientResponse(Guid responseId, string text, SfaServerAction action)
        {
            ResponseId = responseId;
            Text = text;
            Action = action;
        }

        private void OnTextInput(string text, SfaServerAction action)
        {
            Action = action;
            Text = text;
            IsSuccess = true;
            ResponseEvent?.Set();
        }

        public virtual Action<string, SfaServerAction> CreateHandler()
        {
            return OnTextInput;
        }

        public Task<SfaClientResponse> Wait(int timeout = -1)
        {
            if (ResponseEvent is not null)
                return Task.FromResult(this);

            ResponseEvent = new AutoResetEvent(false);

            return Task<SfaClientResponse>.Factory.StartNew(() =>
            {
                if (timeout < 0)
                    ResponseEvent.WaitOne();
                else
                    ResponseEvent.WaitOne(timeout);

                ResponseEvent?.Dispose();
                ResponseEvent = null;

                return this;
            }, TaskCreationOptions.LongRunning);
        }
    }
}
