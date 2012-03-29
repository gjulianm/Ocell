using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using TweetSharp;
using Ocell.Library;
using System.Threading;

namespace Ocell.Library
{
    public class TweetLoader : IDisposable
    {
        private int _loaded;
        private const int ToLoad = 1;
        public int TweetsToLoadPerRequest { get; set; }
        public bool Cached { get; set; }
        private int _requestsInProgress;
        private TwitterResource _resource;
        private static DateTime _rateResetTime;

        public TwitterResource Resource
        {
            get
            {
                return _resource;
            }
            set
            {
                _resource = value;
                Srv = ServiceDispatcher.GetService(_resource.User);
            }
        }
        public ObservableCollection<ITweetable> Source { get; protected set; }
        protected TwitterService Srv;
        protected long LastId;

        #region Constructors
        public TweetLoader(TwitterResource resource, bool cached)
            : this()
        {
            Resource = resource;
            Srv = ServiceDispatcher.GetService(resource.User);
            Cached = cached;
        }

        public TweetLoader(TwitterResource resource)
            : this(resource, true)
        {
            LoadCacheAsync();
        }

        public TweetLoader()
        {
            TweetsToLoadPerRequest = 25;
            _loaded = 0;
            Source = new ObservableCollection<TweetSharp.ITweetable>();
            LastId = 0;
            Cached = true;
            _requestsInProgress = 0;
            if (_rateResetTime == null)
                _rateResetTime = DateTime.MinValue;

            Error += new OnError(CheckForRateLimit);
        }

        void CheckForRateLimit(TwitterResponse response)
        {
            if (response.RateLimitStatus.RemainingHits <= 0)
                _rateResetTime = response.RateLimitStatus.ResetTime;
        }
        #endregion

        public void LoadCacheAsync()
        {
            Thread loaderThread = new Thread(new ThreadStart(LoadCache));
            loaderThread.Start();
        }

        protected void ReceiveCallback(IAsyncResult result)
        {
        }

        #region Loaders
        public void Load(bool Old = false)
        {
            if (Srv == null ||
                _requestsInProgress >= 2 ||
                _rateResetTime > DateTime.Now)
            {
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
                return;
            }

            _requestsInProgress++;

            if (Old)
                LoadOld();
            else
                LoadNew();
        }
        protected void LoadNew()
        {
            _loaded++;
            switch (Resource.Type)
            {
                case ResourceType.Home:
                    Srv.ListTweetsOnHomeTimeline(TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    Srv.ListTweetsMentioningMe(TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Messages:
                    Srv.ListDirectMessagesReceived(TweetsToLoadPerRequest, ReceiveMessages);
                    Srv.ListDirectMessagesSent(TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    Srv.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    Srv.ListTweetsOnList(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), ReceiveTweets);
                    break;
                case ResourceType.Search:
                    Srv.Search(Resource.Data, 1, 20, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    Srv.ListTweetsOnSpecifiedUserTimeline(Resource.Data, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
            }
        }
        protected void LoadOld(long last = 0)
        {
            _loaded++;
            if (Source.Count == 0)
            {
                return;
            }
            if (last == 0)
                last = LastId;
            switch (Resource.Type)
            {
                case ResourceType.Home:
                    Srv.ListTweetsOnHomeTimelineBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    Srv.ListTweetsMentioningMeBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Messages:
                    Srv.ListDirectMessagesReceivedBefore(last, TweetsToLoadPerRequest, ReceiveMessages);
                    Srv.ListDirectMessagesSentBefore(last, TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    Srv.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    Srv.ListTweetsOnListBefore(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), last, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    Srv.SearchBefore(last, Resource.Data, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    Srv.ListTweetsOnSpecifiedUserTimelineBefore(Resource.Data, last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
            }
        }

        public void LoadCache()
        {
            if (!Cached || Resource.User == null)
                return;

            TweetEqualityComparer comparer = new TweetEqualityComparer();
            IEnumerable<TwitterStatus> cacheList = Cacher.GetFromCache(Resource).OrderByDescending(item => item.Id);

            if (!DecisionMaker.ShouldLoadCache(ref cacheList))
                return;


            foreach (var item in cacheList)
                if (!Source.Contains(item, comparer))
                    Source.Add(item);

            if (CacheLoad != null)
                CacheLoad(this, new EventArgs());
        }
        #endregion

        #region Specific Receivers
        protected void ReceiveTweets(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            _requestsInProgress--;
            if (statuses == null)
            {
                if (Error != null)
                    Error(response);
                return;
            }
            List<ITweetable> list = new List<ITweetable>();
            foreach (var item in statuses)
                list.Add(item);
            GenericReceive((IEnumerable<ITweetable>)list, response);
        }

        protected void ReceiveMessages(IEnumerable<TwitterDirectMessage> statuses, TwitterResponse response)
        {
            _requestsInProgress--;
            if (statuses == null)
            {
                if (Error != null)
                    Error(response);
                return;
            }
            List<ITweetable> list = new List<ITweetable>();
            foreach (var item in statuses)
                list.Add(item);
            GenericReceive((IEnumerable<ITweetable>)list, response);
        }

        protected void ReceiveSearch(TwitterSearchResult result, TwitterResponse response)
        {
            _requestsInProgress--;
            if (result == null || result.Statuses == null)
            {
                if (Error != null)
                    Error(response);
                return;
            }
            List<ITweetable> list = new List<ITweetable>();
            foreach (var item in result.Statuses)
                list.Add(item);
            GenericReceive((IEnumerable<ITweetable>)list, response);
        }
        #endregion

        protected void GenericReceive(IEnumerable<ITweetable> list, TwitterResponse response)
        {
            try
            {
                UnsafeGenericReceive(list, response);
            }
            catch (Exception)
            {
            }
        }
        private void UnsafeGenericReceive(IEnumerable<ITweetable> list, TwitterResponse response)
        {
            TweetEqualityComparer comparer = new TweetEqualityComparer();

            if (list == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (Error != null)
                    Error(response);
                return;
            }

            if (Source == null)
                Source = new ObservableCollection<ITweetable>();

            foreach (var status in list)
                if (!Source.Contains(status, comparer))
                    Source.Add(status);

            OrderSource();

            if (list.Any())
                LastId = list.Last().Id;

            if (_loaded < ToLoad && list.Any())
            {
                LoadOld(LastId);
                if (PartialLoad != null)
                    PartialLoad(this, new EventArgs());
            }
            else
            {
                _loaded = 0;
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
            }

            SaveToCache();
        }

        public void SaveToCache()
        {
            if (!Cached)
                return;

            if (Source.Count == 0)
                return;

            if (Source.First().GetType() == typeof(TwitterStatus))
            {
                try
                {
                    Cacher.SaveToCache(Resource, Source.OrderByDescending(item => item.Id).Cast<TwitterStatus>().Take(30));
                }
                catch (Exception)
                {
                    //Do nothing.
                }
            }
        }

        protected void OrderSource()
        {
            Source = new ObservableCollection<ITweetable>(Source.OrderByDescending(item => item.Id));
        }

        public void Dispose()
        {
            Source.Clear();
        }

        #region Events
        public event EventHandler LoadFinished;

        public delegate void OnError(TwitterResponse response);
        public event OnError Error;

        public event EventHandler PartialLoad;

        public event EventHandler CacheLoad;
        #endregion
    }

}
