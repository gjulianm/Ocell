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
using Ocell.Library;
using Ocell.Controls;
using System.Linq;

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
            {
                listbox.Filter = filter;

                listbox.Filter.Global = Config.FilterGlobal;
            }
        }
    }
}
