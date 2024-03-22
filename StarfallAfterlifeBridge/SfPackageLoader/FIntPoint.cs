using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FIntPoint : IFArchiveSerializable
    {
        public int X { get; set; }

        public int Y { get; set; }

        public void Deserialize(FArchive archive)
        {
            X = archive.ReadInt32();
            Y = archive.ReadInt32();
        }

        public override string ToString() => $"({X}, {Y})";
    }
}
