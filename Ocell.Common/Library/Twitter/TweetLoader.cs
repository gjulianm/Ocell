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
        int loaded;
        const int TO_LOAD = 1;
        int requestsInProgress;
        long LastId; 

        public SafeObservable<ITweetable> Source { get; protected set; }

        TwitterResource _resource;
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
                service = ServiceDispatcher.GetService(_resource.User);

                if (conversationService != null)
                    conversationService.Finished -= ConversationFinished;

                conversationService = new ConversationService(_resource.User);
                conversationService.Finished += ConversationFinished;
            }
        }

        bool _isLoading;
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
        
        protected ITwitterService service;
        protected ConversationService conversationService;

        #region Settings
        public int TweetsToLoadPerRequest { get; set; }
        public bool Cached { get; set; }
        public bool ActivateLoadMoreButton { get; set; }
        public bool LoadRetweetsAsMentions { get; set; }
        public int CacheSize { get; set; }
        public bool LoadRTsOnLists { get; set; }
        #endregion       

        #region INotifyPropertyChanged methods
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Constructors
        public TweetLoader(TwitterResource resource, bool cached)
            : this()
        {
            Resource = resource;
            service = ServiceDispatcher.GetService(resource.User);
            conversationService = new ConversationService(resource.User);
            Cached = cached;
        }

        public TweetLoader(TwitterResource resource)
            : this(resource, true)
        {
            LoadCacheAsync();
        }

        public TweetLoader()
        {
            loaded = 0;
            Source = new SafeObservable<ITweetable>();
            LastId = 0;
            requestsInProgress = 0;

            if (_rateResetTime == null)
                _rateResetTime = DateTime.MinValue;

            Error += new OnError(CheckForRateLimit);

            LoadDefaultSettings();
        }

        void LoadDefaultSettings()
        {
            TweetsToLoadPerRequest = 40;
            CacheSize = 40;
            Cached = true;
            ActivateLoadMoreButton = false;
            LoadRTsOnLists = true;
        }

        #endregion

        public void Dispose()
        {
            Source.Clear();
        }

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
            if (service == null)
                service = ServiceDispatcher.GetDefaultService();

            if (Resource == null || service == null ||
                requestsInProgress >= 2 ||
                _rateResetTime > DateTime.Now)
            {
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
                return;
            }

            requestsInProgress++;

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
            loaded++;
            IsLoading = true;

            switch (Resource.Type)
            {
                case ResourceType.Home:
                    service.ListTweetsOnHomeTimeline(TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    service.ListTweetsMentioningMe(TweetsToLoadPerRequest, ReceiveTweets);
                    if (LoadRetweetsAsMentions)
                    {
                        requestsInProgress++;
                        service.ListRetweetsOfMyTweets(TweetsToLoadPerRequest, ReceiveTweets);
                    }
                    break;
                case ResourceType.Messages:
                    requestsInProgress++;
                    service.ListDirectMessagesReceived(TweetsToLoadPerRequest, ReceiveMessages);
                    service.ListDirectMessagesSent(TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    service.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    service.ListTweetsOnList(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), LoadRTsOnLists, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    service.Search(Resource.Data, 1, 20, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    service.ListTweetsOnSpecifiedUserTimeline(Resource.Data, TweetsToLoadPerRequest, true, ReceiveTweets);
                    break;
                case ResourceType.Conversation:
                    conversationService.GetConversationForStatus(Resource.Data, ReceiveTweets);
                    break;
            }
        }

        protected void LoadOld(long last = 0)
        {
            loaded++;

            if (!Source.Any())
                return;
            IsLoading = true;
            if (last == 0)
                last = Source.Min(item => item.Id);

            switch (Resource.Type)
            {
                case ResourceType.Home:
                    service.ListTweetsOnHomeTimelineBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    service.ListTweetsMentioningMeBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    if (LoadRetweetsAsMentions)
                    {
                        requestsInProgress++;
                        service.ListRetweetsOfMyTweetsBefore(last, TweetsToLoadPerRequest, ReceiveTweets);
                    }
                    break;
                case ResourceType.Messages:
                    requestsInProgress++;
                    service.ListDirectMessagesReceivedBefore(last, TweetsToLoadPerRequest, ReceiveMessages);
                    service.ListDirectMessagesSentBefore(last, TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    service.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    service.ListTweetsOnListBefore(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), last, LoadRTsOnLists, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    service.SearchBefore(last, Resource.Data, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    service.ListTweetsOnSpecifiedUserTimelineBefore(Resource.Data, last, true, ReceiveTweets);
                    break;
                case ResourceType.Conversation:
                    conversationService.GetConversationForStatus(Resource.Data, ReceiveTweets);
                    break;
            }
        }
        #endregion

        #region Specific Receivers
        protected void ReceiveTweets(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            requestsInProgress--;
            IsLoading = false;
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
            requestsInProgress--;
            IsLoading = false;
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
            requestsInProgress--;
            IsLoading = false;
            if (result == null || result.Statuses == null)
            {
                if (Error != null)
                    Error(response);
                return;
            }

            GenericReceive(result.Statuses.Cast<ITweetable>(), response);
        }

        private void ConversationFinished(object sender, EventArgs e)
        {
            if (LoadFinished != null)
                LoadFinished(this, e);
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

            if (SourceChanging != null)
                SourceChanging(this, new EventArgs());

            lock (_deferSync)
            {
                if (_deferringRefresh && !_allowOneRefresh)
                {
                    foreach (var item in list)
                        _deferredItems.Enqueue(item);
                }
                else
                {
                    _allowOneRefresh = false;

                    TryAddLoadMoreButton(list);

                    list = list.Except(Source);

                    foreach (var status in list)
                        Source.Add(status);
                }
            }

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
        bool hasLoadMoreButton = false;

        private void TryAddLoadMoreButton(IEnumerable<ITweetable> received)
        {
            if (!ActivateLoadMoreButton || hasLoadMoreButton)
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
            {
                hasLoadMoreButton = true;
                Source.Add(new LoadMoreTweetable { Id = olderTweetReceived.Id - 1 });
            }
        }

        public void RemoveLoadMore()
        {
            hasLoadMoreButton = false;
            ITweetable item = Source.FirstOrDefault(e => e is LoadMoreTweetable);
            while (item != null)
            {
                Source.Remove(item);
                item = Source.FirstOrDefault(e => e is LoadMoreTweetable);
            }

        }
        #endregion

        #region Rate limit checks
        static DateTime _rateResetTime;
        void CheckForRateLimit(TwitterResponse response)
        {
            if (response.RateLimitStatus.RemainingHits <= 0)
                _rateResetTime = response.RateLimitStatus.ResetTime;
        }
        #endregion

        #region Refresh handlers
        Queue<ITweetable> _deferredItems = new Queue<ITweetable>();
        bool _allowOneRefresh = false;
        object _deferSync = new object();
        bool _deferringRefresh = false;

        public void StopSourceRefresh()
        {
            lock (_deferSync)
                _deferringRefresh = true;
        }

        public void ResumeSourceRefresh()
        {
            lock (_deferSync)
            {
                _deferringRefresh = false;

                var list = _deferredItems.AsEnumerable().Except(Source);
                TryAddLoadMoreButton(list);

                foreach(var element in list)
                    Source.Add(element);
            }
        }

        public void AllowNextRefresh()
        {
            lock (_deferSync)
                _allowOneRefresh = true;
        }
        #endregion

        #region Events
        public event EventHandler LoadFinished;

        public delegate void OnError(TwitterResponse response);
        public event OnError Error;

        public event EventHandler PartialLoad;

        public event EventHandler CacheLoad;

        public event EventHandler SourceChanging;
        #endregion
    }


}
