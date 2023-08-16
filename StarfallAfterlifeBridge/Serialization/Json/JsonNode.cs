using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization.Json
{
    public abstract partial class JsonNode : IDisposable
    {
        protected virtual JsonElement Element { get; set; }


        public JsonNode this[string propertyName]
        {
            get => AsObject()?.GetItem(propertyName);
            set => AsObject()?.SetItem(propertyName, value);
        }

        public JsonNode this[int index]
        {
            get => AsArray()?.GetItem(index);
            set => AsArray()?.SetItem(index, value);
        }

        public JsonArray AsArray() => this as JsonArray;

        public JsonObject AsObject() => this as JsonObject;

        public JsonValue AsValue() => this as JsonValue;

        public virtual JsonNode Clone() => default;

        public virtual T GetValue<T>() => default;

        public static JsonNode Parse(object obj) => Parse(obj, null);

        public static JsonNode Parse(object obj, JsonSerializerOptions options)
        {
            try
            {
                return Parse(JsonDocument.Parse(JsonSerializer.Serialize(obj, options)).RootElement);
            }
            catch { }

            return null;
        }

        public static JsonNode Parse(string json, JsonDocumentOptions options = default)
        {
            try
            {
                return Parse(JsonDocument.Parse(json, options).RootElement);
            }
            catch
            {
            }

            return null;
        }

        public static JsonNode Parse(ref Utf8JsonReader reader)
        {

            try
            {
                return Parse(JsonDocument.ParseValue(ref reader).RootElement);
            }
            catch { }

            return null;
        }

        public static JsonNode Parse(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Null: return null;
                case JsonValueKind.Object: return new JsonObject(element);
                case JsonValueKind.Array: return new JsonArray(element);
            }

            return new JsonValue(element);
        }

        public virtual T Deserialize<T>(JsonSerializerOptions options = null)
        {
            options ??= new JsonSerializerOptions();

            try
            {
                if (AsValue() is JsonValue jsonValue && (string)jsonValue is string jsonString)
                    return JsonSerializer.Deserialize<T>(jsonString, options);
            }
            catch { }

            var writerOptions = new JsonWriterOptions
            {
                Encoder = options.Encoder,
                Indented = options.WriteIndented,
                MaxDepth = options.MaxDepth,
                SkipValidation = true
            };

            using var buffer = new PooledStream();
            using var writer = new Utf8JsonWriter((IBufferWriter<byte>)buffer, writerOptions);

            try
            {
                WriteTo(writer, options);
                writer.Flush();
                return JsonSerializer.Deserialize<T>(buffer.Span, options);
            }
            catch (Exception e)
            {
                SfaDebug.Print(e, "JsonNode");
            }

            return default;
        }

        public string ToJsonString(bool WriteIndented = true) => ToJsonString(new JsonSerializerOptions() { WriteIndented = WriteIndented });

        public string ToJsonString(JsonSerializerOptions options)
        {
            options ??= new JsonSerializerOptions();

            var writerOptions = new JsonWriterOptions
            {
                Encoder = options.Encoder,
                Indented = options.WriteIndented,
                MaxDepth = options.MaxDepth,
                SkipValidation = true
            };

            using var buffer = new PooledStream();
            using var writer = new Utf8JsonWriter((IBufferWriter<byte>)buffer, writerOptions);

            WriteTo(writer, options);
            writer.Flush();

            string text = Encoding.UTF8.GetString(buffer.Span);
            return text;
        }

        public abstract void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options = default);

        public async void WriteAsync(Utf8JsonWriter writer, JsonSerializerOptions options = default) =>
            await Task.Factory.StartNew(() => WriteTo(writer, options));

        public virtual void Dispose()
        {
            Element = default;
        }
    }
}
