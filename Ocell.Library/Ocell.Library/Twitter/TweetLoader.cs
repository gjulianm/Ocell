using AncoraMVVM.Base.Collections;
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Base;
using Ocell.Library.Twitter.Comparers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TweetSharp;
using AncoraMVVM.Base.Diagnostics;

namespace Ocell.Library.Twitter
{
    public class TweetLoader : INotifyPropertyChanged
    {
        #region Properties
        public SortedFilteredObservable<ITweetable> Source { get; protected set; }

        private int requestsInProgress;
        public int RequestsInProgress
        {
            get
            {
                return requestsInProgress;
            }

            set
            {
                bool propertyChanges = value > 0 != IsLoading;

                requestsInProgress = value;

                if (propertyChanges)
                {
                    OnPropertyChanged("IsLoading");
                }
            }
        }

        TwitterResource resource;
        public TwitterResource Resource
        {
            get
            {
                return resource;
            }
            set
            {
                if (value == resource)
                    return;

                resource = value;
                RefreshServices();
            }
        }

        public bool IsLoading { get { return RequestsInProgress > 0; } }
        #endregion

        #region Settings
        public int TweetsToLoadPerRequest { get; set; }
        public bool Cached { get; set; }
        public bool ActivateLoadMoreButton { get; set; }
        public bool LoadRetweetsAsMentions { get; set; }
        public int CacheSize { get; set; }
        public bool LoadRTsOnLists { get; set; }
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
            Source = new SortedFilteredObservable<ITweetable>(new TweetComparer());
            Source.SortOrder = SortOrder.Descending;
            RequestsInProgress = 0;

            if (rateResetTime == null)
                rateResetTime = DateTime.MinValue;

            Error += new OnError(CheckForRateLimit);

            LoadDefaultSettings();
        }

        void LoadDefaultSettings()
        {
            TweetsToLoadPerRequest = 40;
            CacheSize = 30;
            Cached = true;
            ActivateLoadMoreButton = false;
            LoadRTsOnLists = true;
        }

        #endregion

        private void ConversationFinished(object sender, EventArgs e)
        {
            if (LoadFinished != null)
                LoadFinished(this, e);
        }

        public void Dispose()
        {
            Source.Clear();
        }

        #region Services
        protected ITwitterService service;
        protected ConversationService conversationService;

        private void RefreshServices()
        {
            service = ServiceDispatcher.GetService(Resource.User);

            if (conversationService != null)
                conversationService.Finished -= ConversationFinished;

            conversationService = new ConversationService(Resource.User);
            conversationService.Finished += ConversationFinished;
        }
        #endregion

        #region Cache
        public void SaveToCacheAsync()
        {
            Task.Factory.StartNew(SaveToCache);
        }

        public void LoadCacheAsync()
        {
            Task.Factory.StartNew(LoadCache);
        }

        public void DeferredCacheLoad()
        {
            new Timer((context) => LoadCache(), null, 1000, Timeout.Infinite);
        }

        public void SaveToCacheAsync(IList<ITweetable> viewport)
        {
            Task.Factory.StartNew(() => SaveToCache(viewport));
        }

        public void SaveToCache(IList<ITweetable> viewport)
        {
            if (!Cached)
                return;

            var toSave = viewport
                .Union(Source.OrderByDescending(x => x.Id).Take(CacheSize))
                .OfType<TwitterStatus>()
                .OrderByDescending(x => x.Id);

            try
            {
                Cacher.SaveToCache(Resource, toSave);
            }
            catch (Exception)
            {
            }
        }

        public void SaveToCache()
        {
            if (!Cached || Source.Count == 0)
                return;

            if (Source.First().GetType() == typeof(TwitterStatus))
            {
                try
                {
                    Cacher.SaveToCache(Resource, Source.OrderByDescending(item => item.Id).OfType<TwitterStatus>().Take(CacheSize));
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

            IEnumerable<TwitterStatus> cacheList = Cacher.GetFromCache(Resource);
            var toAdd = AddLoadMoreButtons(cacheList.OrderByDescending(x => x.Id).Cast<ITweetable>()).Except(Source);

            Source.AddRange(toAdd);

            if (CacheLoad != null)
                CacheLoad(this, new EventArgs());
        }
        #endregion

        #region Loaders
        public void Load(bool getOld = false)
        {
            long? lastId = null;

            if (getOld)
            {
                if (!Source.Any())
                    return;
                else
                    lastId = Source.Min(item => item.Id);
            }

            LoadFrom(lastId);
        }

        public void LoadFrom(long? lastId)
        {
            if (service == null)
                service = ServiceDispatcher.GetDefaultService();

            if (Resource == null || service == null ||
                IsLoading ||
                rateResetTime > DateTime.Now)
            {
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
                return;
            }

            SetTaskReceivers(GetTweetTasks(Resource.Type, lastId), TweetsToITweetable);
            SetTaskReceivers(GetSearchTasks(Resource.Type, lastId), SearchToITweetable);
            SetTaskReceivers(GetDirectMessageTasks(Resource.Type, lastId), DMsToITweetable);

            if (Resource.Type == ResourceType.Conversation)
                conversationService.GetConversationForStatus(Resource.Data, ReceiveConversation);
        }

        protected IEnumerable<ITweetable> TweetsToITweetable(IEnumerable<TwitterStatus> statuses)
        {
            return statuses.Cast<ITweetable>();
        }

        protected IEnumerable<ITweetable> DMsToITweetable(IEnumerable<TwitterDirectMessage> dms)
        {
            if (Resource.Type == ResourceType.Messages && !string.IsNullOrWhiteSpace(Resource.Data))
                return dms.Where(x => x.SenderScreenName == Resource.Data || x.RecipientScreenName == Resource.Data).Cast<ITweetable>();
            else
                return dms.Cast<ITweetable>();
        }

        protected IEnumerable<ITweetable> SearchToITweetable(TwitterSearchResult search)
        {
            return search.Statuses.Cast<ITweetable>();
        }

        protected void SetTaskReceivers<T>(IEnumerable<Task<TwitterResponse<T>>> tasks, Func<T, IEnumerable<ITweetable>> toITweetableFunc)
        {
            foreach (var task in tasks)
            {
                RequestsInProgress++;
                task.ContinueWith((t) => ReceiveResponse(task, toITweetableFunc));
            }
        }

        protected IEnumerable<Task<TwitterResponse<IEnumerable<TwitterStatus>>>> GetTweetTasks(ResourceType type, long? last)
        {
            switch (Resource.Type)
            {
                case ResourceType.Home:
                    yield return service.ListTweetsOnHomeTimelineAsync(new ListTweetsOnHomeTimelineOptions
                    {
                        Count = TweetsToLoadPerRequest,
                        IncludeEntities = true,
                        MaxId = last
                    });
                    break;
                case ResourceType.Mentions:
                    yield return service.ListTweetsMentioningMeAsync(new ListTweetsMentioningMeOptions
                    {
                        Count = TweetsToLoadPerRequest,
                        IncludeEntities = true,
                        MaxId = last
                    });

                    if (LoadRetweetsAsMentions)
                    {
                        yield return service.ListRetweetsOfMyTweetsAsync(new ListRetweetsOfMyTweetsOptions
                        {
                            IncludeUserEntities = true,
                            Count = TweetsToLoadPerRequest,
                            MaxId = last
                        });
                    }
                    break;
                case ResourceType.Favorites:
                    yield return service.ListFavoriteTweetsAsync(new ListFavoriteTweetsOptions
                    {
                        Count = TweetsToLoadPerRequest,
                        MaxId = last
                    });
                    break;
                case ResourceType.List:
                    yield return service.ListTweetsOnListAsync(new ListTweetsOnListOptions
                    {
                        IncludeRts = LoadRTsOnLists,
                        Count = TweetsToLoadPerRequest,
                        OwnerScreenName = Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                        Slug = Resource.Data.Substring(Resource.Data.IndexOf('/') + 1),
                        MaxId = last
                    });
                    break;
                case ResourceType.Tweets:
                    yield return service.ListTweetsOnUserTimelineAsync(new ListTweetsOnUserTimelineOptions
                    {
                        Count = TweetsToLoadPerRequest,
                        ScreenName = Resource.Data,
                        IncludeRts = true,
                        MaxId = last
                    });
                    break;
            }
        }

        protected IEnumerable<Task<TwitterResponse<IEnumerable<TwitterDirectMessage>>>> GetDirectMessageTasks(ResourceType type, long? last)
        {
            if (type == ResourceType.Messages || type == ResourceType.MessageConversation)
            {
                yield return service.ListDirectMessagesReceivedAsync(new ListDirectMessagesReceivedOptions { Count = TweetsToLoadPerRequest, MaxId = last });
                yield return service.ListDirectMessagesSentAsync(new ListDirectMessagesSentOptions { Count = TweetsToLoadPerRequest, MaxId = last });
            }
        }

        protected IEnumerable<Task<TwitterResponse<TwitterSearchResult>>> GetSearchTasks(ResourceType type, long? last)
        {
            if (type == ResourceType.Search)
            {
                yield return service.SearchAsync(new SearchOptions { Count = TweetsToLoadPerRequest, IncludeEntities = true, Q = Resource.Data, MaxId = last });
            }
        }
        #endregion

        #region Receivers
        protected void ReceiveResponse<T>(Task<TwitterResponse<T>> task, Func<T, IEnumerable<ITweetable>> toITweetable)
        {
            RequestsInProgress--;

            if (task.IsFaulted)
            {
                Debug.WriteLine("Task faulted: {0}", task.Exception);
                return;
            }

            var response = task.Result;

            if (!response.RequestSucceeded)
            {
                if (Error != null)
                    Error(response);
                return;
            }

            GenericReceive(toITweetable(response.Content).ToList());
        }

        private void GenericReceive(IEnumerable<ITweetable> list)
        {
            try
            {
                UnsafeGenericReceive(list);
            }
            catch (Exception ex)
            {
                AncoraLogger.Instance.LogException("Error adding tweetables to app", ex);
            }
        }

        private void UnsafeGenericReceive(IEnumerable<ITweetable> list)
        {
            TweetEqualityComparer comparer = new TweetEqualityComparer();

            if (SourceChanging != null)
                SourceChanging(this, new EventArgs());

            if (list.Any())
            {
                if (list.FirstOrDefault() is TwitterDirectMessage
                    && (Resource.Type == ResourceType.Messages && string.IsNullOrWhiteSpace(Resource.Data)))
                    LoadMessages(list.OfType<TwitterDirectMessage>());
                else
                    LoadTweetables(list);
            }

            if (LoadFinished != null && Resource.Type != ResourceType.Conversation)
                LoadFinished(this, new EventArgs());
            else if (PartialLoad != null && Resource.Type == ResourceType.Conversation)
                PartialLoad(this, new EventArgs()); // When loading conversations, calls to this function will be partial.

            SaveToCacheAsync();
        }

        private void LoadTweetables(IEnumerable<ITweetable> list)
        {
            if (Resource.Type != ResourceType.MessageConversation)
                TryAddLoadMoreButton(list);
            else
                list = list.Cast<TwitterDirectMessage>().Where(x => x.SenderScreenName == Resource.Data || x.RecipientScreenName == Resource.Data).Cast<ITweetable>();

            list = list.Except(Source);

            Dependency.Resolve<IDispatcher>().InvokeIfRequired(() => Source.AddListRange(list));
        }

        private void LoadMessages(IEnumerable<TwitterDirectMessage> messages)
        {
            var groups = Source.OfType<GroupedDM>();
            Dependency.Resolve<IDispatcher>().InvokeIfRequired(() =>
            {
                foreach (var msg in messages)
                {
                    var pairId = msg.GetPairName(Resource.User);
                    var group = groups.FirstOrDefault(x => x.ConverserNames.Item1 == pairId || x.ConverserNames.Item2 == pairId);

                    if (group == null)
                        Source.Add(new GroupedDM(msg, Resource.User));
                    else if (!group.Messages.Contains(msg))
                    {
                        // Force reordering.
                        Source.Remove(group);
                        group.Messages.Add(msg);
                        Source.Add(group);

                    }
                }
            });
        }

        private void ReceiveConversation(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            if (!response.RequestSucceeded || statuses == null)
            {
                if (Error != null)
                    Error(response);
                return;
            }

            if (statuses.Any())
                GenericReceive(statuses);
        }
        #endregion

        #region Load more button
        IEnumerable<ITweetable> AddLoadMoreButtons(IEnumerable<ITweetable> cache)
        {
            var list = cache.ToList();
            int avgTime = DecisionMaker.GetAvgTimeBetweenTweets(Source);
            double sumTime = 0;

            for (int i = 1; i < list.Count; i++)
            {
                double diff = Math.Abs((list[i].CreatedDate - list[i - 1].CreatedDate).TotalSeconds);
                sumTime += diff;

                if (sumTime != 0 && i > 1
                    && diff > 10 * (sumTime / (i - 1)) && diff > 5 * avgTime
                    && diff > 200)
                    yield return new LoadMoreTweetable { Id = list[i].Id + 1 };

                yield return list[i];
            }
        }

        private void TryAddLoadMoreButton(IEnumerable<ITweetable> received)
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

            if (diff.TotalSeconds > 4 * avgTime && !(nextTweet is LoadMoreTweetable))
            {
                Source.Add(new LoadMoreTweetable { Id = olderTweetReceived.Id - 1 });
            }
        }

        public void RemoveLoadMore(LoadMoreTweetable item)
        {
            var toRemove = Source.FirstOrDefault(x => x.Id == item.Id);
            Source.Remove(item);
        }
        #endregion

        #region Rate limit checks
        static DateTime rateResetTime;
        void CheckForRateLimit(TwitterResponse response)
        {
            if (response.RateLimitStatus.RemainingHits <= 0)
                rateResetTime = response.RateLimitStatus.ResetTime;
        }
        #endregion

        #region Refresh handlers
        Queue<ITweetable> _deferredItems = new Queue<ITweetable>();
        bool allowOneRefresh = false;
        object deferSync = new object();
        bool deferringRefresh = false;

        public void StopSourceRefresh()
        {
            lock (deferSync)
                deferringRefresh = true;
        }

        public void ResumeSourceRefresh()
        {
            lock (deferSync)
            {
                deferringRefresh = false;

                var list = _deferredItems.AsEnumerable().Except(Source);
                TryAddLoadMoreButton(list);

                foreach (var element in list)
                    Source.Add(element);
            }
        }

        public void AllowNextRefresh()
        {
            lock (deferSync)
                allowOneRefresh = true;
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

        #region INotifyPropertyChanged methods
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            var dispatcher = Dependency.Resolve<IDispatcher>();

            if (!dispatcher.IsUIThread)
                dispatcher.BeginInvoke(() => OnPropertyChanged(propName));

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }


}
