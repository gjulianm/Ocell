using System.Collections;
using System.Collections.Specialized;

namespace Ocell.Library
{
    public class ObservableCollectionReplayer
    {
        private IList target;

        public void ReplayTo(INotifyCollectionChanged source, IList target)
        {
            source.CollectionChanged += CollectionChanged;
            this.target = target;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
                target.Add(e.NewItems[0]);
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null && e.OldItems.Count > 0)
                target.RemoveAt(e.OldStartingIndex);
        }
    }
}
