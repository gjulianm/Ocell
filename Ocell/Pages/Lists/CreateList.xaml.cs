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
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
        }

        private void CreateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ITwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            string mode;
            if (PublicBtn.IsChecked == true)
                mode = "public";
            else
                mode = "private";
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            srv.CreateList(DataTransfer.CurrentAccount.ScreenName, ListName.Text, ListDescp.Text, mode, (list, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("List created!");
                        NavigationService.GoBack();
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Error when creating the list."));
                }

                Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
            });
        }
    }
}
