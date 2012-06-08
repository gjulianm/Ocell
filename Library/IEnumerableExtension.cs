using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Ocell.Library
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<TTransform> Transform<TTransform, TSource>(this IEnumerable<TSource> list, Func<TSource, TTransform> transformer)
        {
            foreach (var item in list)
                yield return transformer.Invoke(item);
        }
    }
}
