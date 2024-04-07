using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    [Flags]
    public enum SfCodexPropertyFlags : int
    {
        None = 0,
        Internal = 1 << 0,
        Trade = 1 << 1,
        Production = 1 << 2,
        Disassembly = 1 << 3,

        MainInfo = 1 << 16,
        SecondaryInfo = 1 << 17,
        AdditionalInfo = 1 << 18,
    }
}
