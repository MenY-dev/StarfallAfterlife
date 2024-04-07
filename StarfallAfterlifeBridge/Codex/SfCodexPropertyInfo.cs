using StarfallAfterlife.Bridge.SfPackageLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public class SfCodexPropertyInfo
    {
        public string Name { get; private set; }

        public SfCodexTextKey DisplayName { get; private set; }

        public Type Type { get; private set; }

        public SfCodexPropertyFlags Flags { get; private set; } = SfCodexPropertyFlags.None;

        public Func<UProperty, SfCodex.UPropertyConverterContext, object> Converter { get; private set; }

        public Func<JsonNode, object> JsonGetter { get; private set; }

        private SfCodexPropertyInfo() { }

        public static SfCodexPropertyInfo Create<T>(
            string name, SfCodexTextKey displayName = default,
            Func<UProperty, SfCodex.UPropertyConverterContext, object> uPropConverter = null,
            SfCodexPropertyFlags flags = SfCodexPropertyFlags.None)
        {
            var prop = new SfCodexPropertyInfo
            {
                Name = name,
                DisplayName = displayName,
                Flags = flags,
                Type = typeof(T),
                Converter = uPropConverter,
                JsonGetter = n =>
                {
                    var type = typeof(T);

                    if (n is null)
                        return default;

                    if (n is JsonValue)
                    {
                        try
                        {
                            if (Nullable.GetUnderlyingType(type) is Type nullableType)
                                type = nullableType;

                            if (type.IsPrimitive ||
                                type.Equals(typeof(Guid)) ||
                                type.Equals(typeof(string)) ||
                                type.Equals(typeof(DateTime)) ||
                                type.Equals(typeof(TimeSpan)) ||
                                type.Equals(typeof(decimal)))
                                return n.GetValue<T>();
                        }
                        catch { }
                    }

                    try
                    {
                        return n.Deserialize<T>();
                    }
                    catch { }

                    return default(T);
                },
            };

            return prop;
        }

        public object GetValue(SfCodexItem item) => 
            item?.Fields?.TryGetValue(Name, out var value) == true ? value : null;

        public T GetValue<T>(SfCodexItem item, T defaultValue = default) =>
            TryGetValue<T>(item, out var value) == true ? value : defaultValue;

        public bool TryGetValue<T>(SfCodexItem item, out T value)
        {
            object obj = null;

            if (item?.Fields?.TryGetValue(Name, out obj) != true)
            {
                value = default;
                return false;
            }

            try
            {
                value = (T)obj;
                return true;
            }
            catch { }

            try
            {
                value = (T)Convert.ChangeType(obj, typeof(T));
                return true;
            }
            catch { }

            value = default;
            return false;
        }

        public object GetValue(JsonNode node)
        {
            try
            {
                return JsonGetter?.Invoke(node) ?? Activator.CreateInstance(Type);
            }
            catch { }

            return null;
        }
    }
}
