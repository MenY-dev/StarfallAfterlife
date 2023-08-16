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
        public static DirectoryInfo CreateUniqueDirectory(string root, string format = "{0}")
        {
            try
            {
                if (Directory.Exists(root) == false)
                    Directory.CreateDirectory(root);

                var dirs = Directory.GetDirectories(root);

                for (int i = 0; i < dirs.Length + 1; i++)
                {
                    var newDir = Path.Combine(root, string.Format(format, i));

                    if (Directory.Exists(newDir) == false)
                        return Directory.CreateDirectory(newDir);
                }
            }
            catch { }

            return null;
        }
    }
}
