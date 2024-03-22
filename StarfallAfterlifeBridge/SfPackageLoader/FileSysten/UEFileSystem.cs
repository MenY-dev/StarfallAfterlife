using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader;

namespace StarfallAfterlife.Bridge.SfPackageLoader.FileSysten
{
    public class UEFileSystem : UEFSDirectory
    {
        private Dictionary<string, UEPackInfo> _loadedPacks = new();

        public void LoadPack(string path)
        {
            var archive = FArchive.Open(path);
            var packInfo = new UEPackInfo();

            archive.Seek(-UEConstants.PackFooterOffset, SeekOrigin.End);
            archive.Read(ref packInfo);

            if (packInfo.Version != 8)
                return;

            // Load files table

            archive.Seek(packInfo.FilesTableOffset, SeekOrigin.Begin);

            const string rootStartPath = "../../..";
            string filesRoot = archive.ReadString();

            if (string.IsNullOrEmpty(filesRoot) == true ||
                filesRoot.StartsWith(rootStartPath, StringComparison.OrdinalIgnoreCase) == false)
                return;

            filesRoot = filesRoot.Substring(rootStartPath.Length);
            var filesCount = archive.ReadInt32();

            if (filesCount < 1)
                return;

            _loadedPacks[path] = packInfo;

            for (int i = 0; i < filesCount; i++)
            {
                var fileInfo = new UEFSFileInfo()
                {
                    PackPath = path
                };

                archive.Read(ref fileInfo);

                if (fileInfo.Size < 1 ||
                    fileInfo.Offset < 0 ||
                    archive.Length <= fileInfo.Offset + fileInfo.Size ||
                    string.IsNullOrWhiteSpace(fileInfo.Path))
                    continue;

                fileInfo.Path = filesRoot + fileInfo.Path;
                AddFile(fileInfo);
            }
        }

    }
}
