using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Ocell.Controls;
using TweetSharp;


namespace Ocell
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings config;
        private ObservableCollection<TwitterResource> pivots;
        private bool selectionChangeFired;
        private Dictionary<string, ExtendedListBox> Lists;

        // Constructora
        public MainPage()
        {
            InitializeComponent();

            pivots = new ObservableCollection<TwitterResource>();
            Lists = new Dictionary<string, ExtendedListBox>();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            pivots.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(pivots_CollectionChanged);
            MainPivot.SelectionChanged += new SelectionChangedEventHandler(MainPivot_SelectionChanged);

            BindPivots();
        }

        void BindPivots()
        {
            MainPivot.DataContext = pivots;
            MainPivot.ItemsSource = pivots;
        }

        void GetPivotsFromConf()
        {
            config = IsolatedStorageSettings.ApplicationSettings;

            ObservableCollection<TwitterResource> pv;

            if (!config.TryGetValue<ObservableCollection<TwitterResource>>("COLUMNS", out pv))
                Dispatcher.BeginInvoke(() => MessageBox.Show("Error loading columns."));

            foreach (var pivot in pv)
                if (!pivots.Contains(pivot))
                    pivots.Add(pivot);
        }

        void pivots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BindPivots();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Clients.isServiceInit)
            {
                // The TwitterService object is not initialised. Start it!
                if (string.IsNullOrWhiteSpace(Tokens.user_token) || string.IsNullOrWhiteSpace(Tokens.user_secret))
                {
                    // There are no credentials. Redirect the user.
                    NavigationService.Navigate(new Uri("/Pages/Settings/OAuth.xaml", UriKind.Relative));
                    return;
                }
            }

            Clients.Service = new TweetSharp.TwitterService(Tokens.consumer_token, Tokens.consumer_secret, Tokens.user_token, Tokens.user_secret);
            Clients.isServiceInit = true;
            Clients.fillScreenName();

            GetPivotsFromConf();
            BindPivots();
        }
        
        void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExtendedListBox ListBox;
            TwitterResource Resource = (TwitterResource) MainPivot.SelectedItem;
            if (Lists.TryGetValue(Resource.String, out ListBox))
            {
                Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
                ListBox.Loader.Load();
            }
        }

        private void compose_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/NewTweet.xaml", UriKind.Relative));
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ExtendedListBox list = sender as ExtendedListBox;
            ResourceToString Converter = new ResourceToString();
            TwitterResource Resource = new TwitterResource();

            if(list==null)
                return;

            if(list.Tag != null && list.Tag is string)
            {
                Resource.String = list.Tag as string;
                list.Bind(Resource);
            }

            if (!Lists.ContainsKey(list.Tag as string))
                Lists.Add(list.Tag as string, list);

            list.Compression += new ExtendedListBox.OnCompression(list_Compression);
            list.Loader.Error += new TweetLoader.OnError(Loader_Error);
            list.Loader.LoadFinished += new TweetLoader.OnLoadFinished(Loader_LoadFinished);
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            list.Loader.LoadCache();
            list.Loader.Load();
        }

        void Loader_LoadFinished()
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }

        void Loader_Error(TwitterResponse response)
        {
            Dispatcher.BeginInvoke(() => {
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

        private void menuItem1_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings/Default.xaml", UriKind.Relative));
        }

        private void add_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Columns/ManageColumns.xaml", UriKind.Relative));
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                DataTransfer.DM = e.AddedItems[0] as TwitterDirectMessage;
                ListBox list = sender as ListBox;
                selectionChangeFired = true;
                list.SelectedIndex = -1;
                if (e.AddedItems[0] is TwitterStatus)
                    NavigationService.Navigate(new Uri("/Pages/Tweet.xaml", UriKind.Relative));
                else
                    NavigationService.Navigate(new Uri("/Pages/DMView.xaml", UriKind.Relative));
            }
            else
                selectionChangeFired = false;
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivot.DataContext = pivots;
            MainPivot.ItemsSource = pivots;
        }
    }
}