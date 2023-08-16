using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Mathematics
{
    public struct Vector2
    {
        [JsonInclude, JsonPropertyName("x")]
        public float X;

        [JsonInclude, JsonPropertyName("y")]
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator +(Vector2 a, float value) => new Vector2(a.X + value, a.Y + value);

        public static Vector2 operator -(Vector2 a, float value) => new Vector2(a.X - value, a.Y - value);

        public static Vector2 operator *(Vector2 a, float value) => new Vector2(a.X * value, a.Y * value);

        public static Vector2 operator /(Vector2 a, float value) => new Vector2(a.X / value, a.Y / value);

        public static bool operator ==(Vector2 a, Vector2 b) => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(Vector2 a, Vector2 b) => a.X != b.X || a.Y != b.Y;

        public static Vector2 Up => new Vector2(0, 1);

        public static Vector2 Down => new Vector2(0, -1);

        public static Vector2 Right => new Vector2(1, 0);

        public static Vector2 Left => new Vector2(-1, 0);

        public static Vector2 PositiveInfinity => new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        public static Vector2 NegativeInfinity => new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        public static Vector2 Zero => new Vector2(0, 0);

        public Vector2 Normalize() => Normalize(this);

        public Vector2 Rotate(float degrees) => Rotate(this, degrees);

        public float GetSize() => Size(this);

        public float GetDistanceTo(Vector2 value) => Distance(this, value);

        public Vector2 GetNegative() => Negative(this);

        public float GetAngleTo(Vector2 value) => Angle(this, value);

        public float GetAtan2() => Atan2(this);

        public static Vector2 Normalize(Vector2 value) => value * (1 / Size(value));

        public static Vector2 Rotate(Vector2 value, float degrees)
        {
            float cosAngle = MathF.Cos(degrees);
            float sinAngle = MathF.Sin(degrees);

            return new Vector2(
                cosAngle * value.X - sinAngle * value.Y,
                sinAngle * value.X + cosAngle * value.Y);
        }

        public static float Size(Vector2 value) => MathF.Sqrt(value.X * value.X + value.Y * value.Y);

        public static Vector2 Negative(Vector2 value) => new Vector2(-value.X, -value.Y);

        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => new Vector2(
            a.X + t * (b.X - a.X),
            a.Y + t * (b.Y - a.Y));

        public static Vector2 Clamp(Vector2 min, Vector2 max, Vector2 value) => new Vector2(
            value.X < min.X ? value.X : value.X > max.X ? max.X : value.X,
            value.Y < min.Y ? value.Y : value.Y > max.Y ? max.Y : value.Y);

        public static Vector2 Resize(Vector2 v, float newSize)
        {
            float currentSize = Size(v);

            if (currentSize == 0)
                return Zero;

            float delta = newSize / currentSize;
            return new Vector2(v.X * delta, v.Y * delta);
        }

        public static float Atan2(Vector2 a)
        {
            return MathF.Atan2(a.Y, a.X);
        }

        public static float Angle(Vector2 a, Vector2 b) => SfMath.ModPI(b.GetAtan2() - a.GetAtan2());

        public static float Abs(Vector2 v) => MathF.Sqrt(MathF.Pow(v.X, 2) + MathF.Pow(v.Y, 2));

        public static float Distance(Vector2 a, Vector2 b)
        {
            return MathF.Sqrt(MathF.Pow(b.X - a.X, 2) + MathF.Pow(b.Y - a.Y, 2));
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2 value &&
                   X == value.X &&
                   Y == value.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"Vector2({X}, {Y})";
        }
    }
}
