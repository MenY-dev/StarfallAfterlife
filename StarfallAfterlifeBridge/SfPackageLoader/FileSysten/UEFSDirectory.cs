using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.SfPackageLoader.FileSysten
{
    public class UEFSDirectory
    {
        public string Name { get; set; }

        public UEFSDirectory Parent { get; set; }

        public Dictionary<string, UEFSDirectory> Children { get; protected set; }

        public Dictionary<string, UEFSFileInfo> Files { get; protected set; }

        private static readonly StringComparer _nameComparison = StringComparer.OrdinalIgnoreCase;

        public void AddFile(UEFSFileInfo fileInfo)
        {
            var path = fileInfo.Path?.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (path is null ||
                path.Length < 1)
                return;

            var fileName = path.Last();
            var folder = this;

            if (string.IsNullOrWhiteSpace(fileName))
                return;

            for (int i = 0; i < path.Length - 1; i++)
            {
                var folderName = path[i];

                if (string.IsNullOrWhiteSpace(folderName))
                    return;

                var nextFolder = folder.Children?.GetValueOrDefault(folderName);

                if (nextFolder is null)
                    nextFolder = folder.AddDirectoryInternal(folderName);

                folder = nextFolder;
            }

            folder.AddFileInternal(fileInfo, fileName);
        }

        public UEFSFileInfo? GetFile(string path)
        {
            var pathItems = path?.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathItems is null ||
                pathItems.Length < 0)
                return null;

            var fileName = pathItems.Last();
            var folder = this;

            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            for (int i = 0; i < pathItems.Length - 1; i++)
            {
                var folderName = pathItems[i];

                if (string.IsNullOrWhiteSpace(folderName))
                    return null;

                var nextFolder = folder.Children?.GetValueOrDefault(folderName);

                if (nextFolder is null)
                    return null;

                folder = nextFolder;
            }

            return folder?.Files?.TryGetValue(fileName, out var value) == true ? value : null;
        }

        public IEnumerable<UEFSFileInfo> GetFiles()
        {
            return (Files ??= new(_nameComparison)).Values;
        }

        public IEnumerable<UEFSFileInfo> GetFilesRecursively()
        {
            var stack = new Stack<UEFSDirectory>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                if (dir is null)
                    continue;

                foreach (var item in dir.GetDirectories())
                    stack.Push(item);

                foreach (var item in dir.GetFiles())
                    yield return item;
            }
        }

        public UEFSDirectory GetDirectory(string path)
        {
            var pathItems = path?.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathItems is null ||
                pathItems.Length < 0)
                return null;

            var folder = this;

            for (int i = 0; i < pathItems.Length; i++)
            {
                var folderName = pathItems[i];

                if (string.IsNullOrWhiteSpace(folderName))
                    return null;

                var nextFolder = folder.Children?.GetValueOrDefault(folderName);

                if (nextFolder is null)
                    return null;

                folder = nextFolder;
            }

            return folder;
        }

        public IEnumerable<UEFSDirectory> GetDirectories()
        {
            return Children?.Values ?? Enumerable.Empty<UEFSDirectory>();
        }

        protected void AddFileInternal(UEFSFileInfo fileInfo, string fileName)
        {
            if (fileName is null)
                return;

            (Files ??= new(_nameComparison))[fileName] = fileInfo;
        }

        protected UEFSDirectory AddDirectoryInternal(string name)
        {
            if (string.IsNullOrEmpty(name) == true)
                return null;

            var node = (Children ??= new(_nameComparison)).GetValueOrDefault(name);

            if (node is not null)
                return node;

            node = new UEFSDirectory()
            {
                Name = name,
                Parent = this,
            };

            Children[name] = node;
            return node;
        }
    }
}
