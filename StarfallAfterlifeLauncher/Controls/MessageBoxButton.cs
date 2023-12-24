using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    [Flags]
    public enum MessageBoxButton : int
    {
        Undefined = 0,
        Ok = 1,
        Cancell = 2,
        Yes = 4,
        No = 8,
        Delete = 16,
        Close = 32,
    }
}
