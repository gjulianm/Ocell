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
            ThemeFunctions.SetBackground(LayoutRoot);
        }

        private void CreateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ITwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            TwitterListMode mode;
            if (PublicBtn.IsChecked == true)
                mode = TwitterListMode.Public;
            else
                mode = TwitterListMode.Private;
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            srv.CreateList(new CreateListOptions { ListOwner = DataTransfer.CurrentAccount.ScreenName, Name = ListName.Text, Description = ListDescp.Text, Mode = mode }, (list, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(Localization.Resources.ListCreated);
                        NavigationService.GoBack();
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.ErrorCreatingList));
                }

                Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
            });
        }
    }
}
