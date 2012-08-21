using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.Pages.Search
{
    public partial class Search : PhoneApplicationPage
    {
        private SearchModel viewModel;

        public Search()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
            viewModel = new SearchModel();
            DataContext = viewModel;

            ThemeFunctions.SetBackground(LayoutRoot);
        }

        private void TweetList_Loaded(object sender, RoutedEventArgs e)
        {
            string query;
            if (!NavigationContext.QueryString.TryGetValue("q", out query) || string.IsNullOrWhiteSpace(query))
                if ((query = DataTransfer.Search) == null)
                    NavigationService.GoBack();

            string fromForm;
            if (NavigationContext.QueryString.TryGetValue("form", out fromForm) && fromForm == "1")
                NavigationService.RemoveBackEntry();

            viewModel.Query = query;
            viewModel.Loader = TweetList.Loader;

            TweetList.AutoManageNavigation = true;
            TweetList.ActivatePullToRefresh = true;
        }
    }
}