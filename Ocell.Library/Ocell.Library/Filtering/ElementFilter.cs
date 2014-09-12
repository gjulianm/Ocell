using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Ocell.Library.Filtering
{
    public enum ExcludeMode { ExcludeOnMatch, ExcludeOnNoMatch };
    public enum MatchMode { Equals, Contains };

    [KnownType(typeof(HashtagFilter)),
    KnownType(typeof(SourceFilter)),
    KnownType(typeof(TextFilter)),
    KnownType(typeof(UserFilter))]
    public abstract class ElementFilter<T>
    {
        public string Filter { get; set; }
        public DateTime IsValidUntil { get; set; }
        public ExcludeMode Mode { get; set; }
        public MatchMode MatchMode { get; set; }

        public TimeSpan Duration
        {
            get
            {
                return IsValidUntil - DateTime.Now;
            }

            set
            {
                var now = DateTime.Now;

                if (DateTime.MaxValue - now <= value)
                    IsValidUntil = DateTime.MaxValue;
                else
                    IsValidUntil = now + value;
            }
        }

        public abstract string Selector(T item);

        public ElementFilter()
        {
            Mode = ExcludeMode.ExcludeOnMatch;
            MatchMode = MatchMode.Contains;
        }

        public ElementFilter(string filter, ExcludeMode mode, MatchMode matchMode, TimeSpan duration)
        {
            this.Filter = filter;
            this.Mode = mode;
            this.MatchMode = matchMode;
            this.Duration = duration;
        }

        public bool Matches(T item)
        {
            var toCheck = Selector(item);

            if (item == null || Filter == null || toCheck == null)
                return false;

            if (MatchMode == Filtering.MatchMode.Contains)
                return toCheck.Contains(Filter);
            else
                return toCheck.Equals(Filter);
        }

        public bool ExcludeElement(T item)
        {
            if (DateTime.Now > IsValidUntil)
                return false;

            var matches = Matches(item);

            if (Mode == ExcludeMode.ExcludeOnMatch)
                return matches;
            else
                return !matches;
        }
    }

    public static class FilterExtensions
    {
        public static bool ExcludesElement<T>(this IEnumerable<ElementFilter<T>> list, T element)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            return list.Select(x => x.ExcludeElement(element)).Any(x => x == true);
        }
    }
}
