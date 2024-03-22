using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public class FTextLocalizationResource : IFArchiveSerializable
    {
        public enum LocResVersion : byte
        {
            Legacy = 0,
            Compact = 1,
            Optimized = 2,
        };

        public LocResVersion Version { get; set; } = 0;

        public Dictionary<string, Dictionary<string, string>> Namespaces { get; } = new();

        public void Deserialize(FArchive archive)
        {
            var startPos = archive.Position;
            string[] strings = null;

            if (new Guid(archive.ReadBytes(16)) != UEConstants.LocResMagic)
                return;

            Version = (LocResVersion)archive.ReadByte();

            if (Version >= LocResVersion.Compact)
            {
                var stringArrayOffset = archive.ReadInt64();

                if (Version >= LocResVersion.Optimized)
                {

                    var tmpOffset = archive.Position;
                    archive.Seek(startPos + stringArrayOffset, SeekOrigin.Begin);
                    int count = archive.ReadInt32();
                    strings = new string[count];

                    for (int i = 0; i < count; i++)
                    {
                        strings[i] = archive.ReadString();
                        archive.Skip(4);
                    }

                    archive.Seek(tmpOffset, SeekOrigin.Begin);
                }
            }

            if (strings is null)
                return;

            if (Version >= LocResVersion.Optimized)
            {
                archive.Skip(4);
            }

            int namespaceCount = archive.ReadInt32();

            for (int i = 0; i < namespaceCount; i++)
            {
                if (Version >= LocResVersion.Optimized)
                    archive.Skip(4);

                var namespaceStrings = new Dictionary<string, string>();
                var namespaceKey = archive.ReadString();
                int keyCount = archive.ReadInt32();

                for (int n = 0; n < keyCount; n++)
                {
                    if (Version >= LocResVersion.Optimized)
                        archive.Skip(4);

                    var key = archive.ReadString();
                    var hash = archive.ReadInt32();

                    if (Version >= LocResVersion.Compact)
                    {
                        var index = archive.ReadInt32();

                        if (index > -1 && index < strings.Length)
                        {
                            namespaceStrings[key] = strings[index];
                        }
                    }
                }

                if (namespaceStrings.Count > 0)
                    Namespaces[namespaceKey] = namespaceStrings;
            }
        }
    }
}
