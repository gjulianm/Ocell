using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TweetSharp;

namespace Ocell.Library.Filtering
{
    public static class FilterManager
    {
        public static void SubscribeToFilterChanges(TwitterResource resource, NotifyCollectionChangedEventHandler callback)
        {
            var localFilterCollection = Config.Filters.Value.GetOrCreate(resource);
            localFilterCollection.CollectionChanged += callback;

            var globalCollection = Config.GlobalFilter.Value;
            globalCollection.CollectionChanged += callback;
        }

        public static IEnumerable<ElementFilter<ITweetable>> GetFiltersFor(TwitterResource resource)
        {
            return Config.Filters.Value.GetOrCreate(resource).Concat(Config.GlobalFilter.Value).ToList();
        }

        public static void AddFilterFor(TwitterResource resource, ElementFilter<ITweetable> filter)
        {
            Config.Filters.Value.GetOrCreate(resource).Add(filter);
        }

        public static void AddGlobalFilter(ElementFilter<ITweetable> filter)
        {
            Config.GlobalFilter.Value.Add(filter);
        }

        public static ElementFilter<ITweetable> CreateAndAddFilterFor(TwitterResource resource, string filterString, Func<string, TimeSpan, ElementFilter<ITweetable>> creator)
        {
            var filter = creator(filterString, Config.DefaultMuteTime.Value ?? TimeSpan.FromHours(8));
            AddFilterFor(resource, filter);
            return filter;
        }

        public static ElementFilter<ITweetable> CreateAndAddGlobalFilter(string filterString, Func<string, TimeSpan, ElementFilter<ITweetable>> creator)
        {
            var filter = creator(filterString, Config.DefaultMuteTime.Value ?? TimeSpan.FromHours(8));
            AddGlobalFilter(filter);
            return filter;
        }
    }
}
