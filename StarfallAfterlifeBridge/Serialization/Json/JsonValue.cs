using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization.Json
{
    public class JsonValue : JsonNode
    {
        public virtual object Value { get; protected set; }

        public JsonValue(object value)
        {
            if (value is JsonNode)
                return;

            Value = value;
        }

        public static JsonValue Create<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is JsonElement element && element.ValueKind == JsonValueKind.Null)
                return null;

            return new JsonValue(value);
        }

        public override T GetValue<T>()
        {
            if (TryGetValue(out T value) == true)
                return value;

            return default!;
        }

        protected virtual bool TryGetValue<T>(out T result)
        {
            if (Value is T fastValue)
            {
                result = fastValue;
                return true;
            }

            if (Value is not JsonElement)
            {
                result = default;
                return false;
            }

            JsonElement element = (JsonElement)Value;
            bool success;

            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                    {
                        success = element.TryGetInt32(out int value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                    {
                        success = element.TryGetInt64(out long value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                    {
                        success = element.TryGetDouble(out double value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
                    {
                        success = element.TryGetInt16(out short value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                    {
                        success = element.TryGetDecimal(out decimal value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
                    {
                        success = element.TryGetByte(out byte value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
                    {
                        success = element.TryGetSingle(out float value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(uint) || typeof(T) == typeof(uint?))
                    {
                        success = element.TryGetUInt32(out uint value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?))
                    {
                        success = element.TryGetUInt16(out ushort value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?))
                    {
                        success = element.TryGetUInt64(out ulong value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(sbyte?))
                    {
                        success = element.TryGetSByte(out sbyte value);
                        result = (T)(object)value;
                        return success;
                    }
                    break;

                case JsonValueKind.String:
                    if (typeof(T) == typeof(string))
                    {
                        string strResult = element.GetString();
                        result = (T)(object)strResult;
                        return true;
                    }

                    if (typeof(T).BaseType == typeof(Enum) && typeof(T).IsEnum)
                    {
                        success = Enum.TryParse(typeof(T), element.GetString(), out object parseResult);
                        result = (T)parseResult;
                        return success;
                    }

                    if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                    {
                        success = element.TryGetDateTime(out DateTime value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
                    {
                        success = element.TryGetDateTimeOffset(out DateTimeOffset value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
                    {
                        success = element.TryGetGuid(out Guid value);
                        result = (T)(object)value;
                        return success;
                    }

                    if (typeof(T) == typeof(char) || typeof(T) == typeof(char?))
                    {
                        string str = element.GetString();

                        if (str.Length == 1)
                        {
                            result = (T)(object)str[0];
                            return true;
                        }
                    }
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
                    {
                        result = (T)(object)element.GetBoolean();
                        return true;
                    }
                    break;
            }

            result = default!;
            return false;
        }

        public override JsonNode Clone()
        {
            return new JsonValue(Value);
        }

        public override void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options = null)
        {
            if (writer is null)
                return;

            try
            {
                object val = Value;

                if (val is JsonElement element)
                    element.WriteTo(writer);
                else if (val is null)
                    writer.WriteNullValue();
                else if (val is string)
                    writer.WriteStringValue((string)val);
                else if (val is bool)
                    writer.WriteBooleanValue((bool)val);
                else if (val is int)
                    writer.WriteNumberValue((int)val);
                else if (val is byte)
                    writer.WriteNumberValue((byte)val);
                else if (val is float)
                    writer.WriteNumberValue((float)val);
                else if (val is double)
                    writer.WriteNumberValue((double)val);
                else if (val is sbyte)
                    writer.WriteNumberValue((sbyte)val);
                else if (val is short)
                    writer.WriteNumberValue((short)val);
                else if (val is ushort)
                    writer.WriteNumberValue((ushort)val);
                else if (val is uint)
                    writer.WriteNumberValue((uint)val);
                else if (val is long)
                    writer.WriteNumberValue((long)val);
                else if (val is ulong)
                    writer.WriteNumberValue((ulong)val);
                else if (val is decimal)
                    writer.WriteNumberValue((decimal)val);
                else if (val is Guid)
                    writer.WriteStringValue((Guid)val);
                else if (val is DateTime)
                    writer.WriteStringValue((DateTime)val);
                else if (val is DateTimeOffset)
                    writer.WriteStringValue((DateTimeOffset)val);
                else
                    JsonSerializer.Serialize(writer, Value, options);
            }
            catch (NotSupportedException)
            {
                writer.WriteNullValue();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            (Value as JsonNode)?.Dispose();
            Value = null;
        }
    }
}
