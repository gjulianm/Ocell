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

namespace Ocell
{
    public partial class SelectAccount : PhoneApplicationPage
    {
        public SelectAccount()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(SelectAccount_Loaded);
        }

        void SelectAccount_Loaded(object sender, RoutedEventArgs e)
        {
            AccountsList.DataContext = Config.Accounts;
            AccountsList.ItemsSource = Config.Accounts;

            AccountsList.SelectionChanged += new SelectionChangedEventHandler(AccountsList_SelectionChanged);
        }

        void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            UserToken Account = e.AddedItems[0] as UserToken;
            if (Account == null)
                Account = Config.Accounts[0];
            DataTransfer.CurrentAccount = Account;
            Dispatcher.BeginInvoke(() =>
                NavigationService.Navigate(new Uri("/Pages/Columns/AddColumn.xaml", UriKind.Relative)));
        }
    }
}
