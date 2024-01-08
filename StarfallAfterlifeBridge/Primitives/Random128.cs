using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Primitives
{
    public class Random128
    {
        private uint x, y, z, w;

        public Random128() : this(Random.Shared.Next()) { }

        public Random128(int seed)
        {
            uint f = 1812433253;
            x = (uint)seed;
            y = x * f + 1;
            z = y * f + 1;
            w = z * f + 1;
        }

        private uint NextCore()
        {
            uint t = w;
            uint s = x;

            w = z;
            z = y;
            y = s;

            t ^= t << 11;
            t ^= t >> 8;

            return x = t ^ s ^ (s >> 19);
        }

        private ulong NextCore64()
        {
            return ((ulong)NextCore() << 32) | NextCore();
        }

        public int Next()
        {
            return (int)(NextCore() % int.MaxValue);
        }

        public int Next(int maxValue)
        {
            if (maxValue < 1)
            {
                NextCore();
                return 0;
            }

            return (int)(NextCore() % maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            var range = maxValue - minValue;

            if (range < 1)
            {
                NextCore();
                return 0;
            }

            return (int)(minValue + (NextCore() % range));
        }

        public float NextSingle()
        {
            return (NextCore() >> 8) * (1.0f / (1u << 24));
        }

        public double NextDouble()
        {
            return (NextCore64() >> 11) * (1.0 / (1UL << 53));
        }
    }
}
