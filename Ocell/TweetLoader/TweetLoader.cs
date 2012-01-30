using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using TweetSharp;

namespace Ocell
{
    public class TweetLoader
    {
        private int Loaded;
        private readonly int ToLoad = 1;
        private readonly int Count = 20;
        public TwitterResource Resource { get; set; }
        public ObservableCollection<ITweetable> Source { get; protected set; }
        protected long LastId;
        
        #region Constructors
        public TweetLoader(TwitterResource resource)
        {
            Resource = resource;
            Loaded = 0;
            Source = new ObservableCollection<TweetSharp.ITweetable>();
            LastId = 0;
        }

        
        public TweetLoader()
        {
            Loaded = 0;
            Source = new ObservableCollection<TweetSharp.ITweetable>();
            LastId = 0;
        }
        #endregion

        #region Loaders
        public void Load(bool Old = false)
        {
            if (Clients.Service == null || !Clients.isServiceInit || Resource == null)
                throw new Exception("Client not initialised.");

            if (Old)
                LoadOld();
            else
                LoadNew();
        }
        protected void LoadNew()
        {
            Loaded++;
            switch (Resource.Type)
            {
                case ResourceType.Home:
                    Clients.Service.ListTweetsOnHomeTimeline(Count, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    Clients.Service.ListTweetsMentioningMe(Count, ReceiveTweets);
                    break;
                case ResourceType.Messages:
                    Clients.Service.ListDirectMessagesReceived(Count, ReceiveMessages);
                    Clients.Service.ListDirectMessagesSent(Count, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    Clients.Service.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    Clients.Service.ListTweetsOnList(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), ReceiveTweets);
                    break;
                case ResourceType.Search:
                    Clients.Service.Search(Resource.Data, 1, 20, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    Clients.Service.ListTweetsOnSpecifiedUserTimeline(Resource.Data, Count, ReceiveTweets);
                    break;
            }
        }
        protected void LoadOld(long Last = 0)
        {
            Loaded++;
            if (Source.Count == 0)
            {
                return;
            }
            if(Last == 0)
            	Last = LastId;
            switch (Resource.Type)
            {
                case ResourceType.Home:
                    Clients.Service.ListTweetsOnHomeTimelineBefore(Last, Count, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    Clients.Service.ListTweetsMentioningMeBefore(Last, Count, ReceiveTweets);
                    break;
                case ResourceType.Messages:
                    Clients.Service.ListDirectMessagesReceivedBefore(Last, Count, ReceiveMessages);
                    Clients.Service.ListDirectMessagesSentBefore(Last, Count, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    Clients.Service.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    Clients.Service.ListTweetsOnListBefore(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), Last, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    Clients.Service.SearchBefore(Last, Resource.Data, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    Clients.Service.ListTweetsOnSpecifiedUserTimelineBefore(Resource.Data, Last, Count, ReceiveTweets);
                    break;
            }
        }
        public void LoadCache()
        {
            TweetEqualityComparer comparer = new TweetEqualityComparer();
            foreach (var item in Cacher.GetFromCache(Resource))
                if (!Source.Contains(item, comparer))
                    Source.Add(item);

            if(CacheLoad != null)
                CacheLoad();
        }
        #endregion

        #region Specific Receivers
        protected void ReceiveTweets(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            if (statuses == null)
                return;
            List<ITweetable> list = new List<ITweetable>();
            foreach (var item in statuses)
                list.Add(item);
            GenericReceive((IEnumerable<ITweetable>)list, response);
        }

        protected void ReceiveMessages(IEnumerable<TwitterDirectMessage> statuses, TwitterResponse response)
        {
            if (statuses == null)
                return;
            List<ITweetable> list = new List<ITweetable>();
            foreach (var item in statuses)
                list.Add(item);
            GenericReceive((IEnumerable<ITweetable>)list, response);
        }

        protected void ReceiveSearch(TwitterSearchResult result, TwitterResponse response)
        {
            if (result == null || result.Statuses == null)
                return;
            List<ITweetable> list = new List<ITweetable>();
            foreach (var item in result.Statuses)
                list.Add(item);
            GenericReceive((IEnumerable<ITweetable>)list, response);
        } 
        #endregion

        protected void GenericReceive(IEnumerable<ITweetable> list, TwitterResponse response)
        {
            TweetEqualityComparer comparer = new TweetEqualityComparer();
            
            if (list == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (Error != null)
                    Error(response);
                return;
            }

            foreach (var status in list)
                if (!Source.Contains(status, comparer))
                    Source.Add(status);

            LastId = list.Last().Id;

            if (Loaded < ToLoad && list.Count() > 0)
            {
                LoadOld(list.Last().Id);
                if (PartialLoad != null)
                    PartialLoad();
            }
            else
            {
                Loaded = 0;
                if (LoadFinished != null)
                    LoadFinished();
            }

            SaveToCache();
        }

        public void SaveToCache()
        {
            if (Source.First().GetType() == typeof(TwitterStatus))
            {
                Cacher.SaveToCache(Resource, Source.Cast<TwitterStatus>().Take(30));
            }
        }

        #region Events
        public delegate void OnLoadFinished();
        public event OnLoadFinished LoadFinished;

        public delegate void OnError(TwitterResponse response);
        public event OnError Error;

        public delegate void OnPartialLoad();
        public event OnPartialLoad PartialLoad;

        public delegate void OnCacheLoad();
        public event OnCacheLoad CacheLoad;
        #endregion
    } 
        
}
