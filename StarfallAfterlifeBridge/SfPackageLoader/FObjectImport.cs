using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FObjectImport : IFArchiveSerializable
    {
        public int Index { get; set; }

        public FName ClassPackage { get; set; }

        public FName ClassName { get; set; }

        public int PackageIndex { get; set; }

        public FName ObjectName { get; set; }

        public void Deserialize(FArchive archive)
        {
            ClassPackage = archive.ReadName();
            ClassName = archive.ReadName();
            PackageIndex = archive.ReadInt32();
            ObjectName = archive.ReadName();
        }
    }
}
