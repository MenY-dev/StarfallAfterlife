using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public interface IFArchiveSerializable
    {
        public void Deserialize(FArchive archive);
    }
}
