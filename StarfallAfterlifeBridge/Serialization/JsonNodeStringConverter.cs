using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization
{
    public class JsonNodeStringConverter : JsonConverter<JsonNode>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(JsonNode).IsAssignableFrom(typeToConvert);
        }

        public override JsonNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonHelpers.ParseNodeUnbuffered(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, JsonNode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToJsonString(options.WriteIndented));
        }
    }
}
