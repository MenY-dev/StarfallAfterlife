using StarfallAfterlife.Bridge.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            var rnd = new Random128(seed);
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

        public static ObservableCollection<TSource> SortBy<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            var sorted = source.OrderBy(keySelector).ToArray();

            for (int i = 0; i < sorted.Count(); i++)
                source.Move(source.IndexOf(sorted[i]), i);

            return source;
        }

        public static ObservableCollection<TSource> Sort<TSource>(this ObservableCollection<TSource> source, IComparer<TSource> comparer)
        {
            var sorted = source.Order(comparer).ToArray();

            for (int i = 0; i < sorted.Count(); i++)
                source.Move(source.IndexOf(sorted[i]), i);

            return source;
        }

        public static ObservableCollection<TSource> Sort<TSource>(this ObservableCollection<TSource> source)
        {
            var sorted = source.Order().ToArray();

            for (int i = 0; i < sorted.Count(); i++)
                source.Move(source.IndexOf(sorted[i]), i);

            return source;
        }
    }
}
