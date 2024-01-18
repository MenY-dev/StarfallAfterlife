using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization
{
    public static class JsonHelpers
    {
        public static JsonNode ParseNode(string jsonString, JsonSerializerOptions options = default)
        {
            try
            {
                return JsonNode.Parse(jsonString);
            }
            catch { return null; }
        }

        public static JsonNode ParseNode(object value, JsonSerializerOptions options = default)
        {
            try
            {
                return JsonSerializer.SerializeToNode(value, options);
            }
            catch { return null; }
        }

        public static JsonNode ParseNodeFromFile(string path, JsonSerializerOptions options = default)
        {
            try
            {
                return ParseNode(File.ReadAllText(path), options);
            }
            catch { return null; }
        }

        public static bool WriteToFile(this JsonNode self, string path, JsonSerializerOptions options = default)
        {
            try
            {
                if (self?.ToJsonString(options?.WriteIndented ?? true) is string text)
                {
                    if (Path.GetDirectoryName(path) is string dir &&
                        Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(path, text);
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static JsonNode ParseNodeUnbuffered(string jsonString, JsonSerializerOptions options = default)
        {
            try
            {
                var maxBytesCount = Encoding.UTF8.GetMaxByteCount(jsonString.Length);
                var buffer = new byte[maxBytesCount].AsSpan();
                var totalBytesCount = Encoding.UTF8.GetEncoder().GetBytes(jsonString, buffer, true);
                return JsonNode.Parse(buffer.Slice(0, totalBytesCount));
            }
            catch
            {
                return null;
            }
        }

        public static JsonNode ParseNodeUnbuffered(object value, JsonSerializerOptions options = default)
        {
            try
            {
                using var buffer = new MemoryStream();
                SerializeUnbuffered(buffer, value, options);
                return JsonNode.Parse(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, (int)buffer.Position));
            }
            catch { return null; }
        }

        public static JsonNode ParseNodeFromFileUnbuffered(string path, JsonSerializerOptions options = default)
        {
            try
            {
                return ParseNodeUnbuffered(File.ReadAllText(path), options);
            }
            catch { return null; }
        }

        public static bool WriteToFileUnbuffered(this JsonNode self, string path, JsonSerializerOptions options = default)
        {
            try
            {
                if (self?.ToJsonStringUnbuffered(options?.WriteIndented ?? true) is string text)
                {
                    if (Path.GetDirectoryName(path) is string dir &&
                        Directory.Exists(dir) == false)
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(path, text);
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static JsonNode Clone(this JsonNode self)
        {
            try
            {
                using var buffer = new MemoryStream();
                using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false });
                self.WriteTo(writer);
                writer.Flush();
                return JsonNode.Parse(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, (int)buffer.Position));
            }
            catch { return null; }
        }

        public static JsonNode Override(this JsonNode self, IEnumerable<KeyValuePair<string, JsonNode>> nodes)
        {
            try
            {
                if (self is not JsonObject)
                    return self;

                foreach (var item in nodes)
                {
                    if (item.Key is null)
                        continue;

                    if (item.Value is JsonNode node &&
                        node.Parent is not null)
                        self[item.Key] = node.Clone();
                    else
                        self[item.Key] = item.Value;
                }
            }
            catch {}

            return self;
        }

        public static string ToJsonString(this JsonNode self, bool writeIndented)
        {
            try
            {
                return self?.ToJsonString(new JsonSerializerOptions { WriteIndented = writeIndented });
            }
            catch { return default; }
        }

        public static string ToJsonStringUnbuffered(this JsonNode self, bool writeIndented)
        {
            try
            {
                using var buffer = new MemoryStream();
                using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = writeIndented });
                self.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, (int)buffer.Position));
            }
            catch { return default; }
        }

        public static void SerializeUnbuffered<TValue>(Stream utf8Stream, TValue value, JsonSerializerOptions options = default)
        {
            try
            {
                options ??= new JsonSerializerOptions();

                using var writer = new Utf8JsonWriter(utf8Stream, new JsonWriterOptions
                {
                    Encoder = options.Encoder,
                    Indented = options.WriteIndented,
                    MaxDepth = options.MaxDepth,
                    SkipValidation = true
                });

                JsonSerializer.Serialize(writer, value, options);
                writer.Flush();
            }
            catch { }
        }

        public static string SerializeUnbuffered<TValue>(TValue value, JsonSerializerOptions options = default)
        {
            try
            {
                using var buffer = new MemoryStream();
                SerializeUnbuffered(buffer, value, options);
                return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, (int)buffer.Position));
            }
            catch { return default; }
        }

        public static TValue DeserializeUnbuffered<TValue>(ReadOnlySpan<byte> span, JsonSerializerOptions options = default)
        {
            try
            {
                var reader = new Utf8JsonReader(span);
                return JsonSerializer.Deserialize<TValue>(ref reader, options);
            }
            catch
            {
                return default;
            }
        }

        public static TValue DeserializeUnbuffered<TValue>(string json, JsonSerializerOptions options = default)
        {
            try
            {
                return DeserializeUnbuffered<TValue>(CreateUTF8JsonSpan(json));
            }
            catch { return default; }
        }


        public static TValue DeserializeUnbuffered<TValue>(this JsonNode self, JsonSerializerOptions options = default)
        {
            try
            {
                using var buffer = new MemoryStream();
                SerializeUnbuffered(buffer, self, options);
                return DeserializeUnbuffered<TValue>(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, (int)buffer.Position));
            }
            catch { return default; }
        }

        private static ReadOnlySpan<byte> CreateUTF8JsonSpan(string jsonString)
        {
            var maxBytesCount = Encoding.UTF8.GetMaxByteCount(jsonString.Length);
            var buffer = new byte[maxBytesCount].AsSpan();
            var totalBytesCount = Encoding.UTF8.GetEncoder().GetBytes(jsonString, buffer, true);
            return buffer.Slice(0, totalBytesCount);
        }

        public static JsonObject AsObjectSelf(this JsonNode self)
        {
            try
            {
                return self?.AsObject();
            }
            catch { return null; }
        }

        public static JsonArray AsArraySelf(this JsonNode self)
        {
            try
            {
                return self?.AsArray();
            }
            catch { return null; }
        }

        public static JsonValue AsValueSelf(this JsonNode self)
        {
            try
            {
                return self?.AsValue();
            }
            catch { return null; }
        }

        public static JsonArray ToJsonArray(this IEnumerable<JsonNode> self)
        {
            if (self is JsonArray)
                return self as JsonArray;

            var result = new JsonArray();

            if (self is not null)
                foreach (var item in self)
                    result.Add(item);

            return result;
        }
    }
}
