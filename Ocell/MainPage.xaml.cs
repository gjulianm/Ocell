using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DanielVaughan;
using DanielVaughan.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Twitter;
using Ocell.Localization;

namespace Ocell
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DateTime LastErrorTime;
        private DateTime LastReloadTime;
        private bool _initialised;
        private MainPageModel viewModel;

        // Constructora
        public MainPage()
        {
            _initialised = false;
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
            

            viewModel = new MainPageModel();
            DataContext = viewModel;

            ThemeFunctions.SetBackground(LayoutRoot);

            this.Loaded += new RoutedEventHandler(CallLoadFunctions);

            LastErrorTime = DateTime.MinValue;
            LastReloadTime = DateTime.MinValue;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            viewModel.RaiseNavigatedTo(this, e);
            base.OnNavigatedTo(e);
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
#if DEBUG
                var list = DebugWriter.ReadAll();
                if (list != null)
                {
                    EmailComposeTask email = new EmailComposeTask();
                    email.To = "gjulian93@gmail.com";
                    email.Subject = "Ocell Error Report";
                    string contents = "";
                    foreach (var line in list)
                        contents += line + Environment.NewLine;
                    email.Body = contents;
                    //Dispatcher.BeginInvoke(() => email.Show());
                    DebugWriter.Clear();
                    DebugWriter.Save();
                }
#endif
                LittleWatson.CheckForPreviousException();
            });

            _initialised = true;
        }

        void ShowFollowMessage()
        {
            if ((Config.FollowMessageShown == false || Config.FollowMessageShown == null) && ServiceDispatcher.CanGetServices)
            {
                var service = Dependency.Resolve<IMessageService>();
                bool result = service.AskYesNoQuestion(Localization.Resources.FollowOcellAppMessage, "");
                if (result)
                    ServiceDispatcher.GetDefaultService().FollowUser("OcellApp", (a, b) => { });
                Config.FollowMessageShown = true;
            }
        }

        void CreateTile()
        {
            SchedulerSync.WriteLastCheckDate(DateTime.Now.ToUniversalTime());
            SchedulerSync.StartPeriodicAgent();
            TileManager.ClearTile();
        }

        bool CheckForLogin()
        {
            if (!Config.Accounts.Any())
            {
                var service = Dependency.Resolve<IMessageService>();
                bool result = service.AskYesNoQuestion(Localization.Resources.YouHaveToLogin, "");
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

                list.Loader.ActivateLoadMoreButton = true;
                list.Loader.TweetsToLoadPerRequest = (int)Config.TweetsPerRequest;
                list.Loader.LoadRetweetsAsMentions = (bool)Config.RetweetAsMentions;
                list.Loader.PropertyChanged += (sender1, e1) =>
                {
                    if (e1.PropertyName == "IsLoading")
                    {
                        if (list.Loader.IsLoading)
                            viewModel.LoadingCount++;
                        else
                            viewModel.LoadingCount--;
                    }
                };
                viewModel.ScrollToTop += (sender1, e1) =>
                {
                    if (e1.BroadcastAll || e1.Resource == Resource)
                        list.ScrollToTop();
                };

                viewModel.ReloadLists += (sender1, e1) =>
                {
                    if (e1.BroadcastAll || e1.Resource == Resource)
                        ThreadPool.QueueUserWorkItem((context) => list.AutoReload());
                };

                list.Loader.LoadCacheAsync();
                list.AutoReload();

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

        private void menuItem1_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(Uris.Settings);
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataTransfer.IsGlobalFilter = false;
            base.OnNavigatedFrom(e);
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var box = sender as TextBlock;
            if (box != null && box.Tag is TwitterResource)
                viewModel.RaiseScrollToTop((TwitterResource)box.Tag);
        }

        private void myprofile_Click(object sender, System.EventArgs e)
        {
            if (DataTransfer.CurrentAccount != null)
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + DataTransfer.CurrentAccount.ScreenName, UriKind.Relative));
        }


        private void HideUserGrid(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.IsSearching = false;
            e.Cancel = true;
            this.BackKeyPress -= HideUserGrid;
        }


        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            viewModel.IsSearching = true;
            this.BackKeyPress += HideUserGrid;
        }
    }
}