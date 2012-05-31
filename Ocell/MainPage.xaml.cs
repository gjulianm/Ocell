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
using DanielVaughan.Services;
using DanielVaughan;


namespace Ocell
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ObservableCollection<TwitterResource> pivots;
        private Dictionary<string, ExtendedListBox> Lists;
        private DateTime LastErrorTime;
        private DateTime LastReloadTime;
        private bool _initialised;

        #region Loaders
        // Constructora
        public MainPage()
        {
            _initialised = false;
            InitializeComponent();

            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(CallLoadFunctions);

            LastErrorTime = DateTime.MinValue;
            LastReloadTime = DateTime.MinValue;
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
            if (_initialised)
                return;

            if(!CheckForLogin())
                return;

            ThreadPool.QueueUserWorkItem((threadContext) =>
            {
                CreateTile();
                ShowFollowMessage();
                LittleWatson.CheckForPreviousException();
                if (Config.DefaultMuteTime == null || Config.DefaultMuteTime == TimeSpan.FromHours(0))
                    Config.DefaultMuteTime = TimeSpan.FromHours(8);
            });

            _initialised = true;
        }

        void ShowFollowMessage()
        {
            if ((Config.FollowMessageShown == false || Config.FollowMessageShown == null) && ServiceDispatcher.CanGetServices)
            {
                var service = Dependency.Resolve<IMessageService>();
                bool result = service.AskYesNoQuestion("Do you want to follow @OcellApp on Twitter to receive updates?", "");
                if (result)
                    ServiceDispatcher.GetDefaultService().FollowUser("OcellApp", (a, b) => { });
                Config.FollowMessageShown = true;
            }
        }

        void CreateTile()
        {
            SchedulerSync.WriteLastCheckDate(DateTime.Now.ToUniversalTime());
            SchedulerSync.StartPeriodicAgent();
            TileManager.UpdateTile(null, null); // Updating with null means it will clear the tile.
        }

        #endregion

        bool CheckForLogin()
        {
            if (!Config.Accounts.Any())
            {
                var service = Dependency.Resolve<IMessageService>();
                bool result = service.AskYesNoQuestion("You have to log in with Twitter in order to use Ocell.", "");
                if (result)
                    NavigationService.Navigate(Uris.LoginPage);
                return false;
            }
            else
                return true;
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            if (list == null)
                return;
            var tag = list.Tag;

            ThreadPool.QueueUserWorkItem((threadcontext) =>
            {
                TwitterResource Resource = new TwitterResource();

                if (tag is TwitterResource)
                {
                    Resource = (TwitterResource)tag;
                    list.Bind(Resource);
                }

                Dispatcher.BeginInvoke(() => FilterManager.SetupFilter(list));

                if (Lists.ContainsKey(Resource.String))
                    return;

                Lists.Add(Resource.String, list);

                list.Compression += new ExtendedListBox.OnCompression(list_Compression);
                list.Loader.Error += new TweetLoader.OnError(Loader_Error);
                list.Loader.LoadFinished += new EventHandler(Loader_LoadFinished);
                list.Loader.ActivateLoadMoreButton = true;

                list.Loader.TweetsToLoadPerRequest = (int)Config.TweetsPerRequest;
                list.Loader.LoadRetweetsAsMentions = (bool)Config.RetweetAsMentions;

                Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
                list.Loader.LoadCacheAsync();
                list.Loader.Load();

                Dispatcher.BeginInvoke(() =>
                {
                    list.Loaded -= ListBox_Loaded;
                    list.Loaded += new RoutedEventHandler(CheckForFilterUpdate);
                });

                GlobalEvents.FiltersChanged += (sender1, e1) => Dispatcher.BeginInvoke(() => FilterManager.SetupFilter(list));
            });
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

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivot.DataContext = pivots;
            MainPivot.ItemsSource = pivots;
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
                if (Lists.TryGetValue(resource.String, out list) && list.ItemsSource != null && list.Loader.Source.Any())
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

        private void GoToUserBtn_Click(object sender, System.EventArgs e)
        {
            Dispatcher.BeginInvoke(() => GoToUserGrid.Visibility = Visibility.Visible);
            this.BackKeyPress += HideUserGrid;
        }
        private void HideUserGrid(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher.BeginInvoke(() => GoToUserGrid.Visibility = Visibility.Collapsed);
            e.Cancel = true;
            this.BackKeyPress -= HideUserGrid;
        }

        private void GoUser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => GoToUserGrid.Visibility = Visibility.Collapsed);
            NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + UserNameBox.Text, UriKind.Relative));
        }
    }
}