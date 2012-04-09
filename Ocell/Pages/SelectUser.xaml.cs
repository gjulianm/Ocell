using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;

namespace Ocell.Pages
{
    public partial class SelectUser : PhoneApplicationPage
    {
        public CollectionViewSource Source;

        public SelectUser()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            Source = new CollectionViewSource();

            AccountsPicker.SelectionChanged += new SelectionChangedEventHandler(AccountsPicker_SelectionChanged);
            UserList.SelectionChanged += new SelectionChangedEventHandler(UserList_SelectionChanged);
            UserFilter.TextChanged += new TextChangedEventHandler(UserFilter_TextChanged);

            AccountsPicker.DataContext = Config.Accounts;
            AccountsPicker.ItemsSource = Config.Accounts;
            
            UserList.DataContext = Source;
            UserList.ItemsSource = Source.View;
        }

        void UserFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Source != null && Source.View != null && UserFilter != null)
            {
                Source.View.Filter = new Predicate<object>(item => (item != null)
                    && (item is TwitterUser)
                    && (item as TwitterUser).ScreenName.ToLowerInvariant().Contains(UserFilter.Text.ToLowerInvariant()));
            }
        }

        void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0)
                return;
            DataTransfer.DMDestinationId = (e.AddedItems[0] as TwitterUser).Id;
        }

        void AccountsPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            DataTransfer.CurrentAccount = AccountsPicker.SelectedItem as UserToken;
            GetUsersFor(AccountsPicker.SelectedItem as UserToken);
        }

        void GetUsersFor(UserToken User)
        {
            TwitterService Service;
            Service = ServiceDispatcher.GetService(User);
            pBar.IsVisible = true;
            pBar.Text = "Downloading list of users...";
            Service.ListFollowers(ReceiveUsers);
        }

        void ReceiveUsers(TwitterCursorList<TwitterUser> Users, TwitterResponse Response)
        {
            Dispatcher.BeginInvoke(() =>
            {
                pBar.IsVisible = false;
            });
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    Source.Source = Users.ToList();
                    UserList.ItemsSource = Source.View;
                });
            }
            else
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("There was a problem trying to get the list of users.")) ;
            }
        }

        private void Next_Click(object sender, System.EventArgs e)
        {
            DataTransfer.ReplyingDM = true; ;
            NavigationService.Navigate(new Uri("/Pages/NewTweet.xaml?removeBack=1", UriKind.Relative));
        }
    }
}