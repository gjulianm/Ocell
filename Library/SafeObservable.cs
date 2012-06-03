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
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;

namespace Ocell.Library
{
    // Adapted from http://www.deanchalk.me.uk/post/Thread-Safe-Dispatcher-Safe-Observable-Collection-for-WPF.aspx.

    public class SafeObservable<T> : IList<T>, INotifyCollectionChanged 
    {
        private IList<T> collection = new List<T>();
        private Dispatcher dispatcher;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private object sync = new object();

        public SafeObservable(IEnumerable<T> source) : this()
        {
            collection = new List<T>(source);
        }

        public SafeObservable()
        {
            dispatcher = Deployment.Current.Dispatcher;
        }

        public void Add(T item)
        {
            if (dispatcher.CheckAccess())
                DoAdd(item);
            else
                dispatcher.BeginInvoke((Action)(() => { DoAdd(item); }));
        }

        public void BulkAdd(IEnumerable<T> items)
        {
            if (dispatcher.CheckAccess())
                DoBulkAdd(items);
            else
                dispatcher.BeginInvoke(() => DoBulkAdd(items));
        }

        private void DoBulkAdd(IEnumerable<T> items)
        {
            int added = 0;
            foreach (var item in items)
            {
                DoAdd(item);
                added++;
                if (added >= 5)
                {
                    Thread.Sleep(10);
                    added = 0;
                }
            }
        }

        private void DoAdd(T item)
        {
            lock (sync)
            {
                collection.Add(item);
                int index = collection.Count;
                if (CollectionChanged != null)
                    CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        public void Clear()
        {
            if (dispatcher.CheckAccess())
                DoClear();
            else
                dispatcher.BeginInvoke((Action)(() => { DoClear(); }));
        }

        private void DoClear()
        {
            lock (sync)
            {
                collection.Clear();
                if (CollectionChanged != null)
                    CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Contains(T item)
        {
            bool result;
            lock(sync)
                result = collection.Contains(item);
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock(sync)
                collection.CopyTo(array, arrayIndex);
            
        }

        public int Count
        {
            get
            {
                int result;
                lock(sync)
                    result = collection.Count;
                return result;
            }
        }

        public bool IsReadOnly
        {
            get { return collection.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            if (dispatcher.CheckAccess())
                return DoRemove(item);
            else
            {
                dispatcher.BeginInvoke(() => DoRemove(item));
                return true; // I KNOW.
            }
        }

        private bool DoRemove(T item)
        {
            bool result;
            lock (sync)
            {
                var index = collection.IndexOf(item);
                if (index == -1)
                {
                    return false;
                }
                result = collection.Remove(item);
                if (result && CollectionChanged != null)
                    CollectionChanged(this, new
                        NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            int result;
            lock(sync)
                result = collection.IndexOf(item);
            return result;
        }

        public void Insert(int index, T item)
        {
            if (dispatcher.CheckAccess())
                DoInsert(index, item);
            else
                dispatcher.BeginInvoke((Action)(() => { DoInsert(index, item); }));
        }

        private void DoInsert(int index, T item)
        {
            lock (sync)
            {
                collection.Insert(index, item);
                if (CollectionChanged != null)
                    CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        public void RemoveAt(int index)
        {
            if (dispatcher.CheckAccess())
                DoRemoveAt(index);
            else
                dispatcher.BeginInvoke((Action)(() => { DoRemoveAt(index); }));
        }

        private void DoRemoveAt(int index)
        {
            lock (sync)
            {
                if (collection.Count == 0 || collection.Count <= index)
                {
                    return;
                }
                collection.RemoveAt(index);
                if (CollectionChanged != null)
                    CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public T this[int index]
        {
            get
            {
                T result;
                lock(sync)
                    result = collection[index];
                return result;
            }
            set
            {
                lock (sync)
                {
                    if (collection.Count == 0 || collection.Count <= index || collection[index].Equals(value))
                        return;
                    var old = collection[index];
                    collection[index] = value;
                    if(CollectionChanged != null)
                        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old, index));
                }
            }

        }
    }
}
