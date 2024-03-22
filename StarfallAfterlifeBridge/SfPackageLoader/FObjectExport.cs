using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct FObjectExport : IFArchiveSerializable
    {
        public int Index { get; set; }
        public int ObjectIndex { get; set; }
        public int SuperIndex { get; set; }
        public int TemplateIndex { get; set; }
        public int PackageIndex { get; set; }
        public FName ObjectName { get; set; }
        public EObjectFlags ObjectFlags { get; set; }
        public long SerialSize { get; set; }
        public long SerialOffset { get; set; }
        public bool ForcedExport { get; set; }
        public bool NotForClient { get; set; }
        public bool NotForServer { get; set; }
        public Guid Guid { get; set; }
        public int PackageFlags { get; set; }
        public bool NotForEditor { get; set; }
        public bool IsAsset { get; set; }
        public int FirstExportDependency { get; set; }
        public int SerializationBeforeSerializationDependencies { get; set; }
        public int CreateBeforeSerializationDependencies { get; set; }
        public int SerializationBeforeCreateDependencies { get; set; }
        public int CreateBeforeCreateDependencies { get; set; }

        public void Deserialize(FArchive archive)
        {
            ObjectIndex = archive.ReadInt32();
            SuperIndex = archive.ReadInt32();
            TemplateIndex = archive.ReadInt32();
            PackageIndex = archive.ReadInt32();
            ObjectName = archive.ReadName();
            ObjectFlags = (EObjectFlags)archive.ReadInt32();
            SerialSize = archive.ReadInt64();
            SerialOffset = archive.ReadInt64();
            ForcedExport = archive.ReadUInt32() != 0;
            NotForClient = archive.ReadUInt32() != 0;
            NotForServer = archive.ReadUInt32() != 0;
            Guid = new(archive.ReadBytes(16));
            PackageFlags = archive.ReadInt32();
            NotForEditor = archive.ReadUInt32() != 0;
            IsAsset = archive.ReadUInt32() != 0;
            FirstExportDependency = archive.ReadInt32();
            SerializationBeforeSerializationDependencies = archive.ReadInt32();
            CreateBeforeSerializationDependencies = archive.ReadInt32();
            SerializationBeforeCreateDependencies = archive.ReadInt32();
            CreateBeforeCreateDependencies = archive.ReadInt32();
        }
    }
}
