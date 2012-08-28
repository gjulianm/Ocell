#if METRO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Foundation.Collections;
namespace Ocell.Library
{
    public static class IPropertySetExtensions
    {
        public static bool TryGetValue<T>(this IPropertySet set, string key, out T item)
        {
            item = default(T);

            if (!set.ContainsKey(key))
                return false;

            object obj = set[key];

            if (!(obj is T))
                return false;

            item = (T)obj;

            return true;
        }

        public static void Save(this IPropertySet set)
        {
            // Do nothing. Just a compatibility function.
        }
    }
}
#endif
