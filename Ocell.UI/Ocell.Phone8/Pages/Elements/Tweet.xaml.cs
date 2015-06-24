
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ocell.Helpers;
using Ocell.Library;
using Ocell.Library.Twitter;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using Ocell.Library.RuntimeData;
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
                else if (ev.PropertyName == "IsMuting")
                {
                    if (ViewModel.IsMuting)
                        this.BackKeyPress += HideMuteGrid;
                    else
                        this.BackKeyPress -= HideMuteGrid;
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
                User = ApplicationData.CurrentAccount
            };

            if (conversation.Loader == null)
            {
                conversation.Loader = new TweetLoader(resource);
            }

            if (conversation.Loader.Resource != resource)
            {
                conversation.Loader.Source.Clear();
                conversation.Loader.Resource = resource;
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

        void HideMuteGrid(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            ViewModel.IsMuting = false;
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