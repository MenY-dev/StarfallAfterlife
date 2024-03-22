using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public interface IUObjectPropertyCustomConverter
    {
        public abstract bool CanConvert(FPropertyTag tag);

        public abstract bool TryRead(UAssetLoadContext context, FPropertyTag tag, ref UProperty property);
    }
}
