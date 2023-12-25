using StarfallAfterlife.Bridge.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization
{
    public class SfaObjectJsonConverter<T> : JsonConverter<T> where T : SfaObject, new()
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var node = JsonNode.Parse(ref reader, new JsonNodeOptions
            {
                PropertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive ?? false,
            });

            if (node is null)
                return null;

            var obj = new T();
            obj.LoadFromJson(node);
            return obj;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var node = value?.ToJson();

            if (node is null)
                writer.WriteNullValue();

            node.WriteTo(writer, options);
        }
    }
}
