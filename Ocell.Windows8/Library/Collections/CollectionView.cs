using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Library.Collections
{
    /// <summary>
    /// Provides a class which sorts and filters any collection, similar to CollectionViewSource in previous versions.
    /// </summary>
    /// <typeparam name="T">Collection type.</typeparam>
    public class CollectionView<T>
    {
        public CollectionView()
        {
            view = new FilteredSortedCollection<T>();
            view.RequestEnumerator = GetEnumerator;
            view.RequestCount = GetCount;
            view.RequestItem = GetItem;

            sortDescriptions = new Collection<ISortDescription<T>>();
        }

        IEnumerable<T> source;
        public IEnumerable<T> Source
        {
            get
            {
                return source; 
            }
            set
            {
                TryUnbindCollectionChanged(source);
                TryBindCollectionChanged(value);

                source = value;
            }
        }

        int GetCount()
        {
            if (source is ICollection<T>)
                return ((ICollection<T>)source).Count;
            else if (source is IReadOnlyCollection<T>)
                return ((IReadOnlyCollection<T>)source).Count;
            else
                return source.Count();
        }

        T GetItem(int index)
        {
            if (source is IList<T>)
                return ((IList<T>)source)[index];
            else if (source is IReadOnlyList<T>)
                return ((IReadOnlyList<T>)source)[index];
            else
                return source.ElementAt(index);
        }

        IEnumerator<T> GetEnumerator()
        {
            IEnumerable<T> collection = source.Where(x => x != null && Filter.Invoke(x));
            foreach (var sorter in SortDescriptions)
                collection = sorter.Sort(collection);

            return collection.GetEnumerator();
        }

        void TryUnbindCollectionChanged(IEnumerable<T> collection)
        {
            if (collection is INotifyCollectionChanged)
                ((INotifyCollectionChanged)collection).CollectionChanged -= SourceCollectionChanged;
        }

        void TryBindCollectionChanged(IEnumerable<T> collection)
        {
            if (collection is INotifyCollectionChanged)
                ((INotifyCollectionChanged)collection).CollectionChanged += SourceCollectionChanged;
        }

        void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Replace
                || e.Action == NotifyCollectionChangedAction.Add)
            {
                var changed = e.NewItems.OfType<T>().Where(x => Filter.Invoke(x));
                var args = new NotifyCollectionChangedEventArgs(e.Action, changed);
                view.RaiseCollectionChanged(args);
            }
            else
                view.RaiseCollectionChanged(e);
        }

        Predicate<T> filter;
        public Predicate<T> Filter
        {
            get
            {
                if (filter == null)
                    filter = (obj) => true;

                return filter;
            }
            set
            {
                filter = value;
            }
        }

        Collection<ISortDescription<T>> sortDescriptions;
        public IList<ISortDescription<T>> SortDescriptions
        {
            get { return sortDescriptions; }
        }

        FilteredSortedCollection<T> view;
        public IObservableCollection<T> View
        {
            get { return view; }
        }
    }
}
