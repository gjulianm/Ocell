using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using LinqToVisualTree;

namespace Ocell.Controls
{
    public class WP7InfiniteScroller : IInfiniteScroller
    {
        ExtendedListBox lb;
        ScrollViewer scrollViewer;

        public bool Bound { get; private set; }

        public void Bind(ExtendedListBox listbox)
        {
            lb = listbox;

            scrollViewer = lb.Descendants().OfType<ScrollViewer>().FirstOrDefault();

            if (scrollViewer == null)
                throw new NotSupportedException("ExtendedListbox must have an underlying ScrollViewer");

            lb.ManipulationCompleted += lb_ManipulationCompleted;

            Bound = true;
        }

        public void Unbind()
        {
            if (lb != null && scrollViewer != null)
                lb.ManipulationCompleted -= lb_ManipulationCompleted;
        }

        void lb_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            CheckInfiniteScroll();
        }

        void CheckInfiniteScroll()
        {
            var distToBottom = scrollViewer.ExtentHeight - scrollViewer.VerticalOffset;
            const double trigger = 25;

            if (distToBottom > 0 && distToBottom < trigger)
            {
                lb.LoadOld();
                lb.Loader.IsLoading = false; // Supress the progress bar.
            }
        }
    }
}
