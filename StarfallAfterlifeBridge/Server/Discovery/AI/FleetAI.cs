﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class FleetAI
    {
        public virtual bool IsConnected { get; set; } = false;

        public DiscoveryFleet Fleet { get; private set; }

        public StarSystem System => Fleet.System;

        protected IAINode CurrentAction { get; set; }

        public virtual void Update()
        {
            if (CurrentAction is IAINode action)
            {
                if (action.State is not AINodeState.Started)
                    StopCurrentAction();
                else
                    action.Update();
            }
        }

        public virtual void StartAction(IAINode action)
        {
            StopCurrentAction();

            if (action is null)
                return;

            action.AI = this;
            action.Start();
            CurrentAction = action;
        }

        public virtual void StopCurrentAction()
        {
            var action = CurrentAction;
            CurrentAction = null;

            if (action is not null)
            {
                action.Stop();
                OnActionFinished(action);
            }
        }

        protected virtual void OnActionFinished(IAINode action)
        {

        }

        public virtual void Connect(DiscoveryFleet fleet)
        {
            Fleet = fleet;
            IsConnected = true;
        }

        public virtual void Disconnect()
        {
            Fleet = null;
            IsConnected = false;
            StopCurrentAction();
        }
    }
}
