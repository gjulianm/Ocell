using AncoraMVVM.Base;
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;

namespace Ocell.Pages.Elements
{
    [ViewModel(typeof(UserModel))]
    public partial class User : PhoneApplicationPage
    {
        UserModel viewModel { get { return DataContext as UserModel; } }

        public User()
        {
            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
        }

        private void Following_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var target = new UserListParams
            {
                Resource = UserListResource.Following,
                User = viewModel.ScreenName
            };

            Dependency.Resolve<INavigationService>().MessageAndNavigate<UserListModel, UserListParams>(target);
        }

        private void TextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var target = new UserListParams
            {
                Resource = UserListResource.Followers,
                User = viewModel.ScreenName
            };

            Dependency.Resolve<INavigationService>().MessageAndNavigate<UserListModel, UserListParams>(target);
        }

        private void Avatar_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask wb = new WebBrowserTask();
            wb.Uri = new Uri(viewModel.Avatar, UriKind.Absolute);
            wb.Show();
        }
    }
}
