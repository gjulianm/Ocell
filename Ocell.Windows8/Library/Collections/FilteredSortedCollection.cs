using System;
using System.Collections.Generic;
using System.Collections;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Ocell.Library.Collections
{
    internal class FilteredSortedCollection<T> : IObservableCollection<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var copy = CollectionChanged;
            if (copy != null)
                copy(this, args);
        }
        
        public Func<IEnumerator<T>> RequestEnumerator { get; set; }
        public Func<int> RequestCount { get; set; }
        public Func<int, T> RequestItem { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            return RequestEnumerator();
        }

        public int Count { get { return RequestCount(); } }
        public T this[int index] { get { return RequestItem(index); } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
