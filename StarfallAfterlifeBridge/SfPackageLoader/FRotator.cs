using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FRotator : IFArchiveSerializable
    {
        public float Pitch { get; set; }

        public float Yaw { get; set; }

        public float Roll { get; set; }

        public void Deserialize(FArchive archive)
        {
            Pitch = archive.ReadSingle();
            Yaw = archive.ReadSingle();
            Roll = archive.ReadSingle();
        }

        public override string ToString() => $"(p:{Pitch}, y:{Yaw}, r:{Roll})";
    }
}
