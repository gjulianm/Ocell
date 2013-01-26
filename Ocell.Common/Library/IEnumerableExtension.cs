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
using System.Linq;

namespace Ocell.Library
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Select all items in the closed range [from, to].
        /// </summary>
        /// <param name="list">List.</param>
        /// <param name="from">Start index. Default is 0 (start).</param>
        /// <param name="to">End index. Default is -1 (end).</param>
        /// <returns></returns>
        public static IEnumerable<T> Range<T>(this IEnumerable<T> list, int from = 0, int to = -1)
        {
            var tmp = list.Skip(from);

            if (to != -1)
                tmp = tmp.Take(to - from + 1);

            return tmp;
        }
    }
}
