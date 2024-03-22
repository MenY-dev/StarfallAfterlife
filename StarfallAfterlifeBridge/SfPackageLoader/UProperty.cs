using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public struct UProperty
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public object Value { get; set; }

        public FPropertyTag Tag { get; set; }

        public T GetValue<T>()
        {
            if (TryGetValue(out T result) == true)
                return result;

            return default;
        }

        public bool TryGetValue<T>(out T value)
        {
            if (Value is T result)
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }

        public T ConvertTo<T>()
        {
            if (Value is object toConvert &&
                TypeDescriptor.GetConverter(Value.GetType()) is TypeConverter converter &&
                converter.CanConvertTo(typeof(T)) == true)
                return (T)converter.ConvertTo(toConvert, typeof(T));

            return default;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        public static explicit operator string(UProperty prop) => prop.GetValue<string>();

        public static explicit operator FText(UProperty prop) => prop.GetValue<FText>();
        public static explicit operator FText?(UProperty prop) => prop.GetValue<FText?>();

        public static explicit operator bool(UProperty prop) => prop.GetValue<bool>();
        public static explicit operator bool?(UProperty prop) => prop.GetValue<bool?>();

        public static explicit operator byte(UProperty prop) => prop.GetValue<byte>();
        public static explicit operator byte?(UProperty prop) => prop.GetValue<byte?>();

        public static explicit operator sbyte(UProperty prop) => prop.GetValue<sbyte>();
        public static explicit operator sbyte?(UProperty prop) => prop.GetValue<sbyte?>();

        public static explicit operator char(UProperty prop) => prop.GetValue<char>();
        public static explicit operator char?(UProperty prop) => prop.GetValue<char?>();

        public static explicit operator short(UProperty prop) => prop.GetValue<short>();
        public static explicit operator short?(UProperty prop) => prop.GetValue<short?>();

        public static explicit operator ushort(UProperty prop) => prop.GetValue<ushort>();
        public static explicit operator ushort?(UProperty prop) => prop.GetValue<ushort?>();

        public static explicit operator int(UProperty prop) => prop.GetValue<int>();
        public static explicit operator int?(UProperty prop) => prop.GetValue<int?>();

        public static explicit operator uint(UProperty prop) => prop.GetValue<uint>();
        public static explicit operator uint?(UProperty prop) => prop.GetValue<uint?>();

        public static explicit operator long(UProperty prop) => prop.GetValue<long>();
        public static explicit operator long?(UProperty prop) => prop.GetValue<long?>();

        public static explicit operator ulong(UProperty prop) => prop.GetValue<ulong>();
        public static explicit operator ulong?(UProperty prop) => prop.GetValue<ulong?>();

        public static explicit operator float(UProperty prop) => prop.GetValue<float>();
        public static explicit operator float?(UProperty prop) => prop.GetValue<float?>();

        public static explicit operator double(UProperty prop) => prop.GetValue<double>();
        public static explicit operator double?(UProperty prop) => prop.GetValue<double?>();

        public static explicit operator decimal(UProperty prop) => prop.GetValue<decimal>();
        public static explicit operator decimal?(UProperty prop) => prop.GetValue<decimal?>();

        public static explicit operator Guid(UProperty prop) => prop.GetValue<Guid>();
        public static explicit operator Guid?(UProperty prop) => prop.GetValue<Guid?>();

        public static explicit operator string[](UProperty prop) => prop.GetValue<string[]>();
        public static explicit operator bool[](UProperty prop) => prop.GetValue<bool[]>();
        public static explicit operator byte[](UProperty prop) => prop.GetValue<byte[]>();
        public static explicit operator sbyte[](UProperty prop) => prop.GetValue<sbyte[]>();
        public static explicit operator char[](UProperty prop) => prop.GetValue<char[]>();
        public static explicit operator short[](UProperty prop) => prop.GetValue<short[]>();
        public static explicit operator ushort[](UProperty prop) => prop.GetValue<ushort[]>();
        public static explicit operator int[](UProperty prop) => prop.GetValue<int[]>();
        public static explicit operator uint[](UProperty prop) => prop.GetValue<uint[]>();
        public static explicit operator long[](UProperty prop) => prop.GetValue<long[]>();
        public static explicit operator float[](UProperty prop) => prop.GetValue<float[]>();
        public static explicit operator double[](UProperty prop) => prop.GetValue<double[]>();
        public static explicit operator decimal[](UProperty prop) => prop.GetValue<decimal[]>();

        public static explicit operator UProperty[](UProperty prop) => prop.GetValue<UProperty[]>();
        public static explicit operator List<UProperty>(UProperty prop) => prop.GetValue<List<UProperty>>();
    }
}
