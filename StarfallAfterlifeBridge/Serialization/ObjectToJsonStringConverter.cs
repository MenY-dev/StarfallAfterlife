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
    public class ObjectToJsonStringConverter<T> : JsonConverter<T> where T : class
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(string))
                return false;

            return base.CanConvert(typeToConvert);
        }
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = JsonSerializer.Deserialize<string>(ref reader, options);
            return JsonHelpers.DeserializeUnbuffered<T>(text, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var text = JsonHelpers.SerializeUnbuffered(value, options);
            JsonSerializer.Serialize(writer, text, options);
        }
    }
}
