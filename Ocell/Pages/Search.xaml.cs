using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;
using Ocell.Library;

namespace Ocell.Pages
{
    public partial class Search : PhoneApplicationPage
    {
        private string query;
        private bool selectionChangeFired = false;
        public Search()
        {
            InitializeComponent(); 
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            TweetList.SelectionChanged += new SelectionChangedEventHandler(ListBox_SelectionChanged);
        }

        private void TweetList_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NavigationContext.QueryString.TryGetValue("q", out query) || string.IsNullOrWhiteSpace(query))
                if ((query = DataTransfer.Search) == null)
                    NavigationService.GoBack();

            Dispatcher.BeginInvoke(() => PTitle.Text = query);
            TweetList.Loader.Resource = new TwitterResource { Type = ResourceType.Search, Data = query, User = Config.Accounts[0] };
            TweetList.Compression += new Controls.ExtendedListBox.OnCompression(TweetList_Compression);
            TweetList.Loader.LoadFinished += new EventHandler(Loader_LoadFinished);
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            TweetList.Loader.Load();
        }

        void TweetList_Compression(object sender, Controls.CompressionEventArgs e)
        {
            bool old = false;
            if (e.Type == Controls.CompressionType.Bottom)
                old = true;
            TweetList.Loader.Load(old);
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
        }

        void Loader_LoadFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!selectionChangeFired)
            {
                DataTransfer.Status = StatusConverter.SearchToStatus(e.AddedItems[0] as TwitterSearchStatus);
                
                ListBox list = sender as ListBox;
                selectionChangeFired = true;
                list.SelectedIndex = -1;

                NavigationService.Navigate(new Uri("/Pages/Tweet.xaml", UriKind.Relative));
            }
            else
                selectionChangeFired = false;
        }

        private void Add_Click(object sender, System.EventArgs e)
        {
            if (!Config.Columns.Contains(TweetList.Loader.Resource))
            {
                Config.Columns.Add(TweetList.Loader.Resource);
                Dispatcher.BeginInvoke(() => MessageBox.Show("Search column added!"));
            }
            else
                Dispatcher.BeginInvoke(() => MessageBox.Show("This search is already added."));
        }
    }
}