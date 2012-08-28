using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Library.Collections
{
    public enum SortDirection { Ascending, Descending }

    public interface ISortDescription<T>
    {
        IEnumerable<T> Sort(IEnumerable<T> collection);
    }

    public class SortDescription<T, TKey> : ISortDescription<T>
    {
        public Func<T, TKey> KeySelector { get; set; }

        public IComparer<TKey> Comparer { get; set; }

        public SortDirection Direction { get; set; }

        public IEnumerable<T> Sort(IEnumerable<T> collection)
        {
            if (KeySelector == null)
                return collection;

            if (Comparer == null)
            {
                if (Direction == SortDirection.Ascending)
                    return collection.OrderBy(KeySelector);
                else
                    return collection.OrderByDescending(KeySelector);
            }
            else
            {
                if (Direction == SortDirection.Ascending)
                    return collection.OrderBy(KeySelector, Comparer);
                else
                    return collection.OrderByDescending(KeySelector, Comparer);
            }
        }
    }
}
