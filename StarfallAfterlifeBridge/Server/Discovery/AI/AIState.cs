using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class AIState
    {
        public bool Looped { get; set; } = false;

        public bool Default { get; set; } = false;

        public string Name { get; set; }

        public bool Completed { get; protected set; }

        public object Context { get; set; }

        public FleetAI AI { get; set; }

        public Func<AIState, IAINode> Action { get; set; }

        public Action<AIState, IAINode> OnEnd { get; set; }

        public AIStateMachine StateMachine { get; set; }

        public IAINode CurrentAction { get; protected set; }

        public void Start()
        {
            Completed = false;
        }

        public void Update()
        {
            if (Completed == true &&
                Looped == false)
                return;

            if (CurrentAction is null &&
                Action?.Invoke(this) is IAINode newAction)
            {
                newAction.AI = AI;
                CurrentAction = newAction;
                CurrentAction.Start();
            }

            if (CurrentAction is IAINode action)
            {
                action.Update();

                if (action.State != AINodeState.Started)
                {
                    Completed = true;
                    CurrentAction = null;
                    OnEnd?.Invoke(this, action);
                }
            }
            else if (Looped == false)
            {
                Completed = true;
            }
        }

        public void Stop()
        {
            Completed = true;
            CurrentAction?.Stop();
        }
    }
}
