using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization
{
    [JsonConverter(typeof(SValueConverter))]
    public class SValue
    {
        [JsonPropertyName("$")]
        public object Value { get; set; }

        public SValue(object value)
        {
            Value = value;
        }

        public static JsonObject Create(string value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(bool value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(byte value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(short value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(ushort value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(int value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(uint value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(long value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(ulong value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(float value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(double value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(decimal value) => new JsonObject() { ["$"] = value };
        public static JsonObject Create(Guid value) => new JsonObject() { ["$"] = value };

        public class SValueConverter : JsonConverter<object>
        {
            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return null;
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$");

                if (value is null) writer.WriteNullValue();
                else if (value is JsonNode node) node.WriteTo(writer);
                else if (value is string) writer.WriteStringValue((string)value);
                else if (value is bool) writer.WriteBooleanValue((bool)value);
                else if (value is int) writer.WriteNumberValue((int)value);
                else if (value is byte) writer.WriteNumberValue((byte)value);
                else if (value is float) writer.WriteNumberValue((float)value);
                else if (value is double) writer.WriteNumberValue((double)value);
                else if (value is sbyte) writer.WriteNumberValue((sbyte)value);
                else if (value is short) writer.WriteNumberValue((short)value);
                else if (value is ushort) writer.WriteNumberValue((ushort)value);
                else if (value is uint) writer.WriteNumberValue((uint)value);
                else if (value is long) writer.WriteNumberValue((long)value);
                else if (value is ulong) writer.WriteNumberValue((ulong)value);
                else if (value is decimal) writer.WriteNumberValue((decimal)value);
                else if (value is Guid) writer.WriteStringValue((Guid)value);
                else if (value is DateTime) writer.WriteStringValue((DateTime)value);
                else if (value is DateTimeOffset) writer.WriteStringValue((DateTimeOffset)value);
                else JsonSerializer.Serialize(writer, value, options);

                writer.WriteEndObject();
            }
        }
    }
}
