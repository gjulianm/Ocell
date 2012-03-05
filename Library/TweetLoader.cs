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
    public class TweetLoader
    {
        private int Loaded;
        private readonly int ToLoad = 1;
        public int TweetsToLoadPerRequest { get; set; }
        public bool Cached { get; set; }
        private int RequestsInProgress;
	    protected TwitterResource _resource;
        private static DateTime RateResetTime;

        public TwitterResource Resource 
        {
	        get
	        {
		        return _resource;
	        }
	        set
	        {
		        _resource = value;
		        _srv = ServiceDispatcher.GetService(_resource.User);
	        }
	    }
        public ObservableCollection<ITweetable> Source { get; protected set; }
	    protected TwitterService _srv;
        protected long LastId;
        
        #region Constructors
        public TweetLoader(TwitterResource resource) : this()
        {
            Resource = resource;
	        _srv = ServiceDispatcher.GetService(Resource.User);

            LoadCacheAsync();
        }

        public TweetLoader()
        {
            TweetsToLoadPerRequest = 25;
            Loaded = 0;
            Source = new ObservableCollection<TweetSharp.ITweetable>();
            LastId = 0;
            Cached = true;
            RequestsInProgress = 0;
            if (RateResetTime == null)
                RateResetTime = DateTime.MinValue;

            Error += new OnError(CheckForRateLimit);
        }

        void CheckForRateLimit(TwitterResponse response)
        {
            if (response.RateLimitStatus.RemainingHits <= 0)
                RateResetTime = response.RateLimitStatus.ResetTime;
        }
        #endregion

        public void LoadCacheAsync()
        {
            Thread LoaderThread = new Thread(new ThreadStart(LoadCache));
            LoaderThread.Start();
        }

        protected void ReceiveCallback(IAsyncResult result)
        {
        }

        #region Loaders
        public void Load(bool Old = false)
        {
            if (Resource == null || _srv == null ||
                RequestsInProgress >= 2 ||
                RateResetTime > DateTime.Now)
            {
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
                return;
            }

            RequestsInProgress++;

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
                    _srv.ListTweetsOnHomeTimeline(TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    _srv.ListTweetsMentioningMe(TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Messages:
                    _srv.ListDirectMessagesReceived(TweetsToLoadPerRequest, ReceiveMessages);
                    _srv.ListDirectMessagesSent(TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    _srv.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    _srv.ListTweetsOnList(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), ReceiveTweets);
                    break;
                case ResourceType.Search:
                    _srv.Search(Resource.Data, 1, 20, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    _srv.ListTweetsOnSpecifiedUserTimeline(Resource.Data, TweetsToLoadPerRequest, ReceiveTweets);
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
                    _srv.ListTweetsOnHomeTimelineBefore(Last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Mentions:
                    _srv.ListTweetsMentioningMeBefore(Last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
                case ResourceType.Messages:
                    _srv.ListDirectMessagesReceivedBefore(Last, TweetsToLoadPerRequest, ReceiveMessages);
                    _srv.ListDirectMessagesSentBefore(Last, TweetsToLoadPerRequest, ReceiveMessages);
                    break;
                case ResourceType.Favorites:
                    _srv.ListFavoriteTweets(ReceiveTweets);
                    break;
                case ResourceType.List:
                    _srv.ListTweetsOnListBefore(Resource.Data.Substring(1, Resource.Data.IndexOf('/') - 1),
                            Resource.Data.Substring(Resource.Data.IndexOf('/') + 1), Last, ReceiveTweets);
                    break;
                case ResourceType.Search:
                    _srv.SearchBefore(Last, Resource.Data, ReceiveSearch);
                    break;
                case ResourceType.Tweets:
                    _srv.ListTweetsOnSpecifiedUserTimelineBefore(Resource.Data, Last, TweetsToLoadPerRequest, ReceiveTweets);
                    break;
            }
        }
        
        public void LoadCache()
        {
            if (!Cached || Resource == null || Resource.User == null)
                return;

            TweetEqualityComparer comparer = new TweetEqualityComparer();
            IEnumerable<TwitterStatus> CacheList = Cacher.GetFromCache(Resource).OrderByDescending(item => item.Id);

            if (!DecisionMaker.ShouldLoadCache(ref CacheList))
                return;

            foreach (var item in CacheList)
                if (!Source.Contains(item, comparer))
                    Source.Add(item);

            if(CacheLoad != null)
                CacheLoad(this, new EventArgs());
        }
        #endregion

        #region Specific Receivers
        protected void ReceiveTweets(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            RequestsInProgress--;
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
            RequestsInProgress--;
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
            RequestsInProgress--;
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
            
            if(Source == null)
            	Source = new ObservableCollection<ITweetable>();

            foreach (var status in list)
                if (!Source.Contains(status, comparer))
                    Source.Add(status);
                    
            OrderSource();

            if (list.Count() != 0)
                LastId = list.Last().Id;

            if (Loaded < ToLoad && list.Count() > 0)
            {
                LoadOld(list.Last().Id);
                if (PartialLoad != null)
                    PartialLoad(this, new EventArgs());
            }
            else
            {
                Loaded = 0;
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

     

        #region Events
        public event EventHandler LoadFinished;

        public delegate void OnError(TwitterResponse response);
        public event OnError Error;

        public event EventHandler PartialLoad;

        public event EventHandler CacheLoad;
        #endregion
    } 
        
}
