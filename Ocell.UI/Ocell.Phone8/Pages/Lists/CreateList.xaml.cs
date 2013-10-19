using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using TweetSharp;
using Ocell.Library.Twitter;
using Ocell.Library;

namespace Ocell.Pages.Lists
{
    public partial class CreateList : PhoneApplicationPage
    {
        public CreateList()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

        }

        private async void CreateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ITwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            TwitterListMode mode;
            if (PublicBtn.IsChecked == true)
                mode = TwitterListMode.Public;
            else
                mode = TwitterListMode.Private;
            pBar.IsVisible = true;

            var response = await srv.CreateListAsync(new CreateListOptions { ListOwner = DataTransfer.CurrentAccount.ScreenName, Name = ListName.Text, Description = ListDescp.Text, Mode = mode });

            if (response.RequestSucceeded)
            {
                MessageBox.Show(Localization.Resources.ListCreated);
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show(Localization.Resources.ErrorCreatingList);
            }

            pBar.IsVisible = false;
        }
    }
}
