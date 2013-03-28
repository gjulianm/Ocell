using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using TweetSharp;
using System.ComponentModel;
using DanielVaughan.ComponentModel;
using Ocell.Library;
using DanielVaughan.Services;
using DanielVaughan;
using Ocell.Localization;
using System.Collections.Generic;
using LinqToVisualTree;
using Ocell.Controls;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace Ocell.Controls
{
    public class ExtendedListBox : LongListSelector
    {
        // Compression states: Thanks to http://blogs.msdn.com/b/slmperf/archive/2011/06/30/windows-phone-mango-change-listbox-how-to-detect-compression-end-of-scroll-states.aspx
        public TweetLoader Loader;
        private ColumnFilter filter;
        protected bool isLoading;
        protected bool selectionChangeFired;
        protected DateTime lastAutoReload;
        protected TimeSpan autoReloadInterval = TimeSpan.FromSeconds(60);
        protected static DateTime lastErrorFired;
        protected IScrollController scrollController;
        protected IReadingPositionManager readingPosManager;
        protected IListboxCompressionDetector pullDetector;
        private bool goTopOnNextLoad = false;
        private Dictionary<ITweetable, ContentPresenter> viewportItems;
        private object viewportItemsLock = new object();

        ScrollViewer sv;
        private ScrollViewer scrollViewer
        {
            get
            {
                if (sv == null)
                    sv = this.Descendants().OfType<ScrollViewer>().FirstOrDefault();

                return sv;
            }
        }

        public bool ActivatePullToRefresh { get; set; }
        public bool AutoManageNavigation { get; set; }
        public string NavigationUri { get; set; }
        public bool AutoManageErrors { get; set; }

        public ColumnFilter Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                if (filter != null)
                    Loader.Source.Filter = filter.GetPredicate();
            }
        }

        public IDictionary<ITweetable, ContentPresenter> ViewportItems
        {
            get
            {
                lock (viewportItemsLock)
                    return new Dictionary<ITweetable, ContentPresenter>(viewportItems); /* Copy to avoid cross-thread access */
            }
        }

        public IList<ITweetable> VisibleItems
        {
            get
            {
                lock (viewportItemsLock)
                    return viewportItems.Keys.ToList();
            }
        }


        public TwitterResource Resource
        {
            get
            {
                return Loader.Resource;
            }
            set
            {
                if (readingPosManager != null && readingPosManager.Bound)
                {
                    readingPosManager.Unbind();
                    readingPosManager.Bind(this);
                }

                Loader.Resource = value;
            }
        }

        #region Setup
        public ExtendedListBox()
        {
            Loader = new TweetLoader();
            viewportItems = new Dictionary<ITweetable, ContentPresenter>();

            ActivatePullToRefresh = true;
            AutoManageNavigation = true;
            AutoManageErrors = true;

            selectionChangeFired = false;
            lastAutoReload = DateTime.MinValue;
            if (lastErrorFired == null)
                lastErrorFired = DateTime.MinValue;

            this.Loaded += OnLoad;
            this.SelectionChanged += ManageNavigation;
#if WP7
            this.Link += ExtendedListBox_Link;
            this.Unlink += ExtendedListBox_Unlink;
#elif WP8
            this.ItemRealized += OnItemRealized;
            this.ItemUnrealized += OnItemUnrealized;
#endif

            ExtendedListBox.SaveViewports += this.SaveInstanceViewport;

            Loader.Error += new TweetLoader.OnError(Loader_Error);
            Loader.CacheLoad += new EventHandler(Loader_CacheLoad);
            Loader.LoadFinished += Loader_LoadFinished;
            SetupCollectionViewSource();

            this.Background = new SolidColorBrush(Colors.Transparent);

            scrollController = Dependency.Resolve<IScrollController>();
            readingPosManager = Dependency.Resolve<IReadingPositionManager>();
            pullDetector = Dependency.Resolve<IListboxCompressionDetector>();
        }

#if WP7
        void ExtendedListBox_Unlink(object sender, LinkUnlinkEventArgs e)
        {
            ITweetable o = e.ContentPresenter.DataContext as ITweetable;
            if (o != null)
            {
                lock (viewportItemsLock)
                    viewportItems.Remove(o);
            }
        }

        void ExtendedListBox_Link(object sender, LinkUnlinkEventArgs e)
        {
            ITweetable o = e.ContentPresenter.DataContext as ITweetable;
            if (o != null)
            {
                lock (viewportItemsLock)
                    viewportItems[o] = e.ContentPresenter;
            }
        }

#elif WP8
        bool viewportChanged = false;

        private void OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            viewportChanged = true;
            ITweetable o = e.Container.DataContext as ITweetable;
            if (o != null)
            {
                lock (viewportItemsLock)
                    viewportItems[o] = e.Container;
            }
        }

        private void OnItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
            viewportChanged = true;
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                ITweetable o = e.Container.DataContext as ITweetable;
                if (o != null)
                {
                    lock (viewportItemsLock)
                        viewportItems.Remove(o);
                }
            }
        }
#endif


        private void SetupCollectionViewSource()
        {
            ItemsSource = Loader.Source;
        }

        private void SetTag()
        {
            if (this.Tag != null && this.Tag is string)
                Resource = new TwitterResource() { String = this.Tag as string };
            else if (this.Tag is TwitterResource)
                Resource = (TwitterResource)this.Tag;
        }

        #endregion

        #region Tweetloader communication
        public void Load()
        {
            scrollController.LoadCalled(-1);
            Loader.Load();
        }

        public void LoadOld()
        {
            scrollController.LoadCalled(-2);
            Loader.Load(true);
        }


        void Loader_Error(TwitterResponse response)
        {
            var messager = Dependency.Resolve<IMessageService>();
            if (DateTime.Now > lastErrorFired.AddSeconds(10) && !string.IsNullOrWhiteSpace(response.StatusDescription))
            {
                lastErrorFired = DateTime.Now;
                if (response.RateLimitStatus.RemainingHits == 0)
                    messager.ShowError(String.Format(Localization.Resources.RateLimitHit, response.RateLimitStatus.ResetTime.ToString("H:mm")));
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && Loader.Resource.Type == ResourceType.List)
                    messager.ShowError(String.Format(Localization.Resources.ListDeleted, Loader.Resource.Data));
                else
                    messager.ShowError(String.Format(Localization.Resources.ErrorLoadingTweets, response.StatusDescription));
            }
        }


        void Loader_CacheLoad(object sender, EventArgs e)
        {
            if (Config.ReloadOptions == ColumnReloadOptions.AskPosition)
                TryTriggerResumeReading();
            else if (Config.ReloadOptions == ColumnReloadOptions.KeepPosition && readingPosManager.CanRecoverPosition())
                readingPosManager.RecoverPosition();
            else
                goTopOnNextLoad = true;

        }

        public void TryTriggerResumeReading()
        {
            if (readingPosManager.CanRecoverPosition())
            {
                if (ReadyToResumePosition != null)
                    ReadyToResumePosition(this, new EventArgs());
            }
        }

        public void ResumeReading()
        {
            if (readingPosManager.CanRecoverPosition())
                readingPosManager.RecoverPosition();
        }

        void Loader_LoadFinished(object sender, EventArgs e)
        {
            if (goTopOnNextLoad)
            {
                goTopOnNextLoad = false;
                ScrollToTop();
            }
        }

        public event EventHandler ReadyToResumePosition;
        #endregion

        #region Listbox Events
        void OnLoad(object sender, RoutedEventArgs e)
        {
            SetTag();

            if (!scrollController.Bound)
                scrollController.Bind(this);

            if (!readingPosManager.Bound)
                readingPosManager.Bind(this);

            if (!pullDetector.Bound)
            {
                pullDetector.Bind(this);
                pullDetector.Compression += RefreshOnPull;
            }
        }

        #endregion

        #region Scroll to top
        public void ScrollToTop()
        {
            var dispatcher = Deployment.Current.Dispatcher;
            if (dispatcher.CheckAccess())
                DoScrollToTop();
            else
                dispatcher.BeginInvoke(DoScrollToTop);

            Loader.ResumeSourceRefresh();
        }

        private void DoScrollToTop()
        {
#if WP8
            var first = Loader.Source.FirstOrDefault();

            if (first != null)
                ScrollTo(first);
#elif WP7
            if(scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(0);
            }
#endif
        }
        #endregion

        #region Load More
        public void LoadIntermediate(LoadMoreTweetable trigger)
        {
            scrollController.LoadCalled();
            Loader.AllowNextRefresh();
            Loader.LoadFrom(trigger.Id + 1);
        }

        public void RemoveLoadMore(LoadMoreTweetable item)
        {
            Loader.RemoveLoadMore(item);
        }
        #endregion

        #region Viewport
        protected static event EventHandler SaveViewports;

        public static void RaiseSaveViewports()
        {
            if (SaveViewports != null)
                SaveViewports(null, null);
        }

        protected void SaveInstanceViewport(object sender, EventArgs e)
        {
            var viewport = VisibleItems.ToList();

            if (viewport.Count == 0)
                return;

            var vpMaxId = viewport.Max(x => x.Id);
            var vpMinId = viewport.Min(x => x.Id);
            var upperAmpliation = Loader.Source.Where(x => x.Id > vpMaxId).OrderBy(x => x.Id).Take(10);
            var lowerAmpliation = Loader.Source.Where(x => x.Id < vpMinId).OrderByDescending(x => x.Id).Take(10);

            Loader.SaveToCache(viewport.Concat(upperAmpliation).Concat(lowerAmpliation).ToList());
        }

        #endregion

        #region Auto managers
        void RefreshOnPull(object sender, CompressionEventArgs e)
        {
            if (!ActivatePullToRefresh)
                return;

            bool old = (e.Type == Controls.CompressionType.Bottom);

            if (old)
                LoadOld();
            else
                Load();
        }

        void ManageNavigation(object sender, SelectionChangedEventArgs e)
        {
            if (!AutoManageNavigation)
                return;

            INavigationService NavigationService = Dependency.Resolve<INavigationService>();

            if (!selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                DataTransfer.DM = e.AddedItems[0] as TwitterDirectMessage;

                selectionChangeFired = true;
                SelectedItem = null;

                if (e.AddedItems[0] is TwitterStatus)
                    NavigationService.Navigate(Uris.ViewTweet);
                else if (e.AddedItems[0] is TwitterDirectMessage)
                    NavigationService.Navigate(Uris.ViewDM);
                else if (e.AddedItems[0] is GroupedDM)
                {
                    DataTransfer.DMGroup = e.AddedItems[0] as GroupedDM;
                    NavigationService.Navigate(Uris.DMConversation);
                }
                else if (e.AddedItems[0] is LoadMoreTweetable)
                {
                    LoadIntermediate(e.AddedItems[0] as LoadMoreTweetable);
                    RemoveLoadMore(e.AddedItems[0] as LoadMoreTweetable);
                }
            }
            else
                selectionChangeFired = false;
        }

        public void AutoReload() // EL will manage times to avoid overcalling Twitter API
        {
            if (DateTime.Now > (lastAutoReload + autoReloadInterval))
            {
                Load();
                lastAutoReload = DateTime.Now;
            }
        }
        #endregion

    }
}
