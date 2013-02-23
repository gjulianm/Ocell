using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (e.ItemKind == Microsoft.Phone.Controls.LongListSelectorItemKind.ListFooter)
            {
                lb.Loader.Load(true);
                lb.Loader.IsLoading = false;
            }
        }
    }
}
