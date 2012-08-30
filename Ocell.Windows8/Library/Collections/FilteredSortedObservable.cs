using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Threading;

namespace Ocell.Library.Collections
{
    /// <summary>
    /// This class provides all necessary methods to filter, sort and observe a collection. It's also
    /// thread safe.
    /// 
    /// I wanted to call this class SuperMan but that would be indeed a bad name.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    public class FilteredSortedObservable<T> : ObservableCollection<T>, IEnumerable<T>, IEnumerable
    {
        SortedSet<T> set;
        ReaderWriterLockSlim rwLock;

        #region Constructors
        public FilteredSortedObservable()
        {
            rwLock = new ReaderWriterLockSlim();
            try
            {
                SynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
            }
            catch (InvalidOperationException)
            {
                SynchronizationContext = TaskScheduler.Current;
            }

            if (rwLock.TryEnterWriteLock(-1))
            {
                set = new SortedSet<T>();
                rwLock.ExitWriteLock();
            }
        }

        public FilteredSortedObservable(IComparer<T> comparer)
            : this()
        {
            if (rwLock.TryEnterWriteLock(-1))
            {
                set = new SortedSet<T>(comparer);
                rwLock.ExitWriteLock();
            }

        }

        public FilteredSortedObservable(IEnumerable<T> collection)
            : this()
        {
            if (rwLock.TryEnterWriteLock(-1))
            {
                set = new SortedSet<T>(collection);
                rwLock.ExitWriteLock();
            }
        }

        public FilteredSortedObservable(IEnumerable<T> collection, IComparer<T> comparer)
            : this()
        {
            if (rwLock.TryEnterWriteLock(-1))
            {
                set = new SortedSet<T>(collection, comparer);
                rwLock.ExitWriteLock();
            }
        }
        #endregion

        protected void CallOnSyncThread(Action action)
        {
            if (TaskScheduler.Current == SynchronizationContext)
                action();
            else
            {
                var task = new Task(action);
                task.Start(SynchronizationContext);
            }
        }


        #region Collection base methods
        protected override void InsertItem(int index, T item)
        {
            if (rwLock.TryEnterWriteLock(-1))
            {
                set.Add(item);
                CallOnSyncThread(() => base.InsertItem(index, item));
                rwLock.ExitWriteLock();
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            // Do absolutely nothing. It's a sorted list, we can't move elements.
        }

        protected override void RemoveItem(int index)
        {
            if (rwLock.TryEnterWriteLock(-1))
            {
                var item = this[index];
                set.Remove(item);
                CallOnSyncThread(() => base.RemoveItem(index));
                rwLock.ExitWriteLock();
            }              
        }

        protected override void SetItem(int index, T item)
        {
            if (rwLock.TryEnterWriteLock(-1))
            {
                var previous = this[index];
                set.Remove(previous);
                set.Add(item);
                CallOnSyncThread(() => base.SetItem(index, item));
                rwLock.ExitWriteLock();
            }
        }
        #endregion

        public new bool Remove(T item)
        {
            bool done = false;

            if (rwLock.TryEnterWriteLock(-1))
            {
                done = set.Remove(item);
                CallOnSyncThread(() => base.Remove(item));
                rwLock.ExitWriteLock();
            }

            return done;
        }

        public new T this[int index]
        {
            get
            {
                T item = default(T);
                if (rwLock.TryEnterReadLock(-1))
                {
                    item = set.ElementAt(index);
                    rwLock.ExitReadLock();
                }

                return item;
            }
            set
            {
                SetItem(index, value);
            }
        }

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

        #region IEnumerable<T> and IEnumerable methods
        public new IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> e = new List<T>().GetEnumerator(); // Default empty enumerator when the lock is not taken.

            if (rwLock.TryEnterReadLock(-1))
            {
                if (Filter != null)
                    e = set.Where(Filter).ToList().GetEnumerator();
                else
                    e = set.ToList().GetEnumerator();
                rwLock.ExitReadLock();
            }

            return e;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public IComparer<T> Comparer
        {
            get
            {
                return set.Comparer;
            }
        }

        public Func<T, bool> Filter { get; set; }

        public TaskScheduler SynchronizationContext { get; set; }
    }
}
