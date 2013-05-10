using System;
using System.Collections;
using System.Collections.Generic;

namespace Ocell.Library
{
    // Just to avoid loading System.Linq
    public static class CustomIEnumerableExtensions
    {
        public static IEnumerable<TDestination> Cast<TDestination>(this IEnumerable list) where TDestination : class
        {
            foreach (var item in list)
                yield return item as TDestination;
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> list, Func<T, bool> condition)
        {
            foreach (var item in list)
                if (condition.Invoke(item))
                    yield return item;
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> list)
        {
            return list.FirstOrDefault((item) => true);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> list, Func<T, bool> condition)
        {
            foreach (var item in list)
                if (condition.Invoke(item))
                    return item;

            return default(T);
        }

        public static bool Any<T>(this IEnumerable<T> list)
        {
            return list.GetEnumerator().MoveNext();
        }

        public static int Count<T>(this IEnumerable<T> list)
        {
            int count = 0;

            var enumerator = list.GetEnumerator();

            while (enumerator.MoveNext())
                count++;

            return count;
        }

        public static List<T> ToList<T>(this IEnumerable<T> list)
        {
            List<T> returned = new List<T>();
            foreach (var item in list)
                returned.Add(item);

            return returned;
        }

        public static IEnumerable<TDest> Select<T, TDest>(this IEnumerable<T> list, Func<T, TDest> transform)
        {
            foreach (var item in list)
                yield return transform(item);
        }

    }
}
