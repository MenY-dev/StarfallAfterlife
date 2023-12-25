using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.IO
{
    public static class FileHelpers
    {
        public static string ReplaceInvalidFileNameChars(string name, char newChar)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, newChar);

            return name;
        }

        public static DirectoryInfo CreateUniqueDirectory(string root, string baseName = "dir")
        {
            try
            {
                if (Directory.Exists(root) == false)
                    Directory.CreateDirectory(root);

                var dirs = Directory.GetDirectories(root);
                var newDir = Path.Combine(root, baseName);

                if (Directory.Exists(newDir) == false)
                    return Directory.CreateDirectory(newDir);

                for (int i = 1; i < dirs.Length + 2; i++)
                {
                    newDir = Path.Combine(root, string.Format("{0}({1})", baseName, i));

                    if (Directory.Exists(newDir) == false)
                        return Directory.CreateDirectory(newDir);
                }
            }
            catch { }

            return null;
        }
    }
}
