using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Pages.Search
{
    [ViewModel(typeof(ResourceViewModel))]
    public partial class Search : PhoneApplicationPage
    {
        public Search()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
        }

        private void TweetList_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as ResourceViewModel).Listbox = TweetList;

            TweetList.AutoManageNavigation = true;
            TweetList.ActivatePullToRefresh = true;
        }
    }
}