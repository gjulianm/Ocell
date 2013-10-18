using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Ocell.Library.Twitter;
using Ocell.Library;
using TweetSharp;

namespace Ocell.Pages.Lists
{
    public partial class ListManager : PhoneApplicationPage
    {
        private string _userName;
        private ITwitterService _srv;
        private bool _selectionChangeFired;
        private int _pendingCalls;
        public ListManager()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };



            _selectionChangeFired = false;
            NewList.Click += new RoutedEventHandler(NewList_Click);
            _pendingCalls = 0;
            this.Loaded += new RoutedEventHandler(ListManager_Loaded);
        }

        void NewList_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(Uris.CreateList);
        }

        void ListManager_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NavigationContext.QueryString.TryGetValue("user", out _userName))
            {
                NavigationService.GoBack();
                return;
            }

            _srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);

            LoadListsIn();
            LoadUserLists();
        }

        private async void LoadUserLists()
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            _pendingCalls++;
            var response = await _srv.ListListsForAsync(new ListListsForOptions { ScreenName = DataTransfer.CurrentAccount.ScreenName });

            if (!response.RequestSucceeded)
            {
                MessageBox.Show(Localization.Resources.ErrorLoadingLists);
                return;
            }

            var lists = response.Content;

            ListsUser.ItemsSource = lists;
            _pendingCalls--;
            if (_pendingCalls <= 0)
                pBar.IsVisible = false;

        }

        private async void LoadListsIn()
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            _pendingCalls++;
            var response = await _srv.ListListMembershipsForAsync(new ListListMembershipsForOptions { ScreenName = _userName, FilterToOwnedLists = true, Cursor = -1 });

            var lists = response.Content;
            if (!response.RequestSucceeded)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.ErrorLoadingLists));
                return;
            }

            Dispatcher.BeginInvoke(() => ListsIn.ItemsSource = lists);
            _pendingCalls--;
            if (_pendingCalls <= 0)
                Dispatcher.BeginInvoke(() => pBar.IsVisible = false);

        }

        private async void ListsIn_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_selectionChangeFired)
            {
                TwitterList list = null;
                if (e.AddedItems.Count > 0)
                    list = e.AddedItems[0] as TwitterList;
                if (list != null)
                {
                    Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
                    var response = await _srv.RemoveListMemberAsync(new RemoveListMemberOptions { OwnerScreenName = list.User.ScreenName, Slug = list.Slug, ScreenName = _userName });

                    LoadListsIn();
                    if (response.RequestSucceeded)
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show(String.Format(Localization.Resources.RemovedFromList, _userName, list.FullName)));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.ErrorMessage));
                    }
                    Dispatcher.BeginInvoke(() => pBar.IsVisible = false);

                }
                _selectionChangeFired = true;
                Dispatcher.BeginInvoke(() => ListsIn.SelectedIndex = -1);
            }
            else
            {
                _selectionChangeFired = false;
            }
        }

        private async void ListsUser_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_selectionChangeFired)
            {
                TwitterList list = null;
                if (e.AddedItems.Count > 0)
                    list = e.AddedItems[0] as TwitterList;
                if (list != null)
                {
                    Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
                    var response = await _srv.AddListMemberAsync(new AddListMemberOptions { ScreenName = _userName, Slug = list.Slug, OwnerScreenName = list.User.ScreenName });

                    LoadListsIn();
                    if (response.RequestSucceeded)
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show(String.Format(Localization.Resources.AddedToList, _userName, list.FullName)));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.ErrorMessage));
                    }
                    Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
                }
                _selectionChangeFired = true;
                Dispatcher.BeginInvoke(() => ListsUser.SelectedIndex = -1);
            }
            else
            {
                _selectionChangeFired = false;
            }
        }
    }
}
