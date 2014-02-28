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
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Settings
{
    [ViewModel(typeof(SettingsModel))]
    public partial class Default : PhoneApplicationPage
    {
        public Default()
        {
            InitializeComponent(); 
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
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
            (DataContext as SettingsModel).Navigated();
            base.OnNavigatedTo(e);
        }
    }
}