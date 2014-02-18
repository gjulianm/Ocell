using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using LinqToVisualTree;
using Ocell.Library;
using Ocell.Library.Twitter;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TweetSharp;

namespace Ocell.Controls
{
    public class WP8ReadingPositionManager : IReadingPositionManager
    {
        ExtendedListBox lb;
        TwitterResource resource;

        public bool Bound { get; protected set; }
        public void Bind(ExtendedListBox listbox)
        {
            if (listbox.Loader == null)
                return;

            lb = listbox;
            resource = listbox.Loader.Resource;
            lb.ManipulationStateChanged += lb_ManipulationStateChanged;
            Bound = true;
        }

        void lb_ManipulationStateChanged(object sender, EventArgs e)
        {
            SavePosition();
        }

        public void Unbind()
        {
            Bound = false;
            lb.ManipulationStateChanged -= lb_ManipulationStateChanged;
            lb = null;
            resource = null;
        }

        public bool CanRecoverPosition()
        {
            long id;
            return Bound && lb != null && Config.ReadPositions.Value.TryGetValue(resource.String, out id)
                            && !lb.VisibleItems.Any(x => x.Id == id);
        }

        private ITweetable GetFirstVisibleItem()
        {
            var items = lb.ViewportItems;

            if (items.Count == 0)
                return null;

            var offset = lb.Descendants().OfType<ViewportControl>().FirstOrDefault().Viewport.Top;
            var visibleItems = items.Where(x => Canvas.GetTop(x.Value) + x.Value.ActualHeight > offset)
                .OrderBy(x => Canvas.GetTop(x.Value));

            if (visibleItems.Any())
                return visibleItems.First().Key;
            else
                return null;
        }

        public void SavePosition()
        {
            if (!Bound)
                return;

            var first = GetFirstVisibleItem();

            if (first != null && resource != null)
            {
                Config.ReadPositions.Value[resource.String] = first.Id;
                Debug.WriteLine("Saved tweet from {0}", first.AuthorName);
            }
        }

        public void RecoverPosition()
        {
            long tweetId = Config.ReadPositions.Value[resource.String];
            var tweet = lb.Loader.Source.FirstOrDefault(x => x.Id == tweetId);

            Dependency.Resolve<IDispatcher>().InvokeIfRequired(() =>
            {
                if (tweet != null)
                    lb.ScrollTo(tweet);
            });
        }
    }
}
