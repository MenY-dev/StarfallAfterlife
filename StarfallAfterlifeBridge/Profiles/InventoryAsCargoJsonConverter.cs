﻿using System;
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
                return InventoryItem.Empty;

            return new InventoryItem()
            {
                Id = (int?)doc["entity"] ?? -1,
                Count = (int?)doc["count"] ?? 0,
                UniqueData = (string)doc["unique_data"],
            };
        }

        public override void Write(Utf8JsonWriter writer, InventoryItem value, JsonSerializerOptions options)
        {
            if (value.IsEmpty)
                writer.WriteNullValue();

            writer.WriteStartObject();
            writer.WriteNumber("entity", value.Id);
            writer.WriteNumber("count", value.Count);
            writer.WriteString("unique_data", value.UniqueData ?? "");
            writer.WriteEndObject();
        }
    }
}
