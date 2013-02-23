using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Ocell.Library.Collections
{
    /// <summary>
    /// Connects two IObservableCollection's to keep changes from source to destiny.
    /// </summary>
    public class CollectionBridge<T> : IDisposable
    {
        IList<T> dest;
        INotifyCollectionChanged src;

        /// <summary>
        /// Constructor. Creates the bridge propagating changes from source to destiny.
        /// WARNING: source needs to implement INotifyCollectionChanged.
        /// </summary>
        /// <param name="source">Source collection. Mandatory to implement INotifyCollectionChanged.</param>
        /// <param name="destiny">Destination</param>
        public CollectionBridge(IEnumerable<T> source, IList<T> destiny)
        {
            if (!(source is INotifyCollectionChanged))
                throw new ArgumentException("Source does not implement INotifyCollectionChanged", "source");

            src = ((INotifyCollectionChanged)source);
            src.CollectionChanged += CollectionChanged;
            dest = destiny;
        }

        ~CollectionBridge()
        {
            Dispose();
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (dest == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.OfType<T>())
                        dest.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.OfType<T>())
                        dest.Remove(item);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    dest.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.NewItems.OfType<T>())
                        dest.Add(item);
                    foreach (var item in e.OldItems.OfType<T>())
                        dest.Remove(item);
                    break;
            }
        }

        public void Dispose()
        {
            if(src != null)
                src.CollectionChanged -= CollectionChanged;
            dest = null;
        }
    }
}