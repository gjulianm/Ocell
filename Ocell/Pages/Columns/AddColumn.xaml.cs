using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;
using System.Linq;
using Ocell.Library;
using Ocell.Library.Twitter;


namespace Ocell.Pages.Columns
{
    public partial class AddColumn : PhoneApplicationPage
    {
        public ObservableCollection<TwitterList> lists;
        private ITwitterService _srv;
        public AddColumn()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
            
            if(ApplicationBar != null) 
                ApplicationBar.MatchOverriddenTheme(); 
            ThemeFunctions.SetBackground(LayoutRoot);

            this.Loaded += new RoutedEventHandler(AddColumn_Loaded);
           
        }

        void AddColumn_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => { NavigationService.RemoveBackEntry(); });
            LoadLists();
        }

        private void CoreList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TwitterResource toAdd = new TwitterResource(); 
            ListBoxItem item = e.AddedItems[0] as ListBoxItem;

            toAdd.String = DataTransfer.CurrentAccount.ScreenName +";" + (item.Tag as string);
            toAdd.User = DataTransfer.CurrentAccount;
            SaveColumn(toAdd);
            DataTransfer.ShouldReloadColumns = true;
            NavigationService.GoBack();
        }

        private void ListsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TwitterResource toAdd;
            TwitterList item = e.AddedItems[0] as TwitterList;

            toAdd = new TwitterResource { Type = ResourceType.List, Data = item.FullName, User = DataTransfer.CurrentAccount };
            SaveColumn(toAdd);
            DataTransfer.ShouldReloadColumns = true;
            NavigationService.GoBack();
        }

        private void SaveColumn(TwitterResource Resource)
        {
            if (!Config.Columns.Contains(Resource))
            {
                Config.Columns.Add(Resource);
                Config.SaveColumns();
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            LoadLists();
        }

        private void LoadLists()
        {
            _srv = ServiceDispatcher.GetCurrentService();
            if (_srv != null)
            {
                Dispatcher.BeginInvoke(() => { pBar.IsVisible = true; });
                _srv.ListListsFor(DataTransfer.CurrentAccount.ScreenName, -1, (tlist, resp) =>
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        lists = new ObservableCollection<TwitterList>(tlist);
                        Dispatcher.BeginInvoke(() =>
                        {
                            ListsList.DataContext = lists;
                            ListsList.ItemsSource = lists;
                            pBar.IsVisible = false;
                        });
                    }
                });
            }
        }
    }
}