using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;

namespace Ocell.Library.Filtering
{
    public class HashtagFilter : ElementFilter<ITweetable>
    {
        public override string Selector(ITweetable item)
        {
            return string.Join("", (item.Entities.HashTags ?? new List<TwitterHashTag>()).Select(h => h.Text));
        }

        public HashtagFilter()
            : base()
        {
        }

        public HashtagFilter(string filter, ExcludeMode mode, TimeSpan duration)
            : base(filter,
                mode,
                MatchMode.Contains,
                duration)
        {
        }

        public HashtagFilter(string filter, TimeSpan duration)
            : base(filter,
                ExcludeMode.ExcludeOnMatch,
                MatchMode.Contains,
                duration)
        {
        }

        public static Func<string, TimeSpan, HashtagFilter> Creator = (f, t) => new HashtagFilter(f, t);
    }

    public class SourceFilter : ElementFilter<ITweetable>
    {
        public override string Selector(ITweetable item)
        {
            return item is TwitterStatus ? ((TwitterStatus)item).Source : "";
        }

        public SourceFilter()
            : base()
        {
        }

        public SourceFilter(string filter, ExcludeMode mode, TimeSpan duration)
            : base(filter,
               mode,
               MatchMode.Contains,
               duration)
        {
        }

        public SourceFilter(string filter, TimeSpan duration)
            : base(filter,
               ExcludeMode.ExcludeOnMatch,
               MatchMode.Contains,
               duration)
        {
        }

        public static Func<string, TimeSpan, SourceFilter> Creator = (f, t) => new SourceFilter(f, t);
    }

    public class TextFilter : ElementFilter<ITweetable>
    {
        public override string Selector(ITweetable item)
        {
            return item.Text;
        }

        public TextFilter()
            : base()
        {
        }

        public TextFilter(string filter, ExcludeMode mode, TimeSpan duration)
            : base(filter,
               mode,
               MatchMode.Contains,
               duration)
        {
        }

        public TextFilter(string filter, TimeSpan duration)
            : base(filter,
               ExcludeMode.ExcludeOnMatch,
               MatchMode.Contains,
               duration)
        {
        }

        public static Func<string, TimeSpan, TextFilter> Creator = (f, t) => new TextFilter(f, t);
    }

    public class UserFilter : ElementFilter<ITweetable>
    {
        public override string Selector(ITweetable item)
        {
            return item.AuthorName;
        }

        public UserFilter()
            : base()
        {
        }

        public UserFilter(string filter, ExcludeMode mode, TimeSpan duration)
            : base(filter,
               mode,
               MatchMode.Equals,
               duration)
        {
        }

        public UserFilter(string filter, TimeSpan duration)
            : base(filter,
               ExcludeMode.ExcludeOnMatch,
               MatchMode.Equals,
               duration)
        {
        }

        public static Func<string, TimeSpan, UserFilter> Creator = (f, t) => new UserFilter(f, t);
    }
}
