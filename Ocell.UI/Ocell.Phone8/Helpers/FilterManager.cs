using System.Linq;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Library.Filtering;
using System;
namespace Ocell
{
    public static class FilterManager
    {
        public static void SetupFilter(ExtendedListBox listbox)
        {
            if (listbox == null || listbox.Loader == null)
                return;

            TwitterResource resource = listbox.Resource;

            ColumnFilter filter = Config.Filters.FirstOrDefault(item => item.Resource == resource);

            if (filter != null)
            {
                filter.CleanOldFilters();
                listbox.Filter = filter;
            }
            else
                listbox.Filter = new ColumnFilter();

            listbox.Filter.Global = Config.GlobalFilter;
            listbox.Filter = listbox.Filter; // Force update of filter.
        }

        public static ITweetableFilter SetupMute(FilterType type, string data)
        {
            if (Config.GlobalFilter == null)
                Config.GlobalFilter = new ColumnFilter();

            ITweetableFilter filter = new ITweetableFilter();
            filter.Inclusion = IncludeOrExclude.Exclude;
            filter.Type = type;
            filter.Filter = data;
            if (Config.DefaultMuteTime == TimeSpan.MaxValue)
                filter.IsValidUntil = DateTime.MaxValue;
            else
                filter.IsValidUntil = DateTime.Now + (TimeSpan)Config.DefaultMuteTime;

            Config.GlobalFilter.AddFilter(filter);
            Config.GlobalFilter = Config.GlobalFilter; // Force save.
            GlobalEvents.FireFiltersChanged(filter, new EventArgs());

            return filter;
        }
    }
}
