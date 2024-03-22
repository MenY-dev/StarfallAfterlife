using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader.FileSysten;
using StarfallAfterlife.Bridge.SfPackageLoader;
using System.IO;

namespace StarfallAfterlife.Bridge.SfPackageLoader.FileSysten
{
    public struct UEFSFileInfo : IFArchiveSerializable
    {
        public string Path { get; set; }

        public string PackPath { get; set; }

        public long Offset { get; set; }

        public long Size { get; set; }

        public long FullSize { get; set; }

        public byte[] Hash { get; set; }

        public void Deserialize(FArchive archive)
        {
            Path = archive.ReadString();
            Offset = archive.ReadInt64();
            Size = archive.ReadInt64();
            FullSize = archive.ReadInt64();

            archive.Skip(4);

            Hash = archive.ReadBytes(20);

            archive.Skip(5);
        }

        public UEFSArchive Open()
        {
            var archive = new UEFSArchive(PackPath);
            var info = new UEPackChunkInfo();

            archive.Seek(Offset);
            archive.Read(ref info);
            archive.Info = info;
            archive.Seek(info.DataOffset, SeekOrigin.Begin);

            return archive;
        }
    }
}
