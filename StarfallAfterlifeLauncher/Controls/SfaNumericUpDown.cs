using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class SfaNumericUpDown : NumericUpDown
    {
        protected override Type StyleKeyOverride => typeof(SfaNumericUpDown);
    }
}
