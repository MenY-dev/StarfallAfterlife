using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum DropTreeNodeType : byte
    {
        Or = 0,
        And = 1,
        Item = 2,
        None = 254,
    }
}
