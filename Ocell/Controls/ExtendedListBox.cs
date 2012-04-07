using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Ocell.Library;
using TweetSharp;
using System.Windows.Data;
using Microsoft.Phone.Controls;

namespace Ocell.Controls
{
    public class ExtendedListBox : ListBox
    {
        // Compression states: Thanks to http://blogs.msdn.com/b/slmperf/archive/2011/06/30/windows-phone-mango-change-listbox-how-to-detect-compression-end-of-scroll-states.aspx

        private bool _isBouncy = false;
        private bool _bound = false;
        private bool _alreadyHookedScrollEvents = false;
        public TweetLoader Loader;
        protected ObservableCollection<ITweetable> _Items;
        protected CollectionViewSource _ViewSource;
        private ColumnFilter _filter;
   

        public ExtendedListBox()
        {
            Loader = new TweetLoader();
            this.Loaded += new RoutedEventHandler(ListBox_Loaded);
            this.Unloaded += new RoutedEventHandler(ExtendedListBox_Unloaded);
            Loader.LoadFinished += new EventHandler(PopulateItemsSource);
            Loader.PartialLoad += new EventHandler(PopulateItemsSource);
            Loader.CacheLoad += new EventHandler(PopulateItemsSource);
            _Items = new ObservableCollection<ITweetable>();
            _ViewSource = new CollectionViewSource();
            SetupCollectionViewSource();
        }

        private void SetupCollectionViewSource()
        {
            _ViewSource.Source = _Items;
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

        protected void PopulateItemsSource(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    UnsafePopulateItemsSource();
            }
                catch (Exception)
                {
        }
            });
        }

        

        private void UnsafePopulateItemsSource()
        {
            TweetEqualityComparer Comparer = new TweetEqualityComparer();
            int loaded = 0;

            foreach (var item in Loader.Source)
            {
                if (!_Items.Contains(item, Comparer))
                {
                    /*if (_Items.Count == 0)
                        _Items.Add(item);
                    else if(_Items[0].Id > item.Id)
                    {
                        if (item.Id < _Items[_Items.Count - 1].Id)
                            _Items.Add(item);
                        else
                            _Items.Insert(GetInsertPositionFor(item), item);
                    }
                    else
                        _Items.Insert(0, item);
                    */
                    _Items.Add(item);
                    loaded++;
                }
                if (loaded >= 2)
                {
                    Thread.Sleep(10);
                    loaded = 0;
                }
            }
            

        }

        private int GetInsertPositionFor(ITweetable item)
        {
            int i;
            for (i = 0; i < _Items.Count; i++)
            {
                if (_Items[i].Id < item.Id)
                    return i;
                }

            return i;
                }

        public void Bind(TwitterResource Resource)
        {
            Loader.Resource = Resource;
            _bound = true;
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
            Loader.LoadFrom(trigger.Id - 1);
        }

        public void RemoveLoadMore()
        {
            ITweetable item = _Items.FirstOrDefault(e => e is LoadMoreTweetable);
            if (item != null)
                _Items.Remove(item);

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
