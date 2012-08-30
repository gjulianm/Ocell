using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace Ocell.Library.Collections
{
    public class FilteredSortedObservable<T> : ObservableCollection<T>, IEnumerable<T>, IEnumerable
    {
        SortedSet<T> set;

        #region Constructors
        public FilteredSortedObservable()
        {
            set = new SortedSet<T>();
        }

        public FilteredSortedObservable(IComparer<T> comparer)
        {
            set = new SortedSet<T>(comparer);
        }

        public FilteredSortedObservable(IEnumerable<T> collection)
        {
            set = new SortedSet<T>(collection);
        }

        public FilteredSortedObservable(IEnumerable<T> collection, IComparer<T> comparer)
        {
            set = new SortedSet<T>(collection, comparer);
        }
        #endregion

        protected override void InsertItem(int index, T item)
        {
            set.Add(item);
            base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            set.Remove(Items[index]);
            base.RemoveItem(index);
        }

        public new T this[int index]
        {
            get
            {
                return set.ElementAt(index);
            }
            set
            {
                SetItem(index, value);
            }
        }

        protected override void SetItem(int index, T item)
        {
            RemoveItem(index);
            set.Add(item);
            base.SetItem(index, item);
        }



        public new IEnumerator<T> GetEnumerator()
        {
            if (Filter != null)
                return set.Where(Filter).GetEnumerator();
            else
                return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IComparer<T> Comparer
        {
            get
            {
                return set.Comparer;
            }
        }

        public Func<T, bool> Filter { get; set; }

        public new int Count
        {
            get
            {
                int count = 0;

                using (var e = GetEnumerator())
                {
                    while (e.MoveNext())
                        count++;
                }

                return count;
            }
        }
    }
}
