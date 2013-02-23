using LinqToVisualTree;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Ocell.Controls
{
    public class WP7ScrollController : IScrollController
    {
        double loadCallScrollPosition;
        ExtendedListBox listbox;
        ScrollViewer scrollViewer;
        double scrollOffsetMargin;

        public WP7ScrollController()
        {
            Bound = false;
        }

        public void Bind(ExtendedListBox list)
        {
            if (listbox != null)
                listbox.LayoutUpdated -= OnLayoutUpdate;

            listbox = list;
            listbox.LayoutUpdated += OnLayoutUpdate;
            scrollViewer = listbox.Descendants().OfType<ScrollViewer>().FirstOrDefault();
            scrollOffsetMargin = 0.5 * scrollViewer.ViewportHeight;

            Bound = true;
        }

        public void LoadCalled(int position = -1)
        {
            if (scrollViewer == null)
                return;

            if (position == -1)
                loadCallScrollPosition = scrollViewer.VerticalOffset;
            else
                loadCallScrollPosition = position;
        }

        void OnLayoutUpdate(object sender, EventArgs e)
        {
            if (scrollViewer == null)
                return;

            var scrollPos = scrollViewer.VerticalOffset;

            if ((scrollPos + scrollOffsetMargin > loadCallScrollPosition && loadCallScrollPosition != -2) || loadCallScrollPosition == 0)
                MaintainViewport();

            lastExtentHeight = scrollViewer.ExtentHeight;
        }

        double lastExtentHeight = 0;

        void MaintainViewport()
        {
            if (lastExtentHeight != scrollViewer.ExtentHeight && lastExtentHeight != 0)
            {
                var oldLastExtentHeight = lastExtentHeight;
                lastExtentHeight = scrollViewer.ExtentHeight;

                if (oldLastExtentHeight > lastExtentHeight)
                    return; // Items removed, don't do anything.

                double toScroll = scrollViewer.VerticalOffset + (scrollViewer.ExtentHeight - oldLastExtentHeight) - 1;
                scrollViewer.ScrollToVerticalOffset(toScroll);
            }
        }

        public bool Bound { get; set; }
    }
}
