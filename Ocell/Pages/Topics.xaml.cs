using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using Ocell.Library.Twitter;

namespace Ocell.Pages
{
    public partial class Topics : PhoneApplicationPage
    {
        public Topics()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(Topics_Loaded);
        }

        void Topics_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                pBar.IsVisible = true;
                pBar.Text = "Downloading trending topics...";
            });
            ServiceDispatcher.GetDefaultService().ListLocalTrendsFor(1, ReceiveTrends);
        }

        private void ReceiveTrends(TweetSharp.TwitterTrends Trends, TweetSharp.TwitterResponse Response)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error loading trending topics. Sorry :(");
                    NavigationService.GoBack();
                });
                return;
            }

            Dispatcher.BeginInvoke(() =>
            {
                TList.DataContext = Trends;
                TList.ItemsSource = Trends;
            });
        }

        private void TList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TweetSharp.TwitterTrend Trend = null;
            if (e.AddedItems != null && e.AddedItems.Count > 0 && (Trend = e.AddedItems[0] as TweetSharp.TwitterTrend) != null)
            {
                string EscapedQuery = Uri.EscapeDataString(Trend.Name);
                NavigationService.Navigate(new Uri("/Pages/Search/Search.xaml?q=" + EscapedQuery, UriKind.Relative));
            }
        }
    }
}
