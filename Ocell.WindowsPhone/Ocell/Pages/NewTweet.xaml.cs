using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Hammock;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using TweetSharp;
using System.Collections.Generic;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Library.Tasks;
using System.Windows.Data;

namespace Ocell.Pages
{
    public partial class NewTweet : PhoneApplicationPage
    {
        protected bool SendingDM;
        public ApplicationBarIconButton SendButton;
        private UsernameProvider _provider;
        private Autocompleter _completer;
        NewTweetModel viewModel = new NewTweetModel();


        public NewTweet()
        {
            InitializeComponent(); 
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); }; 

            ThemeFunctions.SetBackground(LayoutRoot);
            DataContext = viewModel;

            Loaded += NewTweet_Loaded;
            Unloaded += NewTweet_Unloaded;

            TweetBox.TextChanged += OnTextBoxTextChanged;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            // Update the binding source
            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();
        }

        void NewTweet_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_provider != null)
            {
                _provider.Stop();
                _provider.Usernames.Clear();
            }
            DataTransfer.Text = viewModel.TweetText;
            DataTransfer.ReplyingDM = false;
            if (DataTransfer.Draft != null)
            {
                DataTransfer.Draft = viewModel.CreateDraft();
                viewModel.SaveAsDraft(null);
            }
        }

        void NewTweet_Loaded(object sender, RoutedEventArgs e)
        {          
            string RemoveBack;
            if (NavigationContext.QueryString.TryGetValue("removeBack", out RemoveBack) || RemoveBack == "1")
            {
                NavigationService.RemoveBackEntry();
            }

            if (DataTransfer.Draft != null)
            {
                foreach (var account in DataTransfer.Draft.Accounts)
                    ListAccounts.SelectedItems.Add(account);
            }
            else
            {
                ListAccounts.SelectedItems.Add(DataTransfer.CurrentAccount);
            }

            
            _completer = new Autocompleter();
            _completer.User = DataTransfer.CurrentAccount;
            _completer.Textbox = TweetBox;
            _completer.Trigger = '@';
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
                return;

            UpdateOpacity(img);

            if (img.Opacity == 1)
            {
                _completer.User = img.Tag as UserToken;
            }
        }

        private void UpdateOpacity(Image img)
        {
            if (img.Opacity == 0.75)
                img.Opacity = 1;
            else
                img.Opacity = 0.75;

            img.UpdateLayout();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            if (img == null || (img.Tag as UserToken) == null)
                return;

            UserToken usr = img.Tag as UserToken;
            if (usr == DataTransfer.CurrentAccount || (DataTransfer.Draft != null && DataTransfer.Draft.Accounts.Contains(usr)))
                UpdateOpacity(img);
        }

        private void aboutScheduling_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.AboutSchedulingMessage));
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedAccounts = (sender as ListBox).SelectedItems;
        }
    }
}