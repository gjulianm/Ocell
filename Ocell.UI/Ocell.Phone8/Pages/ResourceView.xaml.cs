using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.Pages.Search;
using System.Windows;

namespace Ocell.Pages
{
    [ViewModel(typeof(ResourceViewModel))]
    public partial class ResourceView : PhoneApplicationPage
    {
        public ResourceView()
        {
            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
        }

        private void TweetList_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as ResourceViewModel).Listbox = TweetList;

            TweetList.AutoManageNavigation = true;
            TweetList.ActivatePullToRefresh = true;
        }
    }
}