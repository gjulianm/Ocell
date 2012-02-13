using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TweetSharp;
using Ocell.Library;

namespace Ocell.Controls
{
    public class ExtendedListBox : ListBox
    {
        // Compression states: Thanks to http://blogs.msdn.com/b/slmperf/archive/2011/06/30/windows-phone-mango-change-listbox-how-to-detect-compression-end-of-scroll-states.aspx
        
        protected bool _isBouncy = false;
        protected bool bound = false;
        private bool alreadyHookedScrollEvents = false;
        public TweetLoader Loader;

        public ExtendedListBox()
        {
            Loader = new TweetLoader();
            this.Loaded += new RoutedEventHandler(ListBox_Loaded);
            this.Unloaded += new RoutedEventHandler(ExtendedListBox_Unloaded);
            Loader.LoadFinished += new TweetLoader.OnLoadFinished(PopulateItemsSource);
            Loader.PartialLoad += new TweetLoader.OnPartialLoad(PopulateItemsSource);
            Loader.CacheLoad += new TweetLoader.OnCacheLoad(PopulateItemsSource);
        }

        void ExtendedListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            Loader.SaveToCache();
        }
        
        protected void PopulateItemsSource()
        {
            Dispatcher.BeginInvoke(() =>
            {
                TweetEqualityComparer Comparer = new TweetEqualityComparer();

                ObservableCollection<ITweetable> source = ItemsSource as ObservableCollection<ITweetable>;
                ObservableCollection<ITweetable> newSource = new ObservableCollection<ITweetable>(Loader.Source); 
                
                if (source != null)
                {
                    foreach (var item in source)
                        if (!newSource.Contains(item, Comparer))
                            newSource.Add(item);
                }

                this.ItemsSource = newSource.OrderByDescending(item => item.Id);
            });
        }

        public delegate void OnCompression(object sender, CompressionEventArgs e);
        public event OnCompression Compression;

        public void Bind(TwitterResource Resource)
        {
            Loader.Resource = Resource;
            bound = true;
        }

        #region Scroll Events
        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollBar sb = null;
            ScrollViewer sv = null;
            if (alreadyHookedScrollEvents)
                return;

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

            if (this.Tag != null && this.Tag is string)
                Loader.Resource = new TwitterResource() { String = this.Tag as string };
            else if (this.Tag is TwitterResource)
                Loader.Resource = (TwitterResource)this.Tag;
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
                if(Compression!=null)
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
