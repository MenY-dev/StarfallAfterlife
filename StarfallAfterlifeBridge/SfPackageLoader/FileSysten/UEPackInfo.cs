using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader;

namespace StarfallAfterlife.Bridge.SfPackageLoader.FileSysten
{
    public struct UEPackInfo : IFArchiveSerializable
    {
        public string Path { get; set; }

        public int Version { get; set; }

        public long FilesTableOffset { get; set; }

        public long FilesTableSize { get; set; }

        public byte[] Hash { get; set; }

        public const int DataSize = 44;

        public void Deserialize(FArchive archive)
        {
            if (archive.Position + DataSize >= archive.Length)
                return;

            var magic = archive.ReadUInt32();

            if (magic != UEConstants.PackFooterTag &&
                magic != UEConstants.PackFooterTagReversed)
                return;

            Version = archive.ReadInt32();

            if (Version != 8)
                return;

            FilesTableOffset = archive.ReadInt64();
            FilesTableSize = archive.ReadInt64();

            Hash = new byte[20];
            archive.Read(Hash);
        }
    }
}
