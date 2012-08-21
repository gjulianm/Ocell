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

namespace Ocell.Controls
{
    public class ExtendedListBox : ListBox
    {
        // Compression states: Thanks to http://blogs.msdn.com/b/slmperf/archive/2011/06/30/windows-phone-mango-change-listbox-how-to-detect-compression-end-of-scroll-states.aspx

        private bool _isBouncy = false;
        private bool _alreadyHookedScrollEvents = false;
        public TweetLoader Loader;
        protected ObservableCollection<ITweetable> _Items;
        protected CollectionViewSource _ViewSource;
        private ColumnFilter _filter;
        protected bool _isLoading;
        protected bool _selectionChangeFired;
        protected DateTime _lastAutoReload;
        protected TimeSpan _autoReloadInterval = TimeSpan.FromSeconds(60);
        protected static DateTime _lastErrorFired;

        public bool ActivatePullToRefresh { get; set; }
        public bool AutoManageNavigation { get; set; }
        public string NavigationUri { get; set; }
        public bool AutoManageErrors { get; set; }

       

        public ExtendedListBox()
        {
            Loader = new TweetLoader();
            ActivatePullToRefresh = true;
            AutoManageNavigation = true;
            AutoManageErrors = true;
            _selectionChangeFired = false;
            _lastAutoReload = DateTime.MinValue;
            if (_lastErrorFired == null)
                _lastErrorFired = DateTime.MinValue;
            this.Loaded += new RoutedEventHandler(ListBox_Loaded);
            this.Unloaded += new RoutedEventHandler(ExtendedListBox_Unloaded);
            this.Compression += new OnCompression(RefreshOnPull);
            this.Compression += new OnCompression(UndeferRefresh);
            this.SelectionChanged += new SelectionChangedEventHandler(ManageNavigation);
            this.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(ScrollEnded);

            Loader.Error += new TweetLoader.OnError(Loader_Error);

            _Items = new ObservableCollection<ITweetable>();
            _ViewSource = new CollectionViewSource();
            SetupCollectionViewSource();
        }

        void UndeferRefresh(object sender, CompressionEventArgs e)
        {
            if (e.Type == CompressionType.Top)
                Loader.ResumeSourceRefresh();
        }

        void ScrollEnded(object sender, ManipulationCompletedEventArgs e)
        {            
            var sv = (ScrollViewer)FindElementRecursive(this, typeof(ScrollViewer));
            if (sv.VerticalOffset > 0.3)
                Loader.StopSourceRefresh();
        }

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
            var first = Loader.Source.OrderByDescending(item => item.Id).FirstOrDefault();
            if (first != null)
                ScrollIntoView(first);
        }

        private void SetupCollectionViewSource()
        {
            _ViewSource.Source = Loader.Source;
            ItemsSource = _ViewSource.View;
            System.ComponentModel.SortDescription Sorter = new System.ComponentModel.SortDescription();
            Sorter.PropertyName = "Id";
            Sorter.Direction = System.ComponentModel.ListSortDirection.Descending;
            _ViewSource.SortDescriptions.Add(Sorter);
        }

        private void SetTag()
        {
            if (this.Tag != null && this.Tag is string)
                Loader.Resource = new TwitterResource() { String = this.Tag as string };
            else if (this.Tag is TwitterResource)
                Loader.Resource = (TwitterResource)this.Tag;
        }

        void ExtendedListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            Loader.SaveToCache();
        }

        public void Bind(TwitterResource Resource)
        {
            Loader.Resource = Resource;
        }

        public ColumnFilter Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                if (_filter != null)
                    _ViewSource.View.Filter = _filter.getPredicate();
            }
        }

        public void LoadIntermediate(LoadMoreTweetable trigger)
        {
            Loader.AllowNextRefresh();
            Loader.LoadFrom(trigger.Id + 1);
        }

        public void RemoveLoadMore()
        {
            ITweetable item = _Items.FirstOrDefault(e => e is LoadMoreTweetable);
            while (item != null)
            {
                _Items.Remove(item);
                item = _Items.FirstOrDefault(e => e is LoadMoreTweetable);
            }

            Loader.RemoveLoadMore();
        }

        #region Scroll Events
        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollBar sb = null;
            ScrollViewer sv = null;
            if (_alreadyHookedScrollEvents)
                return;

            _alreadyHookedScrollEvents = true;
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

            SetTag();
        }

        private void hgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionLeft")
            {
                _isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Left));
            }

            if (e.NewState.Name == "CompressionRight")
            {
                _isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Right));
            }
            if (e.NewState.Name == "NoHorizontalCompression")
            {
                _isBouncy = false;
            }
        }

        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionTop")
            {
                _isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Top));
            }
            else if (e.NewState.Name == "CompressionBottom")
            {
                _isBouncy = true;
                if (Compression != null)
                    Compression(this, new CompressionEventArgs(CompressionType.Bottom));
            }
            else if (e.NewState.Name == "NoVerticalCompression")
                _isBouncy = false;
        }

        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_isBouncy)
                _isBouncy = false;
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

        void RefreshOnPull(object sender, CompressionEventArgs e)
        {
            if (!ActivatePullToRefresh)
                return;

            bool old = (e.Type == Controls.CompressionType.Bottom);

            if (old)
                Loader.AllowNextRefresh();

            Loader.Load(old);
        }

        void ManageNavigation(object sender, SelectionChangedEventArgs e)
        {
            if (!AutoManageNavigation)
                return;

            INavigationService NavigationService = Dependency.Resolve<INavigationService>();

            if (!_selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                DataTransfer.DM = e.AddedItems[0] as TwitterDirectMessage;

                _selectionChangeFired = true;
                SelectedItem = null;

                if (e.AddedItems[0] is TwitterStatus)
                    NavigationService.Navigate(Uris.ViewTweet);
                else if (e.AddedItems[0] is TwitterDirectMessage)
                    NavigationService.Navigate(Uris.ViewDM);

                else if (e.AddedItems[0] is TwitterSearchStatus)
                {
                    DataTransfer.Status = StatusConverter.SearchToStatus(e.AddedItems[0] as TwitterSearchStatus);
                    NavigationService.Navigate(Uris.ViewTweet);
                }
                else if (e.AddedItems[0] is LoadMoreTweetable)
                {
                    LoadIntermediate(e.AddedItems[0] as LoadMoreTweetable);
                    RemoveLoadMore();
                }
            }
            else
                _selectionChangeFired = false;
        }

        void Loader_Error(TwitterResponse response)
        {
            var messager = Dependency.Resolve<IMessageService>();
            if (DateTime.Now > _lastErrorFired.AddSeconds(10))
            {
                _lastErrorFired = DateTime.Now;
                if (response.RateLimitStatus.RemainingHits == 0)
                    messager.ShowError(String.Format(Localization.Resources.RateLimitHit, response.RateLimitStatus.ResetTime.ToString("H:mm")));
                else
                    messager.ShowError(String.Format(Localization.Resources.ErrorLoadingTweets, response.StatusDescription));
                
            }
        }

        public void AutoReload() // EL will manage times to avoid overcalling Twitter API
        {
            if (DateTime.Now > (_lastAutoReload + _autoReloadInterval))
            {
                Loader.Load();
                _lastAutoReload = DateTime.Now;
            }
        }

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
