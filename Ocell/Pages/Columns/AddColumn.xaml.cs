using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;



namespace Ocell.AuxScreens
{
    public partial class AddColumn : PhoneApplicationPage
    {
        public ObservableCollection<TwitterList> lists;
        public AddColumn()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(AddColumn_Loaded);
        }

        void AddColumn_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLists();
        }

        private void CoreList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;
            ObservableCollection<TwitterResource> columns;
            TwitterResource toAdd = new TwitterResource(); 
            ListBoxItem item = e.AddedItems[0] as ListBoxItem;

            if (!config.TryGetValue<ObservableCollection<TwitterResource>>("COLUMNS", out columns))
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Error adding column."); });
                return;
            }
            toAdd.String = item.Tag as string;
            if (!columns.Contains(toAdd))
            {
                columns.Add(toAdd);
                config["COLUMNS"] = columns;
                config.Save();

            } NavigationService.GoBack();
        }

        private void ListsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;
            ObservableCollection<TwitterResource> columns;
            TwitterResource toAdd;
            TwitterList item = e.AddedItems[0] as TwitterList;

            if (!config.TryGetValue<ObservableCollection<TwitterResource>>("COLUMNS", out columns))
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Error adding column."); });
                return;
            }

            toAdd = new TwitterResource { Type = ResourceType.List, Data = item.FullName };
            if (!columns.Contains(toAdd))
            {
                columns.Add(toAdd);
                config["COLUMNS"] = columns;
                config.Save();
            }
            NavigationService.GoBack();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            LoadLists();
        }

        private void LoadLists()
        {
            if (Clients.isServiceInit && Clients.ScreenName != null)
            {
                Dispatcher.BeginInvoke(() => { pBar.IsVisible = true; });
                Clients.Service.ListListsFor(Clients.ScreenName, -1, (tlist, resp) =>
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