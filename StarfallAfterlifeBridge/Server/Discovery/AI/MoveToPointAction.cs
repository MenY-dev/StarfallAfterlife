using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class MoveToPointAction : AIAction
    {
        public Vector2 Point { get; }

        public MoveToPointAction(Vector2 point)
        {
            Point = point;
        }

        public override void Start()
        {
            base.Start();
            Fleet?.MoveTo(Point);
        }

        public override void Update()
        {
            base.Update();

            if (Fleet?.Location.GetDistanceTo(Point) is null or < 0.01f)
            {
                State = AINodeState.Completed;
            }
        }
    }
}
