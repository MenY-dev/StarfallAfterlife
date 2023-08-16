using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class InventoryAsCargoJsonConverter : JsonConverter<InventoryItem>
    {
        public override InventoryItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonNode.Parse(ref reader);

            if (doc is null)
                return null;

            return new InventoryItem()
            {
                Id = (int?)doc["entity"] ?? -1,
                Count = (int?)doc["count"] ?? 0,
            };
        }

        public override void Write(Utf8JsonWriter writer, InventoryItem value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();

            writer.WriteStartObject();
            writer.WriteNumber("entity", value.Id);
            writer.WriteNumber("count", value.Count);
            writer.WriteString("unique_data", string.Empty);
            writer.WriteEndObject();
        }
    }
}
