using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public static class UEConstants
    {
        public const uint PackChunkTag = 0x9E2A83C1;

        public const uint PackChunkTagReversed = 0xC1832A9E;

        public const uint PackFooterTag = 0x5A6F12E1;

        public const uint PackFooterTagReversed = 0xE1126F5A;

        public const long PackFooterOffset = 0xCC;

        public static readonly Guid LocResMagic = new Guid("7574140e4a67fc034a15909dc3377f1b");
    }
}
