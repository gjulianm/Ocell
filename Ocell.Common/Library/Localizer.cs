using System;

#if METRO
using Windows.ApplicationModel.Resources;
#endif
namespace Ocell.Library
{
    public static class Localizer
    {
        static ResourceLoader loader = new ResourceLoader(); 

        public static string GetString(string name)
        {
#if METRO
            return loader.GetString(name);
#else
#endif
        }
    }
}