using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Notifications;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Threading;
using System.Diagnostics;


namespace Ocell
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ObservableCollection<TwitterResource> pivots;
        private bool selectionChangeFired;
        private Dictionary<string, ExtendedListBox> Lists;
        private DateTime LastErrorTime;
        private DateTime LastReloadTime;
        private bool _initialised;
        private Stopwatch _watch;

        #region Loaders
        // Constructora
        public MainPage()
        {
            _watch = new Stopwatch();
            _initialised = false;
            InitializeComponent();

            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            pivots = new ObservableCollection<TwitterResource>();
            Lists = new Dictionary<string, ExtendedListBox>();

            this.Loaded += new RoutedEventHandler(CallLoadFunctions);
            pivots.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(pivots_CollectionChanged);
            MainPivot.SelectionChanged += new SelectionChangedEventHandler(PivotSelectionChanged);

            MainPivot.DataContext = pivots;
            MainPivot.ItemsSource = pivots;

            LastErrorTime = DateTime.MinValue;
            LastReloadTime = DateTime.MinValue;

            SetUpPivots();
        }

        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateTime.Now > LastReloadTime.AddSeconds(25))
            {
                LoadTweetsOnPivots();
                LastReloadTime = DateTime.Now;
            }
            RefreshCurrentAccount(e);
        }

        void ReloadColumns()
        {
            foreach (var pivot in Config.Columns)
                if (!pivots.Contains(pivot))
                    pivots.Add(pivot);
            try
            {
                foreach (var column in pivots)
                    if (!Config.Columns.Contains(column))
                        pivots.Remove(column);
            }
            catch
            {
            }
        }

        void CallLoadFunctions(object sender, RoutedEventArgs e)
        {
            if (DataTransfer.ShouldReloadColumns)
                ReloadColumns();

            if (_initialised)
                return;

            ThreadPool.QueueUserWorkItem((threadContext) =>
            {
                CreateTile();
                ShowFollowMessage();
                LittleWatson.CheckForPreviousException();
            });

            string Column;
            if (NavigationContext.QueryString.TryGetValue("column", out Column))
                NavigateToColumn(Uri.UnescapeDataString(Column));

            _initialised = true;
        }


        void NavigateToColumn(string Column)
        {
            TwitterResource Resource = pivots.FirstOrDefault(item => item.String == Column);
            if (Resource != null)
                MainPivot.SelectedItem = Resource;
        }

        void ShowFollowMessage()
        {
            if ((Config.FollowMessageShown == false || Config.FollowMessageShown == null) && ServiceDispatcher.CanGetServices)
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBoxResult Result = MessageBox.Show("Do you want to follow @OcellApp on Twitter to receive the lastest updates?", "", MessageBoxButton.OKCancel);
                        if (Result == MessageBoxResult.OK)
                            ServiceDispatcher.GetDefaultService().FollowUser("OcellApp", DummyReceiveFollow);
                    });
                Config.FollowMessageShown = true;
            }
        }

        void DummyReceiveFollow(TwitterUser User, TwitterResponse Response)
        {
        }

        void CreateTile()
        {
            SchedulerSync.WriteLastCheckDate(DateTime.Now.ToUniversalTime());
            SchedulerSync.StartPeriodicAgent();
            TileManager.UpdateTile(null, null); // Updating with null means it will clear the tile.
        }

        void SetUpPivots()
        {
            if (Config.Accounts.Count == 0)
            {
                ShowLoginMsg();
                return;
            }

            DataTransfer.CurrentAccount = Config.Accounts[0];
            UpdatePivotTitle();

            GetPivotsFromConf();
        }


        #endregion

        private void LoadTweetsOnPivots()
        {
            if (MainPivot.SelectedItem == null)
                return;
            ExtendedListBox ListBox;
            TwitterResource Resource = (TwitterResource)MainPivot.SelectedItem;
            if (Lists.TryGetValue(Resource.String, out ListBox))
            {
                Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
                ListBox.Loader.Load();
            }
        }

        private void RefreshCurrentAccount(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
            {
                TwitterResource item = (TwitterResource)e.AddedItems[0];
                DataTransfer.CurrentAccount = item.User;
                UpdatePivotTitle();
            }
        }
        void BindPivots()
        {
            MainPivot.DataContext = null;
            MainPivot.ItemsSource = pivots;
        }

        void ShowLoginMsg()
        {
            Config.TweetsPerRequest = 40;
            Dispatcher.BeginInvoke(() =>
            {
                MessageBoxResult r = MessageBox.Show("You have to log in with Twitter in order to use Ocell.", "", MessageBoxButton.OKCancel);
                if (r == MessageBoxResult.OK)
                    NavigationService.Navigate(Uris.LoginPage);
            });
        }

        void GetPivotsFromConf()
        {
            foreach (var pivot in Config.Columns)
                if (!pivots.Contains(pivot))
                    pivots.Add(pivot);
        }

        void UpdatePivotTitle()
        {
            Dispatcher.BeginInvoke(() =>
            {
                MainPivot.Title = "OCELL";
                if (DataTransfer.CurrentAccount != null && !string.IsNullOrWhiteSpace(DataTransfer.CurrentAccount.ScreenName))
                    MainPivot.Title += " - " + DataTransfer.CurrentAccount.ScreenName.ToUpperInvariant();
            });
        }

        void pivots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BindPivots();
        }

        private void compose_Click(object sender, System.EventArgs e)
        {
            if (Config.Accounts.Count == 0)
                ShowLoginMsg();
            else
                NavigationService.Navigate(Uris.WriteTweet);
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            TwitterResource Resource = new TwitterResource();

            if (list == null)
                return;

            if (list.Tag is TwitterResource)
            {
                Resource = (TwitterResource)list.Tag;
                list.Bind(Resource);
            }

            FilterManager.SetupFilter(list);

            if (Lists.ContainsKey(Resource.String))
                return;

            Lists.Add(Resource.String, list);

            list.Compression += new ExtendedListBox.OnCompression(list_Compression);
            list.Loader.Error += new TweetLoader.OnError(Loader_Error);
            list.Loader.LoadFinished += new EventHandler(Loader_LoadFinished);
            list.Loader.ActivateLoadMoreButton = true;
            if (Config.RetweetAsMentions == null)
                Config.RetweetAsMentions = true;
            if (Config.TweetsPerRequest == null)
                Config.TweetsPerRequest = 40;

            list.Loader.TweetsToLoadPerRequest = (int)Config.TweetsPerRequest;
            list.Loader.LoadRetweetsAsMentions = (bool)Config.RetweetAsMentions;

            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            list.Loader.LoadCacheAsync();
            list.Loader.Load();

            list.Loaded -= ListBox_Loaded;
            list.Loaded += new RoutedEventHandler(CheckForFilterUpdate);
        }

        void CheckForFilterUpdate(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            if (list != null && ((DataTransfer.ShouldReloadFilters && DataTransfer.cFilter.Resource == list.Loader.Resource) || DataTransfer.IsGlobalFilter))
            {
                FilterManager.SetupFilter(list);
                DataTransfer.ShouldReloadFilters = false;
            }
        }

        void Loader_LoadFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }

        void Loader_Error(TwitterResponse response)
        {
            if (response != null && response.StatusCode != System.Net.HttpStatusCode.OK && LastErrorTime.AddSeconds(10) < DateTime.Now)
            {
                LastErrorTime = DateTime.Now;
                Dispatcher.BeginInvoke(() =>
                {
                    if (response.RateLimitStatus.RemainingHits == 0)
                        MessageBox.Show("Woops! You have spent the limit of calls to Twitter. You'll have to wait until " + response.RateLimitStatus.ResetTime.ToString("H:mm"));
                    else
                        MessageBox.Show("We couldn't load the tweets: " + response.StatusDescription);
                    pBar.IsVisible = false;
                });
            }
        }

        void list_Compression(object sender, CompressionEventArgs e)
        {
            bool Old = (e.Type == CompressionType.Bottom);
            ExtendedListBox List = sender as ExtendedListBox;

            if (List == null || List.Loader == null)
                return;
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            List.Loader.Load(Old);
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(Uris.Settings);
        }

        private void add_Click(object sender, EventArgs e)
        {
            if (Config.Accounts.Count == 0)
                ShowLoginMsg();
            else
                NavigationService.Navigate(Uris.Columns);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                DataTransfer.DM = e.AddedItems[0] as TwitterDirectMessage;
                ExtendedListBox list = sender as ExtendedListBox;
                selectionChangeFired = true;
                list.SelectedItem = null;
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
                    Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
                    list.LoadIntermediate(e.AddedItems[0] as LoadMoreTweetable);
                    list.RemoveLoadMore();
                }
            }
            else
                selectionChangeFired = false;
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivot.DataContext = pivots;
            MainPivot.ItemsSource = pivots;
        }

        private void send_DM_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.SelectUserForDM);
        }

        private void SearchBtn_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.SearchForm);
        }

        private void pinToStart_Click(object sender, System.EventArgs e)
        {
            if (SecondaryTiles.ColumnTileIsCreated((TwitterResource)MainPivot.SelectedItem))
                Dispatcher.BeginInvoke(() => MessageBox.Show("This column is already pinned."));
            else
                SecondaryTiles.CreateColumnTile((TwitterResource)MainPivot.SelectedItem);
        }

        private void about_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.About);
        }

        private void Trending_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.TrendingTopics);
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            DataTransfer.cFilter = Config.Filters.FirstOrDefault(item => item.Resource == ((TwitterResource)MainPivot.SelectedItem));
            if (DataTransfer.cFilter == null)
                DataTransfer.cFilter = new ColumnFilter { Resource = (TwitterResource)MainPivot.SelectedItem };
            DataTransfer.IsGlobalFilter = false;
            NavigationService.Navigate(Uris.Filters);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataTransfer.IsGlobalFilter = false;
            base.OnNavigatedFrom(e);
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TextBlock text = sender as TextBlock;
            if (text == null)
                return;

            ExtendedListBox list;
            TwitterResource resource;

            if (text.Tag is TwitterResource)
            {
                resource = (TwitterResource)text.Tag;
                if(Lists.TryGetValue(resource.String, out list) && list.ItemsSource != null && list.Loader.Source.Any())
                {
                    list.ScrollIntoView(list.Loader.Source.First());
                }
            }

        }

        private void myprofile_Click(object sender, System.EventArgs e)
        {
            if (DataTransfer.CurrentAccount != null)
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + DataTransfer.CurrentAccount.ScreenName, UriKind.Relative));
        }

    }
}