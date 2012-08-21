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
            
            ThemeFunctions.SetBackground(LayoutRoot);

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
                    TweetList.Bind(new TwitterResource { String = "Tweets:" + viewModel.ScreenName, User = DataTransfer.CurrentAccount });
                    TweetList.Loader.Load();
                }
            };

            TweetList.Loader.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsLoading")
                        viewModel.IsLoading = TweetList.Loader.IsLoading;
                };

            TweetList.Loader.Cached = false;
        }

        private void Following_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Elements/UserList.xaml?resource=following&user=" + viewModel.ScreenName, UriKind.Relative));
        }

        private void TextBlock_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Elements/UserList.xaml?resource=followers&user=" + viewModel.ScreenName, UriKind.Relative));
        
        }
    }
}
