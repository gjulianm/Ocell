using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Linq;
using Ocell.Library;

namespace Ocell.Settings
{
    public partial class Default : PhoneApplicationPage
    {
        private bool _selectionChangeFired;
        private int _ProgramaticallyFiredChange = 0;

        public Default()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Default_Loaded);
            Users.SelectionChanged += new SelectionChangedEventHandler(Users_SelectionChanged);
            Accounts_Not.SelectionChanged += new SelectionChangedEventHandler(Accounts_Not_SelectionChanged);
            MessagesPicker.SelectionChanged += new SelectionChangedEventHandler(MessagesPicker_SelectionChanged);
            MentionsPicker.SelectionChanged += new SelectionChangedEventHandler(MentionsPicker_SelectionChanged);
            _selectionChangeFired = false;
        }

        void MentionsPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedIndex = Accounts_Not.SelectedIndex;
            if (SelectedIndex == -1)
                return;

            if (_ProgramaticallyFiredChange > 0)
            {
                _ProgramaticallyFiredChange--;
                return;
            }

            Config.Accounts[SelectedIndex].Preferences.MentionsPreferences = (NotificationType)MentionsPicker.SelectedIndex;
            Config.SaveAccounts();
            BindAccounts();
        }

        void MessagesPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedIndex = Accounts_Not.SelectedIndex;
            if (SelectedIndex == -1)
                return;

            if (_ProgramaticallyFiredChange > 0)
            {
                _ProgramaticallyFiredChange--;
                return;
            }

            Config.Accounts[SelectedIndex].Preferences.MessagesPreferences = (NotificationType)MessagesPicker.SelectedIndex;
            Config.SaveAccounts();
            BindAccounts();
        }

        void Accounts_Not_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            UserToken User = e.AddedItems[0] as UserToken;

            if (User != null)
            {
                if (MentionsPicker.SelectedIndex != (int)User.Preferences.MentionsPreferences)
                {
                    _ProgramaticallyFiredChange++;
                    MentionsPicker.SelectedIndex = (int)User.Preferences.MentionsPreferences;
                }
                if (MessagesPicker.SelectedIndex != (int)User.Preferences.MessagesPreferences)
                {
                    _ProgramaticallyFiredChange++;
                    MessagesPicker.SelectedIndex = (int)User.Preferences.MessagesPreferences;
                }
            }
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
            foreach (var item in Config.Columns)
            {
            	if(item.User == Account)
                	Config.Columns.Remove(item);
            }
            Config.SaveColumns();
        }

        void Default_Loaded(object sender, RoutedEventArgs e)
        {
            BindAccounts();
            //Accounts_Not.SelectedIndex = 0;
        }

        private void BindAccounts()
        {
            if (Config.Accounts != null)
            {
                Users.DataContext = Config.Accounts;
                Users.ItemsSource = Config.Accounts;
                
                Accounts_Not.DataContext = Config.Accounts;
                Accounts_Not.ItemsSource = Config.Accounts;
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