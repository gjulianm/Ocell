using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ocell.Library.Collections
{
    public class SortedFilteredObservable<T> : SafeObservable<T>, IList
    {
        IComparer<T> Comparer;
        SafeObservable<T> discardedItems;

        public SortedFilteredObservable(IComparer<T> comparer)
            : base()
        {
            Comparer = comparer;
            discardedItems = new SafeObservable<T>();
        }

        Predicate<T> filter;
        public Predicate<T> Filter
        {
            get { return filter; }
            set
            {
                filter = value;

                ReevaluateInList();
                ReevaluateDiscarded();
            }
        }

        void ReevaluateDiscarded()
        {
            List<T> itemsAdded = new List<T>();
            foreach (var item in discardedItems)
            {
                if (!Matches(item))
                {
                    itemsAdded.Add(item);
                    OrderedInsert(item);
                }
            }

            foreach (var item in itemsAdded)
                discardedItems.Remove(item);
        }

        void ReevaluateInList()
        {
            List<T> itemsToDelete = new List<T>();
            foreach (var item in this)
            {
                if (Matches(item))
                {
                    itemsToDelete.Add(item);
                    discardedItems.Add(item);
                }
            }

            foreach (var item in itemsToDelete)
                this.Remove(item);
        }

        bool Matches(T item)
        {
            if (filter == null)
                return false;

            return filter.Invoke(item);
        }

        void OrderedInsert(T item)
        {
            lock (this.sync)
            {
                int i = 0;
                while (i < Count && Comparer.Compare(item, base[i]) > 0)
                    i++;

                base.Insert(i, item);
            }
        }

        public override void Add(T item)
        {
            if (Matches(item))
                discardedItems.Add(item);
            else
                OrderedInsert(item);
        }

        object IList.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (value is T)
                    base[index] = (T)value;
            }
        }

        int IList.Add(object value)
        {
            if (value is T)
                Add((T)value);
            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            return (value is T) && Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return value is T
                ? IndexOf((T)value)
                : -1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is T)
                Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (value is T)
                Remove((T)value);
        }

        public bool IsSynchronized { get { return true; } }
        public object SyncRoot { get { return sync; } }
        public void CopyTo(Array array, int start)
        {
            // Dummy.
        }

        public bool IsFixedSize { get { return false; } }
    }
}
