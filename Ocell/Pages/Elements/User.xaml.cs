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
using Ocell.Library.Twitter;

namespace Ocell.Pages.Elements
{
    public partial class User : PhoneApplicationPage
    {
        public TwitterUser CurrentUser;
        private bool follows;
        private bool selectionChangeFired = false;
        ApplicationBarIconButton followBtn;
        ApplicationBarIconButton pinBtn;
        ApplicationBarMenuItem changeAvatarItem;
        private TwitterService _srv;
        private UserToken _account;

        public User()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(User_Loaded);
            TweetList.SelectionChanged += new SelectionChangedEventHandler(ListBox_SelectionChanged);
            TweetList.Loader.Cached = false;
            _account = Config.Accounts.FirstOrDefault();
            if (_account == null)
                NavigationService.Navigate(Uris.MainPage);
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
            _srv.ListUserProfilesFor(new List<string> { userName }, GetUser);
            pvProfile.DataContext = CurrentUser;
            if (followBtn == null)
            {
                followBtn = new ApplicationBarIconButton();
                followBtn.Text = "follow";
                followBtn.IconUri = new Uri("/Images/Icons_White/appbar.minus.rest.png", UriKind.Relative);
                followBtn.Click += new EventHandler(followBtn_Click);
                followBtn.IsEnabled = false;
                ApplicationBar.Buttons.Add(followBtn);
            }

            if (pinBtn == null)
            {
                pinBtn = new ApplicationBarIconButton();
                pinBtn.Text = "pin to main page";
                pinBtn.IconUri = new Uri("/Images/Icons_White/appbar.pin.rest.png", UriKind.Relative);
                pinBtn.Click += new EventHandler(pinBtn_Click);
                pinBtn.IsEnabled = false;
                ApplicationBar.Buttons.Add(pinBtn);
            }
        }

        void pinBtn_Click(object sender, EventArgs e)
        {
            TwitterResource resource = new TwitterResource {
                Type = ResourceType.Tweets,
                Data = CurrentUser.ScreenName,
                User = DataTransfer.CurrentAccount
            };

            // Don't care about the User property of the TwitterResource.
            if(Config.Columns.Any(item => item != null && item.Type == ResourceType.Tweets && item.Data == CurrentUser.ScreenName))
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("That user is already pinned!"));
                return;
            }

            Config.Columns.Add(resource);
            Config.SaveColumns();
            DataTransfer.ShouldReloadColumns = true;
            Dispatcher.BeginInvoke(() => Notificator.ShowMessage("User succesfully pinned!", pBar));
        }

        void GetUser(IEnumerable<TwitterUser> user, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK || user == null || user.Count() == 0)
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
                pinBtn.IsEnabled = true;

                FullName.Text = CurrentUser.Name;
                ScreenName.Text = "@" + CurrentUser.ScreenName;
                Avatar.Source = new BitmapImage(new Uri(CurrentUser.ProfileImageUrl, UriKind.Absolute));

                if (!string.IsNullOrWhiteSpace(CurrentUser.Url))
                {
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

            _srv.GetFriendshipInfo((int)_account.Id, CurrentUser.Id, GetFriendship);
            CheckForAvatarChange();
        }

        private void CheckForAvatarChange()
        {
            if (Config.Accounts.Any(item => item.Id == CurrentUser.Id) && changeAvatarItem == null)
            {
                changeAvatarItem = new ApplicationBarMenuItem();
                changeAvatarItem.IsEnabled = true;
                changeAvatarItem.Text = "change profile image";
                changeAvatarItem.Click += new EventHandler(ChangeAvatar);
                Dispatcher.BeginInvoke(() => ApplicationBar.MenuItems.Add(changeAvatarItem));
            }
        }

        void ChangeAvatar(object sender, EventArgs e)
        {
            PhotoChooserTask task = new PhotoChooserTask();
            task.ShowCamera = true;
            task.Completed += new EventHandler<PhotoResult>(task_Completed);
            task.Show();
        }

        void task_Completed(object sender, PhotoResult e)
        {
            UserToken user;
            user = Config.Accounts.FirstOrDefault(item => item != null && item.ScreenName == CurrentUser.ScreenName);
            if (e.TaskResult == TaskResult.OK && user != null)
            {
                Dispatcher.BeginInvoke(() => { pBar.IsVisible = true; pBar.Text = "Uploading picture..."; });
                TwitterService srv = ServiceDispatcher.GetService(user);
                srv.UpdateProfileImage(e.OriginalFileName, e.ChosenPhoto, ReceivePhotoUpload);
            }
        }

        private void ReceivePhotoUpload(TwitterUser user, TwitterResponse response)
        {
            Dispatcher.BeginInvoke(() => { pBar.IsVisible = false; pBar.Text = ""; });
            if (response.StatusCode == HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => MessageBox.Show("Your profile image has been changed."));
            else
                Dispatcher.BeginInvoke(() => MessageBox.Show("An error happened while uploading your image. Verify the file is less than 700 kB of size."));
        }

        private void GetFriendship(TwitterFriendship friendship, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK || followBtn == null)
                return;


            followBtn.IsEnabled = true;

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

            if (list.Tag != null && list.Tag is string && CurrentUser != null)
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
            if (CurrentUser == null)
                return;
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

                NavigationService.Navigate(Uris.ViewTweet);
            }
            else
                selectionChangeFired = false;
        }

        private void spamBtn_Click(object sender, System.EventArgs e)
        {
            ServiceDispatcher.GetCurrentService().ReportSpam(CurrentUser.Id, (user, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                    Dispatcher.BeginInvoke(() => Notificator.ShowMessage("Blocked and reported user.", pBar));
                else
                    Dispatcher.BeginInvoke(() => Notificator.ShowMessage("User could not be reported.", pBar));
            });
        }

        private void blockBtn_Click(object sender, System.EventArgs e)
        {
            ServiceDispatcher.GetCurrentService().BlockUser(CurrentUser.Id, (user, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                    Dispatcher.BeginInvoke(() => Notificator.ShowMessage("Blocked user.", pBar));
                else
                    Dispatcher.BeginInvoke(() => Notificator.ShowMessage("User could not be blocked.", pBar));
            });
        }

        private void ManageLists_Click(object sender, System.EventArgs e)
        {
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Pages/Lists/ListManager.xaml?user=" + CurrentUser.ScreenName, UriKind.Relative)));
        }
    }
}
