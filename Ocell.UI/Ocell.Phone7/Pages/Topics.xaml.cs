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
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); }; 
            var viewModel = new TopicsModel();
            DataContext = viewModel;
            viewModel.ShowLocationsPicker += viewModel_ShowLocationsPicker;
            ThemeFunctions.SetBackground(LayoutRoot);
        }

        void viewModel_ShowLocationsPicker(object sender, EventArgs e)
        {
            LocPicker.Open();
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
