using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Controls;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.Pages.Elements
{
    public partial class User : PhoneApplicationPage
    {
        UserModel viewModel;

        public User()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
            
            

            viewModel = new UserModel();
            DataContext = viewModel;

            this.Loaded += (sender, e) =>
                {
                    string userName;
                    if (!NavigationContext.QueryString.TryGetValue("user", out userName))
                    {
                        NavigationService.GoBack();
                        return;
                    }
                    viewModel.Loaded(userName);
                };

            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ScreenName")
                {
                    TweetList.Resource = new TwitterResource { String = "Tweets:" + viewModel.ScreenName, User = DataTransfer.CurrentAccount};
                    TweetList.Load();
                    MentionsList.Resource = new TwitterResource { Data = "@" + viewModel.ScreenName, Type = ResourceType.Search, User = DataTransfer.CurrentAccount };
                    MentionsList.Load();
                }
            };

            TweetList.Loader.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsLoading")
                        viewModel.IsLoading = TweetList.Loader.IsLoading;
                };

            MentionsList.Loader.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsLoading")
                    viewModel.IsLoading = MentionsList.Loader.IsLoading;
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
