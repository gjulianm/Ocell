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
        private bool _selectionChangeFired;
        private int _ProgramaticallyFiredChange = 0;
        private bool _selectionChangeFiredPicker;

        public Default()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(Default_Loaded);
            Users.SelectionChanged += new SelectionChangedEventHandler(Users_SelectionChanged);
            Accounts_Not.SelectionChanged += new SelectionChangedEventHandler(Accounts_Not_SelectionChanged);
            MessagesPicker.SelectionChanged += new SelectionChangedEventHandler(MessagesPicker_SelectionChanged);
            MentionsPicker.SelectionChanged += new SelectionChangedEventHandler(MentionsPicker_SelectionChanged);
            _selectionChangeFired = false;
            _selectionChangeFiredPicker = false;
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
                        MessageBoxResult Result = MessageBox.Show("Do you really want to delete the account " + Account.ScreenName + "?", "", MessageBoxButton.OKCancel);
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
            try
            {
                UnsafeRemoveColumnsAssociatedWith(Account);
            }
            catch (Exception)
            {
            }
        }

        void UnsafeRemoveColumnsAssociatedWith(UserToken Account)
        {
            if (Config.Columns.Count == 0 || Account == null)
                return;
            foreach (var item in Config.Columns)
            {
                if (item.User == Account)
                    Config.Columns.Remove(item);
            }
            Config.SaveColumns();
        }

        void Default_Loaded(object sender, RoutedEventArgs e)
        {
            BindAccounts();

            ColumnUpdate.IsChecked = Config.BackgroundLoadColumns;
            RetweetsInMentions.IsChecked = Config.RetweetAsMentions;
            ComposePin.IsEnabled = !SecondaryTiles.ComposeTileIsCreated();
            if (Config.TweetsPerRequest == null)
                Config.TweetsPerRequest = 40;
            tweetsPerReq.Text = Config.TweetsPerRequest.ToString();

            if (Config.Accounts != null && Config.Accounts.Count > 0)
                Accounts_Not.SelectedIndex = 0;

            if (SilencePicker != null) UpdateListPicker();

            if (Config.ReadLaterCredentials.Pocket != null)
            {
                PocketPass.Password = Config.ReadLaterCredentials.Pocket.Password;
                PocketUser.Text = Config.ReadLaterCredentials.Pocket.User;
            }
            if (Config.ReadLaterCredentials.Instapaper != null)
            {
                IPPass.Password = Config.ReadLaterCredentials.Instapaper.Password;
                IPUser.Text = Config.ReadLaterCredentials.Instapaper.User;
            }
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
            NavigationService.Navigate(Uris.LoginPage);
        }

        private void ComposePin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SecondaryTiles.CreateComposeTile();
        }

        private void ColumnUpdate_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Config.BackgroundLoadColumns = ColumnUpdate.IsChecked;
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
                BindAccounts();
                Users.UpdateLayout();
            });
        }

        private void FilterClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DataTransfer.cFilter = Config.GlobalFilter;
            DataTransfer.IsGlobalFilter = true;
            NavigationService.Navigate(Uris.Filters);
        }

        private void RetweetsInMentions_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Config.RetweetAsMentions = (bool)RetweetsInMentions.IsChecked;
        }

        private void tweetsPerReq_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int num;
            if (int.TryParse(tweetsPerReq.Text, out num))
            {
                Config.TweetsPerRequest = num;
            }
        }

        private void ListPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_selectionChangeFiredPicker)
            {
                if (SilencePicker == null)
                    return;

                switch (SilencePicker.SelectedIndex)
                {
                    case 0:
                        Config.DefaultMuteTime = TimeSpan.FromHours(1);
                        break;
                    case 1:
                        Config.DefaultMuteTime = TimeSpan.FromHours(8);
                        break;
                    case 2:
                        Config.DefaultMuteTime = TimeSpan.FromDays(1);
                        break;
                    case 3:
                        Config.DefaultMuteTime = TimeSpan.FromDays(7);
                        break;
                    case 4:
                        Config.DefaultMuteTime = TimeSpan.MaxValue;
                        break;
                }
            }
            else
            {
                _selectionChangeFiredPicker = false;
            }
        }

        private void UpdateListPicker()
        {
            _selectionChangeFiredPicker = true;
            if (Config.DefaultMuteTime == TimeSpan.FromHours(1))
                SilencePicker.SelectedIndex = 0;
            else if (Config.DefaultMuteTime == TimeSpan.FromHours(8))
                SilencePicker.SelectedIndex = 1;
            else if (Config.DefaultMuteTime == TimeSpan.FromDays(1))
                SilencePicker.SelectedIndex = 2;
            else if (Config.DefaultMuteTime == TimeSpan.FromDays(7))
                SilencePicker.SelectedIndex = 3;
            else
                SilencePicker.SelectedIndex = 4;
        }

        private void SaveRLBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AuthPair PocketPair = null;
            AuthPair InstapaperPair = null;

            if (!string.IsNullOrWhiteSpace(PocketUser.Text))
            {
                Dispatcher.BeginInvoke(() => { pBar.Text = "Verifying credentials..."; pBar.IsVisible = true; });
                PocketPair = new AuthPair { User = PocketUser.Text, Password = PocketPass.Password };
                var service = new PocketService { UserName = PocketPair.User, Password = PocketPair.Password };
                service.CheckCredentials((valid, response) =>
                {
                    if (valid)
                    {
                        Dispatcher.BeginInvoke(() => { Notificator.ShowMessage("Credentials saved.", pBar); });
                        Config.ReadLaterCredentials.Pocket = PocketPair;
                        Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                    }
                    else
                        Dispatcher.BeginInvoke(() => { pBar.IsVisible = false; MessageBox.Show("Invalid Pocket credentials."); });
                });
            }
            else
            {
                Config.ReadLaterCredentials.Pocket = null;
                Config.ReadLaterCredentials = Config.ReadLaterCredentials;
            }

            if (!string.IsNullOrWhiteSpace(IPUser.Text))
            {
                Dispatcher.BeginInvoke(() => { pBar.Text = "Verifying credentials..."; pBar.IsVisible = true; });
                InstapaperPair = new AuthPair { User = IPUser.Text, Password = IPPass.Password };
                var service = new InstapaperService { UserName = InstapaperPair.User, Password = InstapaperPair.Password };
                service.CheckCredentials((valid, response) =>
                {
                    if (valid)
                    {
                        Dispatcher.BeginInvoke(() => { Notificator.ShowMessage("Credentials saved.", pBar); });
                        Config.ReadLaterCredentials.Instapaper = InstapaperPair;
                        Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                    }
                    else
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Invalid Instapaper credentials."));
                });
            }
            else
            {
                Config.ReadLaterCredentials.Pocket = null;
                Config.ReadLaterCredentials = Config.ReadLaterCredentials;
            }
        }
    }
}