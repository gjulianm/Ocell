using System.Linq;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Library.Filtering;

namespace Ocell
{
    public static class FilterManager
    {
        public static void SetupFilter(ExtendedListBox listbox)
        {
            if (listbox == null || listbox.Loader == null)
                return;

            TwitterResource resource = listbox.Loader.Resource;

            ColumnFilter filter = Config.Filters.FirstOrDefault(item => item.Resource == resource);

            if (filter != null)
                listbox.Filter = filter;
            else
                listbox.Filter = new ColumnFilter();

            listbox.Filter.Global = Config.GlobalFilter;
            listbox.Filter = listbox.Filter; // Force update of filter.
        }
    }
}
