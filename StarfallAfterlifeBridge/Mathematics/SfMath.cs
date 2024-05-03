using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Mathematics
{
    public static class SfMath
    {
        public const float Pi = MathF.PI;

        public const float Tau = MathF.Tau;

        public static float Abs(float x) => Math.Abs(x);

        public static float ModPI(float angle)
        {
            if (angle > Pi)
                angle -= Tau;

            if (angle < -Pi)
                angle += Tau;

            return angle;
        }

        public static float Mod2PI(float angle)
        {
            if (angle < 0)
                angle += Tau;

            return angle % Tau;
        }

        public static bool IsAngleInRange(float min, float max, float value)
        {
            if (min <= max)
            {
                if (max - min <= Pi)
                    return min <= value && value <= max;
                else
                    return max <= value || value <= min;
            }
            else
            {
                if (min - max <= Pi)
                    return max <= value && value <= min;
                else
                    return min <= value || value <= max;
            }
        }

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;

            if (value > max)
                value = max;

            return value;
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        public static float Pow(float value) => value * value;

        public static float Acos(float value) => MathF.Acos(value);

        public static T AddWithoutOverflow<T>(this T self, T value)
            where T : IMinMaxValue<T>, IAdditionOperators<T, T, T>
        {
            try
            {
                checked
                {
                    return self + value;
                }
            }
            catch
            {
                return T.MaxValue;
            }
        }

        public static T SubtractWithoutOverflow<T>(this T self, T value)
            where T : IMinMaxValue<T>, ISubtractionOperators<T, T, T>
        {
            try
            {
                checked
                {
                    return self - value;
                }
            }
            catch
            {
                return T.MinValue;
            }
        }
    }
}
