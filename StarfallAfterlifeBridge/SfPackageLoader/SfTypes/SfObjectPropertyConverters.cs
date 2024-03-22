using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader;

namespace StarfallAfterlife.Bridge.SfPackageLoader.SfTypes
{
    public static class SfObjectPropertyConverters
    {
        public static List<IUObjectPropertyCustomConverter> Converters { get; } = new()
        {
            new GameplayTagContainerConverter(),
            new DropItemOnDisassembleConverter(),
            new RequiredItemCraftingConverter(),
        };

        public class GameplayTagContainerConverter : IUObjectPropertyCustomConverter
        {
            public bool CanConvert(FPropertyTag tag) => tag.IsType("StructProperty") && tag.IsStructName("GameplayTagContainer");

            public bool TryRead(UAssetLoadContext context, FPropertyTag tag, ref UProperty property)
            {
                var uexp = context.Reader;
                var gameplayTagsCount = uexp.ReadInt32();

                if (gameplayTagsCount < 0)
                    return false;

                var gameplayTags = new string[gameplayTagsCount];

                for (int i = 0; i < gameplayTagsCount; i++)
                    gameplayTags[i] = context.GetName(uexp.ReadName());

                property.Value = gameplayTags;
                return true;
            }
        }

        public class DropItemOnDisassembleConverter : IUObjectPropertyCustomConverter
        {
            public bool CanConvert(FPropertyTag tag) => tag.IsType("StructProperty") && tag.IsStructName("DropItemOnDisassemble");

            public bool TryRead(UAssetLoadContext context, FPropertyTag tag, ref UProperty property)
            {
                var item = (string)UObjectBase.ReadProperty(context);
                var obj = (int?)UObjectBase.ReadProperty(context) ?? 0;
                var min = (int?)UObjectBase.ReadProperty(context) ?? 0;
                var max = (int?)UObjectBase.ReadProperty(context) ?? 0;

                property.Value = new DropItemOnDisassembly(item, min, max);
                return true;
            }
        }

        public class RequiredItemCraftingConverter : IUObjectPropertyCustomConverter
        {
            public bool CanConvert(FPropertyTag tag) => tag.IsType("StructProperty") && tag.IsStructName("RequiredItemCrafting");

            public bool TryRead(UAssetLoadContext context, FPropertyTag tag, ref UProperty property)
            {
                var item = (string)UObjectBase.ReadProperty(context);
                var count = (int?)UObjectBase.ReadProperty(context) ?? 0;

                property.Value = new RequiredItemCrafting(item, count);
                return true;
            }
        }
    }
}
