using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReapalDDD
{
   public static class IEnumerableExtension
    {
        public static string Join<TSource>(this IEnumerable<TSource> source, string separator, Func<TSource, string> map)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (map == null)
                throw new ArgumentNullException("map");

            return source.Any() ? Join(source.Select(map), separator) : string.Empty;
        }
        public static string Join(this IEnumerable<string> source, string separator = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (source.Any())
            {
                separator = separator ?? string.Empty;
                if (source.Count() > 10)
                {
                    var build = new StringBuilder();
                    var etor = source.GetEnumerator();
                    var count = source.Count();
                    var pos = 1;
                    while (etor.MoveNext())
                    {
                        build.Append(etor.Current);
                        if (pos < count)
                        {
                            build.Append(separator);
                        }
                        pos++;
                    }
                    return build.ToString();
                }
                return source.Aggregate((x, y) => x + separator + y);
            }
            return string.Empty;
        }
    }
}
