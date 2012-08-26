using System;
using System.Threading;
#if METRO
using System.Threading.Tasks;
#endif
namespace Ocell.Library
{
    public static class GlobalEvents
    {
        public static event EventHandler FiltersChanged;

        public static void FireFiltersChanged(object sender, EventArgs e)
        {
#if !METRO
            ThreadPool.QueueUserWorkItem((threadcontext) => {
                if (FiltersChanged != null)
                    FiltersChanged(sender, e);
            });
#else
            Task.Run(() => {
                if (FiltersChanged != null)
                    FiltersChanged(sender, e);
            });
#endif
        }
    }
}
