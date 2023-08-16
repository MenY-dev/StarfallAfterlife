using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization.Json
{
    public class SValue : JsonObject
    {
        public SValue(JsonNode node) : base()
        {
            Add("$", node);
        }
    }

    public class SValueAttribute : JsonConverterAttribute
    {
        public SValueAttribute() : base(typeof(SValueConverter)) { }
    }

    public class SValueConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            string name = "$";

            writer.WriteStartObject();

            if (value is null) writer.WriteNull(name);
            else if (value is string) writer.WriteString(name, (string)value);
            else if (value is char) writer.WriteString(name, (string)value);
            else if (value is bool) writer.WriteBoolean(name, (bool)value);
            else if (value is short) writer.WriteNumber(name, (short)value);
            else if (value is int) writer.WriteNumber(name, (int)value);
            else if (value is long) writer.WriteNumber(name, (long)value);
            else if (value is ushort) writer.WriteNumber(name, (ushort)value);
            else if (value is uint) writer.WriteNumber(name, (uint)value);
            else if (value is ulong) writer.WriteNumber(name, (ulong)value);
            else if (value is float) writer.WriteNumber(name, (float)value);
            else if (value is double) writer.WriteNumber(name, (double)value);
            else if (value is decimal) writer.WriteNumber(name, (decimal)value);

            writer.WriteEndObject();
        }

        public override bool CanConvert(Type typeToConvert) => true;
    }
}
