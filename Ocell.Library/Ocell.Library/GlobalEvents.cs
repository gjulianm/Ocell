using System;
using System.Threading.Tasks;
namespace Ocell.Library
{
    public static class GlobalEvents
    {
        public static event EventHandler FiltersChanged;

        public static void FireFiltersChanged(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (FiltersChanged != null)
                    FiltersChanged(sender, e);
            });
        }
    }
}
