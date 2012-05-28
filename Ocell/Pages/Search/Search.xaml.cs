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
        private bool selectionChangeFired = false;

        public Search()
        {
            InitializeComponent();

            viewModel = new SearchModel();
            DataContext = viewModel;

            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
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

        private void Add_Click(object sender, System.EventArgs e)
        {
            if (!Config.Columns.Contains(TweetList.Loader.Resource))
            {
                DataTransfer.ShouldReloadColumns = true;
                Config.Columns.Add(TweetList.Loader.Resource);
                Dispatcher.BeginInvoke(() => MessageBox.Show("Search column added!"));
            }
            else
                Dispatcher.BeginInvoke(() => MessageBox.Show("This search is already added."));
        }
    }
}