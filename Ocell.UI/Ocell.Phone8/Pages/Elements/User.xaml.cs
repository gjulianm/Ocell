using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Twitter;
using System;

namespace Ocell.Pages.Elements
{
    public partial class User : PhoneApplicationPage
    {
        UserModel viewModel;

        public User()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            this.Loaded += (sender, e) =>
                {
                    viewModel = DataContext as UserModel;
                    string userName;
                    if (!NavigationContext.QueryString.TryGetValue("user", out userName))
                    {
                        // TODO: Move this to messaging.
                        NavigationService.GoBack();
                        return;
                    }
                    viewModel.Loaded(userName);
                };

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
            NavigationService.Navigate(new Uri("/Pages/Elements/UserList.xaml?resource=following&user=" + viewModel.ScreenName, UriKind.Relative));
        }

        private void TextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Elements/UserList.xaml?resource=followers&user=" + viewModel.ScreenName, UriKind.Relative));

        }

        private void Avatar_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WebBrowserTask wb = new WebBrowserTask();
            wb.Uri = new Uri(viewModel.Avatar, UriKind.Absolute);
            wb.Show();
        }
    }
}
