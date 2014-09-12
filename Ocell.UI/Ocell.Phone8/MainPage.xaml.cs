using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using BugSense;
using Microsoft.Phone.Controls;
using Ocell.Compatibility;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Pages;
using Ocell.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TweetSharp;

namespace Ocell
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DateTime LastErrorTime;
        private DateTime LastReloadTime;
        private bool _initialised;
        private MainPageModel viewModel;

        public MainPage()
        {
            _initialised = false;
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            viewModel = new MainPageModel();
            DataContext = viewModel;
            this.Loaded += CallLoadFunctions;

            LastErrorTime = DateTime.MinValue;
            LastReloadTime = DateTime.MinValue;

            CreateStoryboards();
            viewModel.ShowRecoverPositionPrompt = ShowResumePositionPrompt;

            SetupRecoverDialogGestures();
        }

        #region Page events
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataTransfer.IsGlobalFilter = false;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string column;
            NavigationContext.QueryString.TryGetValue("column", out column);
            viewModel.OnNavigation(column);

            base.OnNavigatedTo(e);
        }

        private void CallLoadFunctions(object sender, RoutedEventArgs e)
        {
            if (_initialised)
                return;

            if (!CheckForLogin())
                return;

            BugSenseHandler.Instance.RegisterAsyncHandlerContext();

            viewModel.RaiseLoggedInChange();
            viewModel.OnLoad();

            GeolocationPrompt();
            ShowFollowMessage();

            if (Config.PushEnabled == true || (Config.PushEnabled == null && AskForPushPermission()))
                PushNotifications.AutoRegisterForNotifications();

            ThreadPool.QueueUserWorkItem((threadContext) =>
            {
                CreateTile();
                var task = UsernameProvider.DownloadAndCacheFriends(Config.Accounts.Value);
            });

            _initialised = true;
        }

        #endregion Page events

        #region Prompts
        private bool AskForPushPermission()
        {
            if (!TrialInformation.IsFullFeatured)
                return false;

            var result = Dependency.Resolve<INotificationService>().Prompt(Localization.Resources.AskEnablePush);
            Config.PushEnabled = result;
            return result;
        }

        private void GeolocationPrompt()
        {
            if (Config.EnabledGeolocation.Value != null)
                return;

            string boxText = Localization.Resources.AskAccessGrantGeolocation + Environment.NewLine + Environment.NewLine;
            boxText += Localization.Resources.PrivacyPolicy + Environment.NewLine + Environment.NewLine;
            boxText += Localization.Resources.ChangeGeolocSetting;

            var result = MessageBox.Show(boxText, Localization.Resources.Geolocation, MessageBoxButton.OKCancel);

            Config.EnabledGeolocation.Value = result == MessageBoxResult.OK;
        }

        private bool CheckForLogin()
        {
            if (!Config.Accounts.Value.Any())
            {
                var service = Dependency.Resolve<INotificationService>();
                bool result = service.Prompt(Localization.Resources.YouHaveToLogin);
                if (result)
                {
                    OAuth.Type = AuthType.Twitter;
                    NavigationService.Navigate(Uris.LoginPage);
                }
                return false;
            }
            else
                return true;
        }

        private void ShowFollowMessage()
        {
            if ((Config.FollowMessageShown.Value == false || Config.FollowMessageShown.Value == null) && ServiceDispatcher.CanGetServices)
            {
                var service = Dependency.Resolve<INotificationService>();
                bool result = service.Prompt(Localization.Resources.FollowOcellAppMessage);
                if (result)
                    ServiceDispatcher.GetDefaultService().FollowUserAsync(new FollowUserOptions { ScreenName = "OcellApp" });
                Config.FollowMessageShown.Value = true;
            }
        }

        #endregion Prompts

        #region List management
        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            if (list == null)
                return;

            var context = list.DataContext as ColumnModel;

            if (context == null)
                return;

            context.Listbox = list;

            list.AutoReload();

            Dispatcher.BeginInvoke(() => list.Loaded -= ListBox_Loaded);

            viewModel.PreloadedLists++;
        }
        #endregion List management

        #region RecoverDialog
        private Storyboard SbShowDialog;
        private Storyboard SbHideDialog;

        private TranslateTransform trans;
        private void CreateStoryboards()
        {
            trans = new TranslateTransform() { X = 0, Y = 0 };
            RecoverDialog.RenderTransformOrigin = new Point(0, 0);
            RecoverDialog.RenderTransform = trans;
            trans = RecoverDialog.RenderTransform as TranslateTransform;

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

        private ColumnModel currentShowingList;
        private void RecoverDialog_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HideResumePositionPrompt();

            if (currentShowingList != null)
                currentShowingList.RecoverPosition();
        }

        private bool recoverDialogShown = false;
        private void ShowResumePositionPrompt(ColumnModel model)
        {
            currentShowingList = model;

            if (recoverDialogShown)
                return;


            Dispatcher.BeginInvoke(() =>
            {
                SbShowDialog.BeginTime = TimeSpan.FromSeconds(2);
                SbShowDialog.Begin();
            });

            recoverDialogShown = true;
        }

        private void SbShowDialog_Completed(object sender, EventArgs e)
        {
            HideResumePositionPrompt(true);
        }

        private void HideResumePositionPrompt(bool delay = false)
        {
            if (!recoverDialogShown)
                return;

            if (delay)
                SbHideDialog.BeginTime = TimeSpan.FromSeconds(8);
            else
                SbHideDialog.BeginTime = TimeSpan.FromSeconds(0);

            SbHideDialog.Begin();
        }

        private void SetupRecoverDialogGestures()
        {
            RecoverDialog.ManipulationDelta += RecoverDiag_DragDelta;
            RecoverDialog.ManipulationCompleted += RecoverDiag_DragEnd;
        }

        private void RecoverDiag_DragDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ((TranslateTransform)RecoverDialog.RenderTransform).X += e.DeltaManipulation.Translation.X;
        }

        private void RecoverDiag_DragEnd(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.TotalManipulation.Translation.X != 0)
            {
                // Nice, it was a flick which moved the dialog laterally. Now, let's see if the user moved it enough to hide it.
                var moveNeeded = 0.40; // 40% of the dialog must have been moved to the right.
                var actualMove = e.TotalManipulation.Translation.X / RecoverDialog.Width;

                if (actualMove >= moveNeeded)
                    HideResumePositionPrompt(false);
                else
                {
                    SbShowDialog.BeginTime = TimeSpan.FromSeconds(0);
                    SbShowDialog.Begin();
                }

                e.Handled = true;
            }
        }

        #endregion RecoverDialog

        #region UserGrid
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

        #endregion UserGrid

        private void CreateTile()
        {
            SchedulerSync.WriteLastCheckDate(DateTime.Now.ToUniversalTime());
            SchedulerSync.StartPeriodicAgent();
            Dependency.Resolve<TileManager>().ClearMainTileCount();
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var textblock = sender as TextBlock;

            if (textblock == null)
                return;

            var context = textblock.DataContext as ColumnModel;

            if (context != null)
                textblock.Tap += (s, r) => context.ScrollToTop();
        }
    }
}