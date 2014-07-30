
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ocell.Helpers;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    [ViewModel(typeof(TweetModel))]
    public partial class Tweet : PhoneApplicationPage
    {
        Storyboard sbShow;
        Storyboard sbHide;
        Storyboard UserListShow;
        Storyboard UserListHide;
        bool conversationLoaded = false;

        private TweetModel ViewModel { get { return DataContext as TweetModel; } }

        public Tweet()
        {
            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            this.Loaded += new RoutedEventHandler(Tweet_Loaded);

            panorama.SelectionChanged += (sender, e) =>
            {
                var pano = panorama.SelectedItem as PanoramaItem;

                if (pano == null)
                    return;

                string tag = pano.Tag as string;
                if (tag == "conversation" && !conversationLoaded)
                {
                    conversation.Load();
                    conversationLoaded = true;
                }
            };
        }

        void Tweet_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.TweetSent += (s, ev) =>
            {
                conversation.Loader.Source.Insert(0, ev.Payload);
                TBNoFocus();
            };

            sbShow = this.Resources["sbShow"] as Storyboard;
            sbHide = this.Resources["sbHide"] as Storyboard;
            UserListShow = this.Resources["UserListShow"] as Storyboard;
            UserListHide = this.Resources["UserListHide"] as Storyboard;

            ViewModel.PropertyChanged += (s, ev) =>
            {
                if (ev.PropertyName == "UserList")
                {
                    if (ViewModel.UserList != null)
                        Dispatcher.BeginInvoke(() => UserListShow.Begin());
                    else
                        Dispatcher.BeginInvoke(() => UserListHide.Begin());
                }
            };

            if (ViewModel.ShowWebLink && Uri.IsWellFormedUriString(ViewModel.WebUrl, UriKind.Absolute))
            {
                Dependency.Resolve<IProgressIndicator>().IsLoading = true;
                WebBrowser.Navigate(new Uri(ViewModel.WebUrl, UriKind.Absolute));
            }

            Initialize();

            if (ApplicationBar != null)
                ApplicationBar.MatchOverriddenTheme();

            conversation.Loader.PropertyChanged += (s, ea) =>
            {
                if (ea.PropertyName == "IsLoading")
                    Dependency.Resolve<IProgressIndicator>().IsLoading = conversation.Loader.IsLoading;
            };
        }

        void Initialize()
        {
            CreateText(ViewModel.Tweet);
            ViewModel.Completed = true;

            TwitterResource resource = new TwitterResource
            {
                Data = ViewModel.Tweet.Id.ToString(),
                Type = ResourceType.Conversation,
                User = DataTransfer.CurrentAccount
            };

            if (conversation.Loader == null)
            {
                conversation.Loader = new TweetLoader(resource);
            }

            if (conversation.Loader.Resource != resource)
            {
                conversation.Loader.Source.Clear();
                conversation.Resource = resource;
            }

            conversation.Loader.Cached = false;
            conversation.AutoManageNavigation = true;
        }

        private void CreateText(ITweetable Status)
        {
            if (Status == null)
                return;

            var formatter = new TweetRTBFormatter(Status, Text);
            formatter.Format();
        }

        private void AuthorTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.NavigateToAuthor.Execute(null);
        }

        private ITweetableFilter CreateNewFilter(FilterType type, string data)
        {
            if (Config.GlobalFilter.Value == null)
                Config.GlobalFilter.Value = new ColumnFilter();

            if (Config.DefaultMuteTime.Value == null)
                Config.DefaultMuteTime.Value = TimeSpan.FromHours(8);

            ITweetableFilter filter = new ITweetableFilter();
            filter.Type = type;
            filter.Filter = data;
            if (Config.DefaultMuteTime.Value == TimeSpan.MaxValue)
                filter.IsValidUntil = DateTime.MaxValue;
            else
                filter.IsValidUntil = DateTime.Now + (TimeSpan)Config.DefaultMuteTime.Value;
            filter.Inclusion = IncludeOrExclude.Exclude;

            Config.GlobalFilter.Value.AddFilter(filter);
            Config.GlobalFilter.Value = Config.GlobalFilter.Value;

            return filter;
        }

        private void MuteUser_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var filter = FilterManager.SetupMute(FilterType.User, ViewModel.Tweet.Author.ScreenName);
            Dependency.Resolve<INotificationService>().
                ShowMessage(String.Format(Localization.Resources.UserIsMutedUntil, ViewModel.Tweet.Author.ScreenName, filter.IsValidUntil.ToString("f")));
            ViewModel.IsMuting = false;
        }

        private void MuteHashtags_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ITweetableFilter filter = null;
            string message = "";
            foreach (var entity in ViewModel.Tweet.Entities)
            {
                if (entity.EntityType == TwitterEntityType.HashTag)
                {
                    filter = FilterManager.SetupMute(FilterType.Text, ((TwitterHashTag)entity).Text);
                    message += ((TwitterHashTag)entity).Text + ", ";
                }
            }
            if (message == "")
                Dependency.Resolve<INotificationService>().ShowMessage(Localization.Resources.NoHashtagsToMute);
            else
                Dependency.Resolve<INotificationService>().
                ShowMessage(String.Format(Localization.Resources.HashtagsMutedUntil, message.Substring(0, message.Length - 2), filter.IsValidUntil.ToString("f")));
            ViewModel.IsMuting = false;
        }

        private void Source_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RemoveHTML conv = new RemoveHTML();
            string source = conv.Convert(ViewModel.Tweet.Source, null, null, null) as string;
            var filter = FilterManager.SetupMute(FilterType.Source, source);
            Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.SourceMutedUntil, source, filter.IsValidUntil.ToString("f"))); // TODO: Refactor this already.
            ViewModel.IsMuting = false;
        }

        void HideMuteGrid(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            ViewModel.IsMuting = false;
            this.BackKeyPress -= HideMuteGrid;
        }

        private void MuteBtn_Click(object sender, EventArgs e)
        {
            this.BackKeyPress += HideMuteGrid;
            ViewModel.IsMuting = true;
        }

        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ViewModel.ImageFailed(sender, e);
        }

        private void ImageOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.ImageOpened(sender, e);
        }

        private void ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModel.ImageTapped(sender, e);
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TBFocus();
        }

        public void TBFocus()
        {
            sbShow.Begin();
            ViewModel.ReplyBoxGotFocus();
            textBox.SelectionStart = textBox.Text.Length;
        }

        public void TBNoFocus()
        {
            Dispatcher.BeginInvoke(() =>
            {
                sbHide.Begin();
            });
        }
        bool suppressTBFocusLost = false;
        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((c) =>
            {
                // LostFocus gets triggered before the button click, so wait a little bit and let it be triggered.
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() =>
                {
                    if (!suppressTBFocusLost)
                        TBNoFocus();
                    else
                        suppressTBFocusLost = false;
                });
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            suppressTBFocusLost = true;
        }

        private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Dependency.Resolve<IProgressIndicator>().IsLoading = false;
        }

        private void panorama_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (panorama.SelectedItem == TweetPano)
                ViewModel.AppBarMode = ApplicationBarMode.Default;
            else
                ViewModel.AppBarMode = ApplicationBarMode.Minimized;
        }
    }
}