using Avalonia.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class BoolTransition : InterpolatingTransitionBase<bool>
    {
        protected override bool Interpolate(double progress, bool from, bool to)
        {
            if (from == to)
                return from;

            if (from == false)
                return progress > 0;

            return progress < 1;
        }
    }
}
