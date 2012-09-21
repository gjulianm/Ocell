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
using System.Windows.Media.Animation;
using System.Windows.Media;

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
            string column;
            NavigationContext.QueryString.TryGetValue("column", out column);
            viewModel.RaiseNavigatedTo(this, e, column);
            base.OnNavigatedTo(e);
        }

        bool AskForPushPermission()
        {
            var result = Dependency.Resolve<IMessageService>().AskYesNoQuestion(Localization.Resources.AskEnablePush);
            Config.PushEnabled = result;
            return result;
        }

        void CallLoadFunctions(object sender, RoutedEventArgs e)
        {
            if (_initialised)
                return;

            if (!CheckForLogin())
                return;

            if (!GeolocationPrompt())
                return;

            CreateStoryboards();

            ThreadPool.QueueUserWorkItem((threadContext) =>
            {
                CreateTile();
                ShowFollowMessage();
                if(Config.PushEnabled == true || (Config.PushEnabled == null  && AskForPushPermission()))
                    PushNotifications.RegisterPushChannel();
                UsernameProvider.FillUserNames(Config.Accounts);
#if DEBUG
                var list = Logger.ReadAll();
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
                    Logger.Clear();
                    Logger.Save();
                }
#endif
                LittleWatson.CheckForPreviousException();
            });

            _initialised = true;
        }

        bool GeolocationPrompt()
        {
            if (Config.EnabledGeolocation == null)
            {
                NavigationService.Navigate(new Uri("/Pages/Settings/AskGeolocation.xaml", UriKind.Relative));
                return false;
            }
            return true;
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

        int loadedLists = 0;
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

                viewModel.CheckIfCanResumePosition += (sender1, e1) =>
                {
                    if (e1.Resource == list.Loader.Resource)
                        list.TryTriggerResumeReading();
                };

                list.ReadyToResumePosition += (sender1, e1) =>
                {
                    var resource = (TwitterResource)viewModel.SelectedPivot;
                    if (list.Loader.Resource == resource)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            long id;
                            if (Config.ReadPositions.TryGetValue(resource.String, out id)
                                && !list.GetVisibleItems().Any(x => x.Id == id))
                            {
                                ShowResumePositionPrompt(list);
                            }
                        });
                    }
                };

                // Performance trick. In the first load, the first column will be loaded as fast as possible.
                // Then, when the three first columns are loaded, just load cache inmediately for following columns.
                if (list.Loader.Resource == (TwitterResource)viewModel.SelectedPivot || loadedLists >= 3)
                    list.Loader.LoadCacheAsync();
                else
                    list.Loader.DeferredCacheLoad();
                loadedLists++;

                list.AutoReload();

                Dispatcher.BeginInvoke(() =>
                {
                    list.Loaded -= ListBox_Loaded;
                    list.Loaded += new RoutedEventHandler(CheckForFilterUpdate);
                });

                GlobalEvents.FiltersChanged += (sender1, e1) => Dispatcher.BeginInvoke(() => FilterManager.SetupFilter(list));
            });
        }

        #region RecoverDialog
        Storyboard SbShowDialog;
        Storyboard SbHideDialog;

        void CreateStoryboards()
        {
            TranslateTransform trans = new TranslateTransform() { X = 0, Y = 0 };
            RecoverDialog.RenderTransformOrigin = new Point(0, 0);
            RecoverDialog.RenderTransform = trans;

            SbShowDialog = new Storyboard();
            DoubleAnimation showAnim = new DoubleAnimation();
            showAnim.Duration = TimeSpan.FromMilliseconds(400);
            showAnim.To = -480;
            var easing = new QuadraticEase();
            easing.EasingMode = EasingMode.EaseOut;
            showAnim.EasingFunction = easing;
            Storyboard.SetTarget(showAnim, RecoverDialog);
            Storyboard.SetTargetProperty(showAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            SbShowDialog.Children.Add(showAnim);
            SbShowDialog.Completed += new EventHandler(SbShowDialog_Completed);

            SbHideDialog = new Storyboard();
            DoubleAnimation hideAnim = new DoubleAnimation();
            hideAnim.Duration = TimeSpan.FromMilliseconds(400);
            hideAnim.To = 0;
            hideAnim.EasingFunction = easing;
            Storyboard.SetTarget(hideAnim, RecoverDialog);
            Storyboard.SetTargetProperty(hideAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            SbHideDialog.Children.Add(hideAnim);
        }

        ExtendedListBox currentShowingList;
        private void RecoverDialog_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HideResumePositionPrompt();

            if (currentShowingList != null)
                currentShowingList.ResumeReadPosition();
        }

        bool recoverDialogShown = false;
        void ShowResumePositionPrompt(ExtendedListBox list)
        {
            currentShowingList = list;

            if (recoverDialogShown)
                return;

            SbShowDialog.BeginTime = TimeSpan.FromSeconds(2);
            SbShowDialog.Begin();

            recoverDialogShown = true;
        }

        void SbShowDialog_Completed(object sender, EventArgs e)
        {
            HideResumePositionPrompt(true);
        }

        void HideResumePositionPrompt(bool delay = false)
        {
            if (!recoverDialogShown)
                return;

            if (delay)
                SbHideDialog.BeginTime = TimeSpan.FromSeconds(6);
            else
                SbHideDialog.BeginTime = TimeSpan.FromSeconds(0);

            SbHideDialog.Begin();
        }
        #endregion

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

        void storyboard_Completed(object sender, EventArgs e)
        {

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