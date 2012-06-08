using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using TweetSharp;
using Ocell.Library;
using System.Threading;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Ocell.Library.Twitter
{
    public class TweetLoader : INotifyPropertyChanged, IDisposable
    {
        private int _loaded;
        private const int _toLoad = 1;
        public int TweetsToLoadPerRequest { get; set; }
        public bool Cached { get; set; }
        public bool ActivateLoadMoreButton { get; set; }
        public bool LoadRetweetsAsMentions { get; set; }
        public int CacheSize { get; set; }
        private int _requestsInProgress;
        protected TwitterResource _resource;
        private static DateTime _rateResetTime;
        private ConversationService _conversationService;

        public event PropertyChangedEventHandler PropertyChanged;

        public TwitterResource Resource
        {
            get
            {
                return _resource;
            }
            set
            {
                if (value == _resource)
                    return;

                _resource = value;
                _srv = ServiceDispatcher.GetService(_resource.User);
                _conversationService = new ConversationService(_resource.User);
                _conversationService.Finished += ConversationFinished;
            }
        }
        public SafeObservable<ITweetable> Source { get; protected set; }
        protected ITwitterService _srv;
        protected long LastId;

        protected bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            protected set
            {
                if (value == _isLoading)
                    return;
                _isLoading = value;
                var dispatcher = Deployment.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                    OnPropertyChanged("IsLoading");
                else
                    dispatcher.BeginInvoke(() => OnPropertyChanged("IsLoading"));
            }
        }

        protected void OnPropertyChanged(string propName)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region Constructors
        public TweetLoader(TwitterResource resource, bool cached)
            : this()
        {
            Resource = resource;
            _srv = ServiceDispatcher.GetService(resource.User);
            _conversationService = new ConversationService(resource.User);
            Cached = cached;
        }

        public TweetLoader(TwitterResource resource)
            : this(resource, true)
        {
            LoadCacheAsync();
        }

        public TweetLoader()
        {
            TweetsToLoadPerRequest = 40;
            CacheSize = 40;
            _loaded = 0;
            Source = new SafeObservable<ITweetable>();
            LastId = 0;
            Cached = true;
            ActivateLoadMoreButton = false;
            _requestsInProgress = 0;
            if (_rateResetTime == null)
                _rateResetTime = DateTime.MinValue;

            Error += new OnError(CheckForRateLimit);
        }
        #endregion

        #region Cache
        private void SaveToCacheThreaded()
        {
            ThreadPool.QueueUserWorkItem((threadContext) => SaveToCache());
        }
        public void LoadCacheAsync()
        {
            ThreadPool.QueueUserWorkItem((threadContext) =>
                {
                    LoadCache();
                });
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
                    Cacher.SaveToCache(Resource, Source.OrderByDescending(item => item.Id).Cast<TwitterStatus>().Take(CacheSize));
                }
                catch (Exception)
                {
                    //Do nothing.
                }
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

        #region Loaders
        public void Load(bool Old = false)
        {
            if (Resource == null || _srv == null ||
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
        public void LoadFrom(long Id)
        {
            LoadOld(Id);
        }
        protected void LoadNew()
        {
            _loaded++;
            IsLoading = true;
            switch (Resource.Type)
            {
                case ResourceType.Home:
                    _srv.ListTweetsOnHomeTimeline(TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    _srv.ListTweetsMentioningMe(TweetsToLoadPerRequest, ReceiveTweets);
                    if (LoadRetweetsAsMentions)
                    {
                        _requestsInProgress++;
                        _srv.ListRetweetsOfMyTweets(TweetsToLoadPerRequest, ReceiveTweets);
                    }
                    break;
                case ResourceType.Messages:
                    _requestsInProgress++;
                    _srv.ListDirectMessagesReceived(TweetsToLoadPerRequest, ReceiveMessages);
                    _srv.ListDirectMessagesSent(TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    _srv.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    _srv.ListTweetsOnList(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    _srv.Search(Resource.Data, 1, 20, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    _srv.ListTweetsOnSpecifiedUserTimeline(Resource.Data, TweetsToLoadPerRequest, true, ReceiveTweets);
                    break;
                case ResourceType.Conversation:
                    _conversationService.GetConversationForStatus(Resource.Data, ReceiveTweets);
                    break;
            }
        }
        protected void LoadOld(long last = 0)
        {
            _loaded++;

            if (!Source.Any())
                return;
            IsLoading = true;
            if (last == 0)
                last = Source.Min(item => item.Id);

            switch (Resource.Type)
            {
                case ResourceType.Home:
                    _srv.ListTweetsOnHomeTimelineBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    _srv.ListTweetsMentioningMeBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    if (LoadRetweetsAsMentions)
                    {
                        _requestsInProgress++;
                        _srv.ListRetweetsOfMyTweetsBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    }
                    break;
                case ResourceType.Messages:
                    _requestsInProgress++;
                    _srv.ListDirectMessagesReceivedBefore(last, TweetsToLoadPerRequest, ReceiveMessages);
                    _srv.ListDirectMessagesSentBefore(last, TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    _srv.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    _srv.ListTweetsOnListBefore(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    _srv.SearchBefore(last, Resource.Data, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    _srv.ListTweetsOnSpecifiedUserTimelineBefore(Resource.Data, last, true, ReceiveTweets);
                    break;
                case ResourceType.Conversation:
                    _conversationService.GetConversationForStatus(Resource.Data, ReceiveTweets);
                    break;
            }
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

            GenericReceive(statuses.Cast<ITweetable>(), response);
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

            GenericReceive(statuses.Cast<ITweetable>(), response);
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

            GenericReceive(result.Statuses.Cast<ITweetable>(), response);
        }
        #endregion

        #region Generic Receivers
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

            IsLoading = false;
            if (list == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (Error != null)
                    Error(response);
                return;
            }

            if (Source == null)
                Source = new SafeObservable<ITweetable>();

            TryAddLoadMoreButton(ref list);

            foreach (var status in list)
                if (!Source.Contains(status, comparer))
                    Source.Add(status);

            if (list.Any())
                LastId = list.Min(item => item.Id);


            if (LoadFinished != null && _resource.Type != ResourceType.Conversation)
                LoadFinished(this, new EventArgs());
            else if (PartialLoad != null && _resource.Type == ResourceType.Conversation)
                PartialLoad(this, new EventArgs()); // When loading conversations, calls to this function will be partial.


            SaveToCacheThreaded();
        }
        #endregion

        #region Load more button
        private void TryAddLoadMoreButton(ref IEnumerable<ITweetable> received)
        {
            if (!ActivateLoadMoreButton)
                return;

            if (Source == null || !Source.Any())
                return;

            ITweetable olderTweetReceived;
            ITweetable nextTweet;

            olderTweetReceived = received.OrderBy(item => item.Id).FirstOrDefault();

            if (olderTweetReceived == null)
                return;

            nextTweet = Source.FirstOrDefault(item => item.Id < olderTweetReceived.Id);

            if (nextTweet == null)
                return;

            int avgTime = DecisionMaker.GetAvgTimeBetweenTweets(Source);
            TimeSpan diff = olderTweetReceived.CreatedDate - nextTweet.CreatedDate;

            if (diff.TotalSeconds > 4 * avgTime)
                Source.Add(new LoadMoreTweetable { Id = olderTweetReceived.Id - 1 });
        }
        public void RemoveLoadMore()
        {
            ITweetable item = Source.FirstOrDefault(e => e is LoadMoreTweetable);
            while (item != null)
            {
                Source.Remove(item);
                item = Source.FirstOrDefault(e => e is LoadMoreTweetable);
            }

        }
        #endregion

        void CheckForRateLimit(TwitterResponse response)
        {
            if (response.RateLimitStatus.RemainingHits <= 0)
                _rateResetTime = response.RateLimitStatus.ResetTime;
        }

        protected void OrderSource()
        {
            Source = new SafeObservable<ITweetable>(Source.OrderByDescending(item => item.Id));
        }

        public void Dispose()
        {
            Source.Clear();
        }

        private void ConversationFinished(object sender, EventArgs e)
        {
            if (LoadFinished != null)
                LoadFinished(this, e);
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
