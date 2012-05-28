using System.Collections.Generic;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.Library.Twitter.Comparers
{
    public class TweetEqualityComparer : IEqualityComparer<ITweetable>
    {
        public bool Equals(ITweetable s1, ITweetable s2)
        {
            if (s1 == null || s2 == null)
                return false;

            return (s1.Id == s2.Id);
        }

        public int GetHashCode(ITweetable s)
        {
            if (s != null)
                return s.Id.GetHashCode();
            else
                return 0;
        }
    }

    public class TwitterStatusEqualityComparer : IEqualityComparer<TwitterStatus>
    {
        public bool Equals(TwitterStatus s1, TwitterStatus s2)
        {
            if (s1 == null || s2 == null)
                return false;

            return (s1.Id == s2.Id);
        }

        public int GetHashCode(TwitterStatus s)
        {
            if (s != null)
                return s.Id.GetHashCode();
            else
                return 0;
        }
    }

    public class TweetComparer : IComparer<ITweetable>
    {
        public int Compare(ITweetable a, ITweetable b)
        {
            if (a.Id > b.Id)
                return -1;
            else if (a.Id < b.Id)
                return 1;
            else
                return 0;
        }
    }

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