using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Serialization.Json
{
    public partial class JsonNode
    {

#nullable enable

        public static implicit operator JsonNode(bool value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(bool? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(byte value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(byte? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(char value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(char? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(DateTime value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(DateTime? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(DateTimeOffset value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(DateTimeOffset? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(decimal value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(decimal? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(double value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(double? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(Guid value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(Guid? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(short value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(short? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(int value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(int? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(long value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(long? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(sbyte value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(sbyte? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(float value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(float? value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(string? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(ushort value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(ushort? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(uint value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(uint? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(ulong value) => JsonValue.Create(value);

        public static implicit operator JsonNode?(ulong? value) => JsonValue.Create(value);

        public static implicit operator JsonNode(Enum value) => JsonValue.Create(value?.ToString());

        public static explicit operator bool(JsonNode value) => value.GetValue<bool>();

        public static explicit operator bool?(JsonNode? value) => value?.GetValue<bool>();

        public static explicit operator byte(JsonNode value) => value.GetValue<byte>();

        public static explicit operator byte?(JsonNode? value) => value?.GetValue<byte>();

        public static explicit operator char(JsonNode value) => value.GetValue<char>();

        public static explicit operator char?(JsonNode? value) => value?.GetValue<char>();

        public static explicit operator DateTime(JsonNode value) => value.GetValue<DateTime>();

        public static explicit operator DateTime?(JsonNode? value) => value?.GetValue<DateTime>();

        public static explicit operator DateTimeOffset(JsonNode value) => value.GetValue<DateTimeOffset>();

        public static explicit operator DateTimeOffset?(JsonNode? value) => value?.GetValue<DateTimeOffset>();

        public static explicit operator decimal(JsonNode value) => value.GetValue<decimal>();

        public static explicit operator decimal?(JsonNode? value) => value?.GetValue<decimal>();

        public static explicit operator double(JsonNode value) => value.GetValue<double>();

        public static explicit operator double?(JsonNode? value) => value?.GetValue<double>();

        public static explicit operator Guid(JsonNode value) => value.GetValue<Guid>();

        public static explicit operator Guid?(JsonNode? value) => value?.GetValue<Guid>();

        public static explicit operator short(JsonNode value) => value.GetValue<short>();

        public static explicit operator short?(JsonNode? value) => value?.GetValue<short>();

        public static explicit operator int(JsonNode value) => value.GetValue<int>();

        public static explicit operator int?(JsonNode? value) => value?.GetValue<int>();

        public static explicit operator long(JsonNode value) => value.GetValue<long>();

        public static explicit operator long?(JsonNode? value) => value?.GetValue<long>();

        public static explicit operator sbyte(JsonNode value) => value.GetValue<sbyte>();

        public static explicit operator sbyte?(JsonNode? value) => value?.GetValue<sbyte>();

        public static explicit operator float(JsonNode value) => value.GetValue<float>();

        public static explicit operator float?(JsonNode? value) => value?.GetValue<float>();

        public static explicit operator string?(JsonNode? value) => value?.GetValue<string>();

        public static explicit operator ushort(JsonNode value) => value.GetValue<ushort>();

        public static explicit operator ushort?(JsonNode? value) => value?.GetValue<ushort>();

        public static explicit operator uint(JsonNode value) => value.GetValue<uint>();

        public static explicit operator uint?(JsonNode? value) => value?.GetValue<uint>();

        public static explicit operator ulong(JsonNode value) => value.GetValue<ulong>();

        public static explicit operator ulong?(JsonNode? value) => value?.GetValue<ulong>();

        public static explicit operator Enum?(JsonNode? value) => value?.GetValue<Enum>();
    }
}
