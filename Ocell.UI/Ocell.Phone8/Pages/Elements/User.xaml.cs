using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Twitter;
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

            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ScreenName")
                {
                    TweetList.Resource = new TwitterResource { String = "Tweets:" + viewModel.ScreenName, User = DataTransfer.CurrentAccount };
                    TweetList.Load();
                    MentionsList.Resource = new TwitterResource { Data = "@" + viewModel.ScreenName, Type = ResourceType.Search, User = DataTransfer.CurrentAccount };
                    MentionsList.Load();
                }
            };

            TweetList.Loader.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsLoading")
                    Dependency.Resolve<IProgressIndicator>().IsLoading = TweetList.Loader.IsLoading;
            };

            MentionsList.Loader.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsLoading")
                    Dependency.Resolve<IProgressIndicator>().IsLoading = MentionsList.Loader.IsLoading;
            };

            TweetList.Loader.Cached = false;
            MentionsList.Loader.Cached = false;
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
