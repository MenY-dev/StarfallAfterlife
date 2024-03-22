using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader.SfTypes
{
    public readonly record struct DropItemOnDisassembly(string Item, int Min, int Max);
}
