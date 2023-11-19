using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class AttackAction : AIAction
    {
        public StarSystemObject Target { get; }

        public TimeSpan AttackDuration { get; }

        public int TargetLostDistance { get; }

        public AttackAction(StarSystemObject target, TimeSpan duration = default, int targetLostDistance = 0)
        {
            Target = target;
            AttackDuration = duration;
            TargetLostDistance = targetLostDistance;
        }

        public override void Start()
        {
            base.Start();

            if (Target is StarSystemObject target &&
                Fleet is DiscoveryFleet fleet)
            {
                fleet.SetAttackTarget(target);
            }
        }

        public override void Update()
        {
            base.Update();

            if (Fleet is null || Target is null ||
                (AttackDuration != default && TotalTime > AttackDuration) ||
                (TargetLostDistance > 0 && Fleet.Hex.GetDistanceTo(Target.Hex) > TargetLostDistance) ||
                (Target is DiscoveryFleet targetFleet && Fleet.CanAttack(targetFleet) == false))
            {
                State = AIActionState.Failed;
                Fleet.Stop();
            }
        }

        public override void Stop()
        {
            base.Stop();
            Fleet?.Stop();
        }
    }
}
