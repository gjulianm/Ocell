using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Controls;
using TweetSharp;
using Ocell.Library;

namespace Ocell
{
    public partial class User : PhoneApplicationPage
    {
        public TwitterUser CurrentUser;
        private bool follows;
        private bool selectionChangeFired = false;
        ApplicationBarIconButton followBtn;
        private TwitterService _srv;
        private UserToken _account;
        
        public User()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(User_Loaded);
            TweetList.SelectionChanged += new SelectionChangedEventHandler(ListBox_SelectionChanged);
            TweetList.Loader.Cached = false;
            _account = Config.Accounts.FirstOrDefault();
            if (_account == null)
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            _srv = ServiceDispatcher.GetService(_account);
        }

        void User_Loaded(object sender, RoutedEventArgs e)
        {
            string userName;
            if (!NavigationContext.QueryString.TryGetValue("user", out userName))
                NavigationService.GoBack();

            Regex remove = new Regex("@|:");
            userName = remove.Replace(userName, "");

            Dispatcher.BeginInvoke(() => { pBar.IsVisible = true; pBar.Text = "Retrieving profile..."; });
            _srv.ListUserProfilesFor(new List<string>{userName}, GetUser);
            pvProfile.DataContext = CurrentUser;
            followBtn = new ApplicationBarIconButton();
            followBtn.Text = "follow";
            followBtn.IconUri = new Uri("/Images/Icons_White/appbar.minus.rest.png", UriKind.Relative);
            followBtn.Click +=new EventHandler(followBtn_Click);

            ApplicationBar.Buttons.Add(followBtn);
        }

        void GetUser(IEnumerable<TwitterUser> user, TwitterResponse response)
        {
            if(response.StatusCode != HttpStatusCode.OK || user ==null || user.Count() == 0) 
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("The user could not be retrieved.");
                    pBar.IsVisible = false;
                    pBar.Text = "";
                    NavigationService.GoBack();
                });
                return;
            }

            CurrentUser = user.First();
            Dispatcher.BeginInvoke(() =>
            {
                FullName.Text = CurrentUser.Name;
                ScreenName.Text = "@" + CurrentUser.ScreenName;
                Avatar.Source = new BitmapImage(new Uri(CurrentUser.ProfileImageUrl, UriKind.Absolute));
                if(!string.IsNullOrWhiteSpace(CurrentUser.Url)) {
                    Website.Content = CurrentUser.Url;
                    Website.NavigateUri = new Uri(CurrentUser.Url, UriKind.Absolute);
                }
                Biography.Text = CurrentUser.Description;
                Followers.Text = CurrentUser.FollowersCount.ToString();
                Following.Text = CurrentUser.FriendsCount.ToString();
                Tweets.Text = CurrentUser.StatusesCount.ToString();
                pBar.IsVisible = false;
                pBar.Text = "";

                TweetList.Bind(new TwitterResource { Data = CurrentUser.ScreenName, Type = ResourceType.Tweets, User = DataTransfer.CurrentAccount });
                TweetList.Loader.Load();
            });

            _srv.GetFriendshipInfo((int) _account.Id, CurrentUser.Id, GetFriendship);
        }

        private void GetFriendship(TwitterFriendship friendship, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK || followBtn == null)
                return;

            if (friendship.Relationship.Source.Following)
                Dispatcher.BeginInvoke(() =>
                {
                    followBtn.Text = "unfollow";
                    followBtn.IconUri = new Uri("/Images/Icons_White/appbar.minus.rest.png", UriKind.Relative);
                    follows = true;
                });
            else
                Dispatcher.BeginInvoke(() =>
                {
                    followBtn.Text = "follow";
                    followBtn.IconUri = new Uri("/Images/Icons_White/appbar.add.rest.png", UriKind.Relative);
                    follows = true;
                });

        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            TwitterResource Resource = new TwitterResource();

            if (list == null)
                return;

            if (list.Tag != null && list.Tag is string && CurrentUser!=null)
            {
                if ((string)list.Tag == "Search")
                {
                    Resource.Type = ResourceType.Search;
                    Resource.Data = "@" + CurrentUser.ScreenName;
                }
                else if ((string)list.Tag == "Tweets")
                {
                    Resource.Type = ResourceType.Tweets;
                    Resource.Data = CurrentUser.ScreenName;
                }
                list.Bind(Resource);
            }

            list.Compression += new ExtendedListBox.OnCompression(list_Compression);
            list.Loader.Error += new TweetLoader.OnError(Loader_Error);
            list.Loader.LoadFinished += new EventHandler(Loader_LoadFinished);
        }
        void Loader_LoadFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }

        void Loader_Error(TwitterResponse response)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("Error loading tweets: " + response.StatusDescription);
                pBar.IsVisible = false;
            });
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

        private void Website_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = (sender as HyperlinkButton).NavigateUri;
            browser.Show();
        }

        private void followBtn_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            if (follows)
                ServiceDispatcher.GetCurrentService().UnfollowUser(CurrentUser.Id, Receive);
            else
                ServiceDispatcher.GetCurrentService().FollowUser(CurrentUser.Id, Receive);
        }

        private void Receive(TwitterUser user, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => MessageBox.Show("Error :("));
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
            ServiceDispatcher.GetCurrentService().GetFriendshipInfo((int)DataTransfer.CurrentAccount.Id, CurrentUser.Id, GetFriendship);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                ListBox list = sender as ListBox;
                selectionChangeFired = true;
                list.SelectedIndex = -1;

                NavigationService.Navigate(new Uri("/Pages/Tweet.xaml", UriKind.Relative));
            }
            else
                selectionChangeFired = false;
        }
    }
}
