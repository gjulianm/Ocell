﻿

using Microsoft.Phone.Controls;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
            this.Loaded += new RoutedEventHandler(CallLoadFunctions);

            LastErrorTime = DateTime.MinValue;
            LastReloadTime = DateTime.MinValue;

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
            viewModel.RaiseNavigatedTo(this, e, column);
            base.OnNavigatedTo(e);
        }

        private void CallLoadFunctions(object sender, RoutedEventArgs e)
        {
            if (_initialised)
                return;

            if (!CheckForLogin())
                return;

            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            frame.Background = Config.Background.GetBrush();

            viewModel.RaiseLoggedInChange();
            viewModel.OnLoad();

            GeolocationPrompt();

            CreateStoryboards();

            ThreadPool.QueueUserWorkItem((threadContext) =>
            {
                CreateTile();
                ShowFollowMessage();
                if (Config.PushEnabled == true || (Config.PushEnabled == null && AskForPushPermission()))
                    PushNotifications.AutoRegisterForNotifications();
                UsernameProvider.FillUserNames(Config.Accounts);
#if DEBUG
                var contents = FileAbstractor.ReadContentsOfFile("BA_DEBUG");
                if (!string.IsNullOrEmpty(contents))
                {
                    EmailComposeTask email = new EmailComposeTask();
                    email.To = "gjulian93@gmail.com";
                    email.Subject = "Ocell Background Agent Report";
                    email.Body = contents;
                    Dispatcher.BeginInvoke(() => email.Show());
                }
#endif
                LittleWatson.CheckForPreviousException();
            });

            _initialised = true;
        }

        #endregion Page events

        #region Prompts
        private bool AskForPushPermission()
        {
            if (!TrialInformation.IsFullFeatured)
                return false;

            var result = Dependency.Resolve<IMessageService>().AskYesNoQuestion(Localization.Resources.AskEnablePush);
            Config.PushEnabled = result;
            return result;
        }

        private void GeolocationPrompt()
        {
            if (Config.EnabledGeolocation != null)
                return;

            string boxText = Localization.Resources.AskAccessGrantGeolocation + Environment.NewLine + Environment.NewLine;
            boxText += Localization.Resources.PrivacyPolicy + Environment.NewLine + Environment.NewLine;
            boxText += Localization.Resources.ChangeGeolocSetting;

            var result = MessageBox.Show(boxText, Localization.Resources.Geolocation, MessageBoxButton.OKCancel);

            Config.EnabledGeolocation = result == MessageBoxResult.OK;
        }

        private bool CheckForLogin()
        {
            if (!Config.Accounts.Any())
            {
                var service = Dependency.Resolve<IMessageService>();
                bool result = service.AskYesNoQuestion(Localization.Resources.YouHaveToLogin, "");
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
            if ((Config.FollowMessageShown == false || Config.FollowMessageShown == null) && ServiceDispatcher.CanGetServices)
            {
                var service = Dependency.Resolve<IMessageService>();
                bool result = service.AskYesNoQuestion(Localization.Resources.FollowOcellAppMessage, "");
                if (result)
                    ServiceDispatcher.GetDefaultService().FollowUserAsync(new FollowUserOptions { ScreenName = "OcellApp" });
                Config.FollowMessageShown = true;
            }
        }

        #endregion Prompts

        #region List management
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
                    list.Resource = Resource;
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
                    if (e1.Resource == list.Loader.Resource && Config.ReloadOptions == ColumnReloadOptions.AskPosition)
                        list.TryTriggerResumeReading();
                };

                list.ReadyToResumePosition += (sender1, e1) =>
                {
                    var selectedPivot = (TwitterResource)viewModel.SelectedPivot;
                    if (list.Loader.Resource == selectedPivot)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            long id;
                            if (Config.ReadPositions.TryGetValue(selectedPivot.String, out id)
                                && !list.VisibleItems.Any(x => x.Id == id))
                            {
                                ShowResumePositionPrompt(list);
                            }
                        });
                    }
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

        private void CheckForFilterUpdate(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            if (list != null && ((DataTransfer.ShouldReloadFilters && DataTransfer.cFilter.Resource == list.Loader.Resource) || DataTransfer.IsGlobalFilter))
            {
                FilterManager.SetupFilter(list);
                DataTransfer.ShouldReloadFilters = false;
            }
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

        private ExtendedListBox currentShowingList;
        private void RecoverDialog_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HideResumePositionPrompt();

            if (currentShowingList != null)
                currentShowingList.ResumeReading();
        }

        private bool recoverDialogShown = false;
        private void ShowResumePositionPrompt(ExtendedListBox list)
        {
            currentShowingList = list;

            if (recoverDialogShown)
                return;

            SbShowDialog.BeginTime = TimeSpan.FromSeconds(2);
            SbShowDialog.Begin();

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
                SbHideDialog.BeginTime = TimeSpan.FromSeconds(6);
            else
                SbHideDialog.BeginTime = TimeSpan.FromSeconds(0);

            SbHideDialog.Begin();
        }

        private void SetupRecoverDialogGestures()
        {
            var gestureListener = GestureService.GetGestureListener(RecoverDialog);
            gestureListener.DragDelta += RecoverDiag_DragDelta;
            gestureListener.DragCompleted += RecoverDiag_DragEnd;
        }

        private void RecoverDiag_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            ((TranslateTransform)RecoverDialog.RenderTransform).X += e.HorizontalChange;
        }

        private void RecoverDiag_DragEnd(object sender, DragCompletedGestureEventArgs e)
        {
            if (e.Direction == System.Windows.Controls.Orientation.Horizontal && e.HorizontalChange > 0)
            {
                // Nice, it was a flick which moved the dialog to the right. Now, let's see if the user moved it enough to hide it.
                var moveNeeded = 0.40; // 40% of the dialog must have been moved to the right.
                var actualMove = e.HorizontalChange / RecoverDialog.Width;

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

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var box = sender as TextBlock;
            if (box != null && box.Tag is TwitterResource)
                viewModel.RaiseScrollToTop((TwitterResource)box.Tag);
        }
    }
}