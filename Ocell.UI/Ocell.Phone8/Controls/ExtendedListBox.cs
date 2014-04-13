using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using LinqToVisualTree;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using Ocell.Pages.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TweetSharp;

namespace Ocell.Controls
{
    public class ExtendedListBox : LongListSelector
    {
        public TweetLoader Loader
        {
            get { return (TweetLoader)GetValue(LoaderProperty); }
            set { SetValue(LoaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Loader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoaderProperty =
            DependencyProperty.Register("Loader", typeof(TweetLoader), typeof(ExtendedListBox), new PropertyMetadata(null, OnLoaderChanged));

        private static void OnLoaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = (ExtendedListBox)d;

            list.OnLoaderChanged();
        }

        private void OnLoaderChanged()
        {
            if (Loader == null)
                return;

            Loader.Error += Loader_Error;
            SetupCollectionViewSource();
        }

        protected bool isLoading;
        protected bool selectionChangeFired;
        protected DateTime lastAutoReload;
        protected TimeSpan autoReloadInterval = TimeSpan.FromSeconds(60);
        protected static DateTime lastErrorFired;
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

        private ColumnFilter filter;
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
                if (Loader != null)
                    return Loader.Resource;
                else
                    return null;
            }
            set
            {
                if (readingPosManager != null && readingPosManager.Bound)
                {
                    readingPosManager.Unbind();
                    readingPosManager.Bind(this);
                }

                if (Loader != null)
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

            this.ItemRealized += OnItemRealized;
            this.ItemUnrealized += OnItemUnrealized;

            ExtendedListBox.SaveViewports += this.SaveInstanceViewport;

            this.Background = new SolidColorBrush(Colors.Transparent);

            readingPosManager = Dependency.Resolve<IReadingPositionManager>();
            pullDetector = Dependency.Resolve<IListboxCompressionDetector>();
        }

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

        private void SetupCollectionViewSource()
        {
            ItemsSource = (IList)Loader.Source;
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
            if (Loader != null)
                Loader.Load();
        }

        public void LoadOld()
        {
            if (Loader != null)
                Loader.Load(true);
        }


        void Loader_Error(TwitterResponse response)
        {
            var messager = Dependency.Resolve<INotificationService>();
            if (DateTime.Now > lastErrorFired.AddSeconds(10))
            {
                lastErrorFired = DateTime.Now;
                if (response.RateLimitStatus.RemainingHits == 0)
                    messager.ShowError(String.Format(Localization.Resources.RateLimitHit, response.RateLimitStatus.ResetTime.ToString("H:mm")));
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && Loader.Resource.Type == ResourceType.List)
                    messager.ShowError(String.Format(Localization.Resources.ListDeleted, Loader.Resource.Data));
                else
                    messager.ShowError(String.Format(Localization.Resources.ErrorLoadingTweets, response.StatusCode));
            }
        }

        public bool CanRecoverPosition()
        {
            return readingPosManager.CanRecoverPosition();
        }

        public void ResumeReading()
        {
            if (readingPosManager.CanRecoverPosition())
                readingPosManager.RecoverPosition();
        }


        public event EventHandler ReadyToResumePosition;
        #endregion

        #region Listbox Events
        void OnLoad(object sender, RoutedEventArgs e)
        {
            TiltEffect.SetIsTiltEnabled(this, true);

            SetTag();

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
            if (Loader == null)
                return;

            var dispatcher = Deployment.Current.Dispatcher;
            if (dispatcher.CheckAccess())
                DoScrollToTop();
            else
                dispatcher.BeginInvoke(DoScrollToTop);

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

            if (viewport.Count == 0 || Loader == null)
                return;

            var vpMaxId = viewport.Max(x => x.Id);
            var vpMinId = viewport.Min(x => x.Id);
            var upperAmpliation = Loader.Source.Where(x => x.Id > vpMaxId).OrderBy(x => x.Id).Take(10);
            var lowerAmpliation = Loader.Source.Where(x => x.Id < vpMinId).OrderByDescending(x => x.Id).Take(10);

            Loader.SaveToCache(viewport.Concat(upperAmpliation).Concat(lowerAmpliation).ToList());
        }

        #endregion

        #region Auto managers
        DateTime lastPullRefresh = DateTime.MinValue;
        void RefreshOnPull(object sender, CompressionEventArgs e)
        {
            if (!ActivatePullToRefresh || DateTime.Now - lastPullRefresh < TimeSpan.FromSeconds(10))
                return;

            lastPullRefresh = DateTime.Now;

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

                // TODO: Solve this navigation.

                if (e.AddedItems[0] is TwitterStatus)
                    NavigationService.Navigate(new Uri("/Pages/Elements/Tweet.xaml?id=" + DataTransfer.Status.Id.ToString(), UriKind.Relative));
                else if (e.AddedItems[0] is GroupedDM)
                {
                    DataTransfer.DMGroup = e.AddedItems[0] as GroupedDM;
                    NavigationService.Navigate<DMConversationModel>();
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
