using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader;

namespace StarfallAfterlife.Bridge.SfPackageLoader.FileSysten
{
    public class UEFSArchive : FArchive, IDisposable
    {
        public UEPackChunkInfo Info { get; set; }

        public UEFSArchive(string path) : base(path)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            Info = default;
        }
    }
}
