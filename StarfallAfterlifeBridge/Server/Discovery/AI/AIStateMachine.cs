using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class AIStateMachine : IAINode
    {
        public List<AIState> States { get; } = new();

        public List<AIWatchdog> Watchdogs { get; } = new();

        public AIState CurrentState { get; set; }

        public FleetAI AI { get; set; }

        AINodeState IAINode.State { get; set; }

        public string Name { get; set; }

        public void Update()
        {
            for (int i = 0; i < Watchdogs.Count; i++)
                Watchdogs.ElementAtOrDefault(i)?.Update();

            if (CurrentState is null)
                StartState(null);

            if (CurrentState is AIState state)
            {
                state.Update();

                if (state.Completed == true && CurrentState == state)
                    StartState(null);
            }
        }

        public AIState GetState(string name) =>
            States.FirstOrDefault(s => s.Name == name);

        public AIState GetDefaultState() =>
            States.FirstOrDefault(s => s.Default);

        public bool StartState(AIState state, object context = null)
        {
            CurrentState?.Stop();

            if (state is null)
                state = GetDefaultState();

            if (state is null || States.Contains(state) == false)
                return false;

            CurrentState = state;
            CurrentState.AI = AI;
            CurrentState.Context = context;
            CurrentState.StateMachine = this;
            CurrentState.Start();

            return true;
        }

        public bool StartStateByName(string name, object context = null)
        {
            CurrentState?.Stop();

            var state = name is null ? GetDefaultState() : GetState(name);

            if (state is null)
                return false;

            CurrentState = state;
            CurrentState.AI = AI;
            CurrentState.Context = context;
            CurrentState.StateMachine = this;
            CurrentState.Start();

            return true;
        }

        void IAINode.Start()
        {
            var now = DateTime.UtcNow;

            foreach (var watchdog in Watchdogs)
            {
                if (watchdog is null)
                    continue;

                if (watchdog.InvokeAtStart == true)
                {
                    watchdog.NextTick = default;
                }
                else
                {
                    watchdog.NextTick = now + watchdog.Period;
                }
            }

            ((IAINode)this).State = AINodeState.Started;
        }

        void IAINode.Stop()
        {
            ((IAINode)this).State = AINodeState.Failed;
            CurrentState?.Stop();
        }
    }
}
