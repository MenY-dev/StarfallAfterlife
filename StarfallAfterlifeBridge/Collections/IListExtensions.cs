using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Collections
{
    public static class IListExtensions
    {
        public static IList<T> Randomize<T>(this IList<T> self, int seed)
        {
            if (self is null)
                return null;

            var rnd = new Random(seed);
            var count = self.Count;

            for (int i = count - 1; i > 1; i--)
            {
                var newPos = rnd.Next(0, i + 1);
                var tmp = self[newPos];
                self[newPos] = self[i];
                self[i] = tmp;
            }

            return self;
        }
    }
}
