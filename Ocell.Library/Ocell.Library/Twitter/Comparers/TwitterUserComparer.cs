
using System.Collections.Generic;
using TweetSharp;
namespace Ocell.Library.Twitter.Comparers
{
    public class TwitterUserComparer : IComparer<TwitterUser>
    {
        bool compareFullName;

        public TwitterUserComparer(bool compareFullName = false)
        {
            this.compareFullName = compareFullName;
        }

        public int Compare(TwitterUser x, TwitterUser y)
        {
            if (x == null)
                return 1;
            else if (y == null)
                return -1;

            string xKey, yKey;

            if (compareFullName)
            {
                xKey = x.Name;
                yKey = y.Name;
            }
            else
            {
                xKey = x.ScreenName;
                yKey = y.ScreenName;
            }

            return xKey.CompareTo(yKey);
        }
    }
}
