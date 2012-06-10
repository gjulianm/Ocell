﻿using System;
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

            foreach (var item in list)
                count++;

            return count;
        }
    }
}
