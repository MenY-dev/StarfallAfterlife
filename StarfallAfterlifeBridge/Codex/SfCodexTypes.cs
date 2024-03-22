using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public static class SfCodexTypes
    {
        public struct Vector3
        {
            [JsonPropertyName("x")]
            public float X { get; set; }

            [JsonPropertyName("y")]
            public float Y { get; set; }

            [JsonPropertyName("z")]
            public float Z { get; set; }

        }

        public struct ItemDropInfo
        {
            [JsonPropertyName("item")]
            public int Item { get; set; }

            [JsonPropertyName("min")]
            public int Min { get; set; }

            [JsonPropertyName("max")]
            public int Max { get; set; }
        }

        public struct ItemCountInfo
        {
            [JsonPropertyName("item")]
            public int Item { get; set; }

            [JsonPropertyName("count")]
            public int Count { get; set; }
        }

        public struct HardpointComponent
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("width")]
            public int Width { get; set; }

            [JsonPropertyName("height")]
            public int Height { get; set; }

            [JsonPropertyName("grid_x")]
            public int GridX { get; set; }

            [JsonPropertyName("grid_y")]
            public int GridY { get; set; }

            [JsonPropertyName("angle"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public float TurnAngle { get; set; }

            [JsonPropertyName("pos"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public Vector3 Position { get; set; }

            [JsonPropertyName("rot"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public Vector3 Rotation { get; set; }
        }
    }
}
