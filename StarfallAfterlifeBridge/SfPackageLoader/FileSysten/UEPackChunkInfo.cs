using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader;

namespace StarfallAfterlife.Bridge.SfPackageLoader.FileSysten
{
    public struct UEPackChunkInfo : IFArchiveSerializable
    {
        public const int ChunkInfoSize = 53;

        public long Offset { get; private set; }

        public long Pos { get; private set; }

        public long DataOffset { get; private set; }

        public long Size { get; private set; }

        public long FullSize { get; private set; }

        public int CompressionType { get; private set; }

        public byte[] Hash { get; private set; }

        public void Deserialize(FArchive archive)
        {
            Offset = archive.Position;
            Pos = archive.ReadInt64();
            Size = archive.ReadInt64();
            FullSize = archive.ReadInt64();
            CompressionType = archive.ReadInt32();
            Hash = new byte[20];
            archive.Read(Hash);
            archive.Skip(5);
            DataOffset = archive.Position;
        }
    }
}
