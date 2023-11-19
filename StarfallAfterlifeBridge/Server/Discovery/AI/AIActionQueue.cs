using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class AIActionQueue : AIAction
    {
        public List<AIAction> Queue { get; } = new();

        protected List<AIActionState> Results { get; } = new();

        protected AIAction CurrentAction { get; set; }

        public AIAction LastAction { get; protected set; }

        public QueueCompletionHandling CompletionHandling { get; set; }

        public override void Start()
        {
            base.Start();
            LastAction = null;
            CurrentAction = null;
        }

        public override void Update()
        {
            base.Update();

            if (State is not AIActionState.Started)
                return;

            var action = CurrentAction;

            if (action is null && Queue.Count == 0)
            {
                HandleEndOfQueue();
                return;
            }

            if (action is null ||
                action.State is not AIActionState.Started)
            {
                action?.Stop();

                if (CompletionHandling is QueueCompletionHandling.All &&
                    action is not null &&
                    action.State is not AIActionState.Completed)
                {
                    State = AIActionState.Failed;
                    Queue.Clear();
                    return;
                }

                if ((action = StartNextAction()) is null)
                {
                    HandleEndOfQueue();
                    return;
                }
            }

            action.Update();
        }

        protected void HandleEndOfQueue()
        {
            CurrentAction = null;

            if (CompletionHandling is QueueCompletionHandling.Any)
            {
                State = Results.Any(r => r is not AIActionState.Completed) ?
                        AIActionState.Completed :
                        AIActionState.Failed;
            }
            else
            {
                State = AIActionState.Completed;
            }

            return;
        }

        public override void Stop()
        {
            if (State is AIActionState.Started)
                CurrentAction?.Stop();

            Queue.Clear();
            CurrentAction = null;
            base.Stop();
        }

        protected virtual AIAction StartNextAction()
        {
            AIAction newAction = null;

            while (newAction is null && Queue.Count > 0)
            {
                newAction = Queue[0];
                Queue.RemoveAt(0);
            }

            if (newAction is not null)
            {
                LastAction = newAction;
                newAction.AI = AI;
                newAction.Start();
            }

            return CurrentAction = newAction;
        }
    }
}
