using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocell.Library.Twitter.Comparers
{
    public class TwitterResourceCompare : IEqualityComparer<TwitterResource>
    {
        public bool Equals(TwitterResource a, TwitterResource b)
        {
            return (a.User.Key == b.User.Key) && (a.Type == b.Type) && (a.Data == b.Data);
        }
        public int GetHashCode(TwitterResource s)
        {
            return s.User.Key.GetHashCode() * s.String.GetHashCode();
        }
    }
}
