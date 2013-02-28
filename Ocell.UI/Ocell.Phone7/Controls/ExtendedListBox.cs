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

        private bool isBouncy = false;
        private bool alreadyHookedScrollEvents = false;
        public TweetLoader Loader;
        protected CollectionViewSource viewSource;
        private ColumnFilter filter;
        protected bool isLoading;
        protected bool selectionChangeFired;
        protected DateTime lastAutoReload;
        protected TimeSpan autoReloadInterval = TimeSpan.FromSeconds(60);
        protected static DateTime lastErrorFired;
        protected IScrollController scrollController;
        protected IReadingPositionManager readingPosManager;
        protected IInfiniteScroller infiniteScroller;
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
                lock(viewportItemsLock)
                    return viewportItems.Keys.ToList();
            }
        }

        #region Setup
        public ExtendedListBox()
        {
            Loader = new TweetLoader();
            viewSource = new CollectionViewSource();
            viewportItems = new Dictionary<ITweetable, ContentPresenter>();

            ActivatePullToRefresh = true;
            AutoManageNavigation = true;
            AutoManageErrors = true;

            selectionChangeFired = false;
            lastAutoReload = DateTime.MinValue;
            if (lastErrorFired == null)
                lastErrorFired = DateTime.MinValue;

            this.Loaded += OnLoad;
            this.Compression += RefreshOnPull;
            this.SelectionChanged += ManageNavigation;

            this.ItemRealized += OnItemRealized;
            this.ItemUnrealized += OnItemUnrealized;

            ExtendedListBox.SaveViewports += this.SaveInstanceViewport;

            Loader.Error += new TweetLoader.OnError(Loader_Error);
            Loader.CacheLoad += new EventHandler(Loader_CacheLoad);
            Loader.LoadFinished += Loader_LoadFinished;
            SetupCollectionViewSource();

            this.Background = new SolidColorBrush(Colors.Transparent);

            scrollController = Dependency.Resolve<IScrollController>();
            readingPosManager = Dependency.Resolve<IReadingPositionManager>();
            infiniteScroller = Dependency.Resolve<IInfiniteScroller>();
        }

        private void OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                ITweetable o = e.Container.DataContext as ITweetable;
                if (o != null)
                {
                    lock (viewportItemsLock)
                        viewportItems[o] = e.Container;
                }
            }
        }

        private void OnItemUnrealized(object sender, ItemRealizationEventArgs e)
        {
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

        
        
        private void SetupCollectionViewSource()
        {
            ItemsSource = Loader.Source;
        }

        private void SetTag()
        {
            if (this.Tag != null && this.Tag is string)
                Loader.Resource = new TwitterResource() { String = this.Tag as string };
            else if (this.Tag is TwitterResource)
                Loader.Resource = (TwitterResource)this.Tag;
        }

        public void Bind(TwitterResource Resource)
        {
            Loader.Resource = Resource;
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

            if (!infiniteScroller.Bound)
                infiniteScroller.Bind(this);

            if (!alreadyHookedScrollEvents)
                HookScrollEvent();            
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
            var first = Loader.Source.FirstOrDefault();

            if (first != null)
                ScrollTo(first);
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
            var vpMaxId = viewport.Max(x => x.Id);
            var vpMinId = viewport.Min(x => x.Id);
            var upperAmpliation = Loader.Source.Where(x => x.Id > vpMaxId).OrderBy(x => x.Id).Take(10);
            var lowerAmpliation = Loader.Source.Where(x => x.Id < vpMinId).OrderByDescending(x => x.Id).Take(10);
            Loader.SaveToCache(viewport.Concat(upperAmpliation).Concat(lowerAmpliation).ToList());
        }

        #endregion

        #region Scroll Events
        private void HookScrollEvent()
        {
            ScrollBar sb = null;
            ScrollViewer sv = null;

            alreadyHookedScrollEvents = true;
            this.AddHandler(ExtendedListBox.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)LB_ManipulationCompleted, true);
            sb = (ScrollBar)FindElementRecursive(this, typeof(ScrollBar));
            sv = (ScrollViewer)FindElementRecursive(this, typeof(ScrollViewer));

            if (sv != null)
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");
                    VisualStateGroup hgroup = FindVisualState(element, "HorizontalCompression");
                    if (vgroup != null)
                        vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
                    if (hgroup != null)
                        hgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(hgroup_CurrentStateChanging);
                }
            }
        }

        private void hgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionLeft")
            {
                isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Left));
            }

            if (e.NewState.Name == "CompressionRight")
            {
                isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Right));
            }
            if (e.NewState.Name == "NoHorizontalCompression")
            {
                isBouncy = false;
            }
        }

        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionTop")
            {
                isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Top));
            }
            else if (e.NewState.Name == "CompressionBottom")
            {
                isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Bottom));
            }
            else if (e.NewState.Name == "NoVerticalCompression")
                isBouncy = false;
        }

        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (isBouncy)
                isBouncy = false;
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }
            return returnElement;
        }

        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
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

        public delegate void OnCompression(object sender, CompressionEventArgs e);
        public event OnCompression Compression;
    }

    public class CompressionEventArgs : EventArgs
    {
        public CompressionType Type { get; protected set; }

        public CompressionEventArgs(CompressionType type)
        {
            Type = type;
        }
    }

    public enum CompressionType { Top, Bottom, Left, Right };
}
