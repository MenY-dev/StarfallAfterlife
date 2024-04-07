using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public class UAsset : IFArchiveSerializable
    {
        public long HeaderOffset { get; set; }

        public uint Tag { get; set; }

        public int Version { get; set; }

        public int HeadersSize { get; set; }

        public string Group { get; set; }

        public int Flags { get; set; }

        public int NamesCount { get; set; }

        public int NamesOffset { get; set; }

        public int GatherableTextDataCount { get; set; }

        public int GatherableTextDataOffset { get; set; }

        public int ExportsCount { get; set; }

        public int ExportsOffset { get; set; }

        public int ImportsCount { get; set; }

        public int ImportsOffset { get; set; }

        public int DependsOffset { get; set; }

        public Guid Guid { get; set; }

        public List<string> Names { get; set; }

        public List<FObjectExport> Exports { get; set; }

        public List<FObjectImport> Imports { get; set; }

        public List<int[]> Depends { get; set; }

        public List<UObject> Objects { get; set; }


        public void Deserialize(FArchive archive)
        {
            HeaderOffset = archive.Position;
            Tag = archive.ReadUInt32();
            Version = archive.ReadInt32();

            if (Tag != UEConstants.PackChunkTag && Tag != UEConstants.PackChunkTagReversed ||
                Version != -7)
                return;

            archive.Skip(16);

            HeadersSize = archive.ReadInt32();
            Group = archive.ReadString();
            Flags = archive.ReadInt32();
            NamesCount = archive.ReadInt32();
            NamesOffset = archive.ReadInt32();
            GatherableTextDataCount = archive.ReadInt32();
            GatherableTextDataOffset = archive.ReadInt32();
            ExportsCount = archive.ReadInt32();
            ExportsOffset = archive.ReadInt32();
            ImportsCount = archive.ReadInt32();
            ImportsOffset = archive.ReadInt32();
            DependsOffset = archive.ReadInt32();

            archive.Skip(16);

            Guid = new Guid(archive.ReadBytes(16));

            var generations = archive.ReadArray(r => (ExportCount: r.ReadInt32(), NameCount: r.ReadInt32()));

            (short Major, short Minor, short Patch, int Changelist, string Branch) ReadEngineVersion() =>
                (archive.ReadInt16(), archive.ReadInt16(), archive.ReadInt16(), archive.ReadInt32(), archive.ReadString());

            var engineVersion = ReadEngineVersion();
            var compatibleVersion = ReadEngineVersion();

            var compressionFlags = archive.ReadUInt32();
            var compressedChunks = archive.ReadArray(r =>
            (
                UncompressedOffset: r.ReadInt32(),
                UncompressedSize: r.ReadInt32(),
                CompressedOffset: r.ReadInt32(),
                CompressedSize: r.ReadInt32()
            ));

            var packageSource = archive.ReadUInt32();
            var additionalPackagesToCook = archive.ReadArray(r => r.ReadString());
            var assetRegistryDataOffset = archive.ReadInt32();
            var bulkDataStartOffset = archive.ReadInt64();
            var worldTileInfoDataOffset = archive.ReadInt32();
            var chunkIDs = archive.ReadArray(r => r.ReadInt32());
            var preloadDependencyCount = archive.ReadInt32();
            var preloadDependencyOffset = archive.ReadInt32();

            archive.Seek(HeaderOffset + NamesOffset, SeekOrigin.Begin);
            Names = new(NamesCount);

            for (int i = 0; i < NamesCount; i++)
            {
                Names.Add(archive.ReadString());
                archive.Skip(4);
            }

            archive.Seek(HeaderOffset + ImportsOffset, SeekOrigin.Begin);
            Imports = new(ImportsCount);

            for (int i = 0; i < ImportsCount; i++)
            {
                var import = archive.Read<FObjectImport>();
                import.Index = i;
                Imports.Add(import);
            }

            archive.Seek(HeaderOffset + ExportsOffset, SeekOrigin.Begin);
            Exports = new(ExportsCount);

            for (int i = 0; i < ExportsCount; i++)
            {
                var export = archive.Read<FObjectExport>();
                export.Index = i;
                export.SerialOffset -= HeadersSize;
                Exports.Add(export);
            }

            archive.Seek(HeaderOffset + DependsOffset, SeekOrigin.Begin);
            var dependsCount = archive.ReadInt32();
            Depends = new();

            for (int i = 0; i < dependsCount; i++)
            {
                var count = archive.ReadInt32();

                if (count < 0)
                    continue;

                var objDepends = new int[count];

                for (int j = 0; j < count; j++)
                    objDepends[j]= archive.ReadInt32();

                Depends.Add(objDepends);
            }

        }

        public void LoadObjectsData(FArchive uexp, UAssetLoadContext context = default)
        {
            var startPos = uexp.Position;
            Objects = new();

            foreach (var export in Exports ?? new())
            {
                var objectName = GetName(export.ObjectName);
                var endPos = startPos + export.SerialOffset + export.SerialSize;

                if (startPos > endPos - 8 ||
                    objectName is null)
                    continue;

                var obj = new UObject() { Name = objectName };
                var objectContext = new UAssetLoadContext
                {
                    Asset = this,
                    Reader = uexp,
                    Size = export.SerialSize,
                    StartOffset = startPos + export.SerialOffset,
                    Converters = context.Converters
                };

                uexp.Seek(objectContext.StartOffset, SeekOrigin.Begin);
                obj.Load(objectContext);
                Objects.Add(obj);
            }
        }

        public string GetName(FName? name) =>
            name is null ? null : GetName(name.Value.Index);

        public string GetName(int index)
        {
            if (Names is null ||
                index < 0 ||
                index >= Names.Count)
                return null;

            return Names[index];
        }

        public string GetClassName(int index)
        {
            if (index < 0)
                return GetName(GetImport(-index - 1)?.ClassName);
            else if (index > 0)
                return GetName(GetExport(index - 1)?.ObjectName);
            
            return null;
        }

        public FObjectExport? GetExport(int index) =>
            Exports?.ElementAtOrDefault(index) ?? default;

        public FObjectExport? GetExport(UObject obj) =>
            Exports?.ElementAtOrDefault(Objects?.IndexOf(obj) ?? -1) ?? default;

        public FObjectImport? GetImport(int index) =>
            Imports?.ElementAtOrDefault(index) ?? default;

        public UObject GetObject(int index) =>
            Objects?.ElementAtOrDefault(index) ?? default;

        public UObject GetObject(FObjectExport export) =>
            Objects?.ElementAtOrDefault(export.Index) ?? default;
    }
}
