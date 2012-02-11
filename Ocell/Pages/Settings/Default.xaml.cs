using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Linq;

namespace Ocell.Settings
{
    public partial class Default : PhoneApplicationPage
    {
        private bool _selectionChangeFired;
        public Default()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Default_Loaded);
            Users.SelectionChanged += new SelectionChangedEventHandler(Users_SelectionChanged);
            _selectionChangeFired = false;
        }

        void Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_selectionChangeFired)
            {
                if (e.AddedItems.Count == 0)
                    return;
                UserToken Account = e.AddedItems[0] as UserToken;
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBoxResult Result = MessageBox.Show("Do you really want to delete the account " + Account.ScreenName +"?", "", MessageBoxButton.OKCancel);
                        if (Result == MessageBoxResult.OK)
                        {
                            Config.Accounts.Remove(Account);
                            RemoveColumnsAssociatedWith(Account);
                            BindAccounts();
                            Config.SaveAccounts();
                        }
                    });
                _selectionChangeFired = true;
                Users.SelectedIndex = -1;
            }
            else
                _selectionChangeFired = false;
        }

        void RemoveColumnsAssociatedWith(UserToken Account)
        {
            if (Config.Columns.Count == 0 || Account == null)
                return;
            foreach (var item in Config.Columns.Where(item => { return item.User == Account; }))
                Config.Columns.Remove(item);
            Config.SaveColumns();
        }

        void Default_Loaded(object sender, RoutedEventArgs e)
        {
            BindAccounts();
        }

        private void BindAccounts()
        {
            if (Config.Accounts != null)
            {
                Users.DataContext = Config.Accounts;
                Users.ItemsSource = Config.Accounts;
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings/OAuth.xaml", UriKind.Relative));
        }
    }
}