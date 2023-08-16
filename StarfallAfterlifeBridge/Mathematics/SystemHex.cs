using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Mathematics
{
    //[JsonConverter(typeof(SystemHexJsonConverter))]
    public struct SystemHex
    {
        [JsonInclude, JsonPropertyName("x")]
        public int X { get; set; }

        [JsonInclude, JsonPropertyName("y")]
        public int Y { get; set; }

        public static SystemHex Zero => new SystemHex(0, 0);

        public static readonly SystemHex[] Sides = new SystemHex[]
        {
            new SystemHex(0, 1),   // 0
            new SystemHex(1, 0),   // 1
            new SystemHex(1, -1),  // 2
            new SystemHex(0, -1),  // 3
            new SystemHex(-1, 0),  // 4
            new SystemHex(-1, 1),  // 5
        };

        public static SystemHex operator +(SystemHex a, SystemHex b) => new SystemHex(a.X + b.X, a.Y + b.Y);

        public static SystemHex operator -(SystemHex a, SystemHex b) => new SystemHex(a.X - b.X, a.Y - b.Y);

        public static SystemHex operator *(SystemHex a, int scale) => new SystemHex(a.X * scale, a.Y * scale);

        public static SystemHex operator /(SystemHex a, int scale) => new SystemHex(a.X / scale, a.Y / scale);

        public static bool operator ==(SystemHex a, SystemHex b) => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(SystemHex a, SystemHex b) => a.X != b.X || a.Y != b.Y;

        public SystemHex(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int GetSize() => (Math.Abs(X) + Math.Abs(Y) + Math.Abs(-X - Y)) / 2;

        public static int Distance(SystemHex a, SystemHex b) => (a - b).GetSize();

        public int GetDistanceTo(SystemHex hex) => (this - hex).GetSize();

        public List<SystemHex> GetRing(int radius) => new(GetRingEnumerator(radius));

        public IEnumerable<SystemHex> GetRingEnumerator(int radius)
        {
            if (radius < 1)
                yield break;

            var hex = this + Sides[4] * radius;

            for (int currentSide = 0; currentSide < 6; currentSide++)
            {
                var dir = Sides[currentSide];

                for (int i = 0; i < radius; i++)
                {
                    var result = hex;
                    hex += dir;
                    yield return result;
                }
            }

            yield break;
        }

        public List<SystemHex> GetSpiral(int radius) => new(GetSpiralEnumerator(radius));

        public IEnumerable<SystemHex> GetSpiralEnumerator(int radius)
        {
            yield return this;

            for (int r = 1; r <= radius; r++)
                foreach (var hex in GetRingEnumerator(r))
                    yield return hex;
        }

        public override bool Equals(object obj)
        {
            return obj is SystemHex value &&
                   X == value.X &&
                   Y == value.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"Hex({X}, {Y})";
        }

        public class SystemHexJsonConverter : JsonConverter<SystemHex>
        {
            public override SystemHex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                var result = new SystemHex();
                string propertyName = null;

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.PropertyName:
                            {
                                propertyName = reader.GetString();
                                break;
                            }
                        case JsonTokenType.Number:
                            {
                                switch (propertyName)
                                {
                                    case "x": result.X = reader.GetInt32(); break;
                                    case "y": result.Y = reader.GetInt32(); break;
                                }
                                break;
                            }
                        case JsonTokenType.EndObject:
                            return result;
                    }
                }

                return result;
            }

            public override void Write(Utf8JsonWriter writer, SystemHex value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("x", value.X);
                writer.WriteNumber("y", value.Y);
                writer.WriteEndObject();
            }
        }
    }
}
