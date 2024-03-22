using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FVector : IFArchiveSerializable
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public void Deserialize(FArchive archive)
        {
            X = archive.ReadSingle();
            Y = archive.ReadSingle();
            Z = archive.ReadSingle();
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
