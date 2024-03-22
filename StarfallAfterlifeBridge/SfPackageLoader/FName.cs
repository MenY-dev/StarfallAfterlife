using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FName : IFArchiveSerializable
    {
        public int Index { get; set; }

        public int Number { get; set; }

        public void Deserialize(FArchive archive)
        {
            Index = archive.ReadInt32();
            Number = archive.ReadInt32();
        }
    }
}
