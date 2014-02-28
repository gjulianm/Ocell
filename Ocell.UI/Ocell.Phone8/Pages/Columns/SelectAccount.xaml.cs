using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell
{
    // TODO: Where am I using this?
    public partial class SelectAccount : PhoneApplicationPage
    {
        public SelectAccount()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            this.Loaded += new RoutedEventHandler(SelectAccount_Loaded);
        }

        void SelectAccount_Loaded(object sender, RoutedEventArgs e)
        {
            AccountsList.DataContext = Config.Accounts.Value;
            AccountsList.ItemsSource = Config.Accounts.Value;

            AccountsList.SelectionChanged += new SelectionChangedEventHandler(AccountsList_SelectionChanged);
        }

        void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            UserToken Account = e.AddedItems[0] as UserToken;
            if (Account == null)
                Account = Config.Accounts.Value[0];
            DataTransfer.CurrentAccount = Account;
            Dispatcher.BeginInvoke(() =>
                NavigationService.Navigate(Uris.AddColumn));
        }
    }
}
