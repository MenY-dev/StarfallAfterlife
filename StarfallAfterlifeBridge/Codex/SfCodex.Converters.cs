using StarfallAfterlife.Bridge.SfPackageLoader.SfTypes;
using StarfallAfterlife.Bridge.SfPackageLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.SfPackageLoader.FileSysten;

namespace StarfallAfterlife.Bridge.Codex
{
    public partial class SfCodex
    {
        public record struct UPropertyConverterContext(
            SfCodex Codex, UObject Obj, UAsset Asset, UEFileSystem FileSystem);

        public static object ConvertDropItemsOnDisassemble(UProperty prop, UPropertyConverterContext context)
        {
            if (prop.Value is UProperty[] itemsProps)
                return itemsProps
                    .Where(i => i.Value is DropItemOnDisassembly)
                    .Select(i => (DropItemOnDisassembly)i.Value)
                    .Select(i =>
                    {
                        if (i.Item is string item &&
                            item.Split('.')?.LastOrDefault() is string name &&
                            context.Codex?.ClassToId(name) is int id)
                            return new SfCodexTypes.ItemDropInfo() { Item = id, Min = i.Min, Max = i.Max };

                        return default;
                    })
                    .Where(i => i.Item != 0)
                    .ToArray();

            return Array.Empty<SfCodexTypes.ItemDropInfo>();
        }

        public static object ConvertRequiredItemCrafting(UProperty prop, UPropertyConverterContext context)
        {
            if (prop.Value is UProperty[] itemsProps)
                return itemsProps
                    .Where(i => i.Value is RequiredItemCrafting)
                    .Select(i => (RequiredItemCrafting)i.Value)
                    .Select(i =>
                    {
                        if (i?.Item is string item &&
                            item.Split('.')?.LastOrDefault() is string name &&
                            context.Codex?.ClassToId(name) is int id)
                            return new SfCodexTypes.ItemCountInfo() { Item = id, Count = i.Count, };

                        return default;
                    })
                    .Where(i => i.Item != 0)
                    .ToArray();

            return Array.Empty<SfCodexTypes.ItemCountInfo>();
        }

        public static object ConvertQualityData(UProperty prop, UPropertyConverterContext context)
        {
            var items = new Dictionary<string, int>();

            if (prop.Value is List<KeyValuePair<UProperty, UProperty>> itemsProps)
            {
                foreach (var item in itemsProps)
                {
                    if (item.Key.Value is string key &&
                        item.Value.Value is string value &&
                        value.Split('.')?.LastOrDefault() is string name &&
                        context.Codex?.ClassToId(name) is int id)
                        items[key] = id;
                }
            }

            return items;
        }

        public static object ConvertObjectIndexToName(UProperty prop, UPropertyConverterContext context)
        {
            if (prop.Value is int index &&
                index != 0)
            {
                return context.Asset?.GetClassName(index);
            }

            return null;
        }

        public static object ConvertToTextKey(UProperty prop, UPropertyConverterContext context)
        {
            if (prop.Value is FText text)
                return new SfCodexTextKey()
                {
                    Key = text.Key,
                    Namespace = string.IsNullOrEmpty(text.Namespace) ? null : text.Namespace,
                };

            return default;
        }

        public static object ConvertPathToClassName(UProperty prop, UPropertyConverterContext context)
        {
            if (prop.Value is string path)
                return path.Split('.').LastOrDefault();

            return default;
        }

        public static object ConvertPathToAssetId(UProperty prop, UPropertyConverterContext context)
        {
            if (prop.Value is string path &&
                path.Split('.').LastOrDefault() is string name &&
                context.Codex?.ClassToId(name) is int id)
                return id;

            return default;
        }
    }
}
