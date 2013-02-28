using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.Controls
{
    public class WP8InfiniteScroller : IInfiniteScroller
    {
        ExtendedListBox lb;
        public bool Bound { get; private set; }

        public void Bind(ExtendedListBox listbox)
        {
            lb = listbox;
            lb.ItemRealized += lb_ItemRealized;
        }

        void lb_ItemRealized(object sender, Microsoft.Phone.Controls.ItemRealizationEventArgs e)
        {
            var tweet = e.Container.DataContext as ITweetable;

            if (tweet != null && lb.Loader.Source.LastOrDefault() == tweet)
            {
                lb.LoadOld();
                lb.Loader.IsLoading = false; // Supress loading bar.
            }
        }
    }
}
