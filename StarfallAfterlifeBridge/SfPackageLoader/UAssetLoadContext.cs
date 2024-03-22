using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct UAssetLoadContext
    {
        public UAsset Asset { get; set; }

        public FArchive Reader { get; set; }

        public long StartOffset { get; set; }

        public long Size { get; set; }

        public List<IUObjectPropertyCustomConverter> Converters { get; set; }

        public string GetName(FName? name) =>
            name is null ? null : GetName(name.Value.Index);

        public string GetName(int index)
        {
            if (Asset?.Names is null ||
                index < 0 ||
                index >= Asset.Names.Count)
                return null;

            return Asset.Names[index];
        }
    }
}
