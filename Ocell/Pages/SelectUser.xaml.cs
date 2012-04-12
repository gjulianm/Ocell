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
using System.Collections.ObjectModel;

namespace Ocell.Pages
{
    public partial class SelectUser : PhoneApplicationPage
    {
        public CollectionViewSource Source;
        private UserProvider _provider;
        private ObservableCollection<TwitterUser> _users;

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

            if(_provider == null)
                _provider = new UserProvider();
            _users = new ObservableCollection<TwitterUser>();
            _provider.GetFollowers = true;
            _provider.GetFollowing = false;
            _provider.Users.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Users_CollectionChanged);
            Source.Source = _users;
            Source.SortDescriptions.Add(new System.ComponentModel.SortDescription("ScreenName", System.ComponentModel.ListSortDirection.Ascending));

            UserList.DataContext = Source;
            UserList.ItemsSource = Source.View;
        }

        void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    foreach (var item in e.NewItems.Cast<TwitterUser>())
                        if (!_users.Contains(item))
                            _users.Add(item);
                });
            }

            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    foreach (var item in e.OldItems.Cast<TwitterUser>())
                        if (_users.Contains(item))
                            _users.Remove(item);
                });
            }
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
            if (_provider == null)
                _provider = new UserProvider();
            Dispatcher.BeginInvoke(() => { pBar.IsVisible = true; pBar.Text = "Downloading list of users..."; });
            _provider.User = User;
            _provider.GetFollowers = true;
            _provider.GetFollowing = false;
            _provider.Finished += new EventHandler(_provider_Finished);
            _provider.Start();
        }

        void _provider_Finished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }

        private void Next_Click(object sender, System.EventArgs e)
        {
            DataTransfer.ReplyingDM = true; ;
            NavigationService.Navigate(new Uri("/Pages/NewTweet.xaml?removeBack=1", UriKind.Relative));
        }
    }
}