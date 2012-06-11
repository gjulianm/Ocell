using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Linq;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Library.Notifications;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Library.ReadLater;

namespace Ocell.Settings
{
    public partial class Default : PhoneApplicationPage
    {
        public Default()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
            DataContext = new DefaultModel();
        }
        

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MenuItem Item = sender as MenuItem;
                ProtectedConverter Converter = new ProtectedConverter();
                UserToken User;
                if (Item != null)
                {
                    User = Item.CommandParameter as UserToken;
                    if (User != null)
                        Item.Header = Converter.Convert(User, null, null, null);
                }
                Users.UpdateLayout();
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            (DataContext as DefaultModel).Navigated();
            base.OnNavigatedTo(e);
        }
    }
}