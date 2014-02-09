using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

#if WP8

#endif

namespace Ocell.Pages
{
    public partial class NewTweet : PhoneApplicationPage
    {
        protected bool SendingDM;
        public ApplicationBarIconButton SendButton;
        private Autocompleter _completer;
        NewTweetModel viewModel = new NewTweetModel();


        public NewTweet()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                if (ApplicationBar != null)
                    ApplicationBar.MatchOverriddenTheme();
                viewModel.TryLoadDraft();
            };


            DataContext = viewModel;

            Loaded += NewTweet_Loaded;
            Unloaded += NewTweet_Unloaded;
            TweetBox.TextChanged += OnTextBoxTextChanged;

            GeolocImg.Tap += (sender, e) =>
                {
                    viewModel.IsGeotagged = !viewModel.IsGeotagged;
                };

            viewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsGeotagged")
                    {
                        if (viewModel.IsGeotagged)
                            Dependency.Resolve<IDispatcher>().InvokeIfRequired(EnableGeoloc.Begin);
                        else
                            Dependency.Resolve<IDispatcher>().InvokeIfRequired(DisableGeoloc.Begin);
                    }
                };
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

            if (ListAccounts.SelectedItems != null)
            {
                if (DataTransfer.Draft != null && DataTransfer.Draft.Accounts != null)
                {
                    foreach (var account in DataTransfer.Draft.Accounts.Where(x => x != null))
                        ListAccounts.SelectedItems.Add(account);
                }
                else
                {
                    if (!ListAccounts.SelectedItems.Contains(DataTransfer.CurrentAccount))
                        ListAccounts.SelectedItems.Add(DataTransfer.CurrentAccount);
                }
            }


            // TODO: Fix this with autocompleter.
            //_completer = new Autocompleter();
            //_completer.User = DataTransfer.CurrentAccount;
            //_completer.Textbox = TweetBox;
            _completer.Trigger = '@';
            viewModel.Completer = _completer;

            // Update the UI.
            if (viewModel.IsGeotagged)
                Dependency.Resolve<IDispatcher>().InvokeIfRequired(EnableGeoloc.Begin);
            else
                Dependency.Resolve<IDispatcher>().InvokeIfRequired(DisableGeoloc.Begin);
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
                return;

            var user = img.Tag as UserToken;

            UpdateOpacity(img);

            if (img.Opacity == 1)
            {
                // TODO: Fix.
                //_completer.User = user;
                if (!viewModel.SelectedAccounts.Contains(user))
                    viewModel.SelectedAccounts.Add(user);
            }
            else
            {
                if (viewModel.SelectedAccounts.Contains(user))
                    viewModel.SelectedAccounts.Remove(user);
            }
        }

        private void UpdateOpacity(Image img)
        {
            if (img.Opacity < 0.60) // FUCK FLOAT.
                img.Opacity = 1;
            else
                img.Opacity = 0.55;

            img.UpdateLayout();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            if (img == null || (img.Tag as UserToken) == null)
                return;

            UserToken usr = img.Tag as UserToken;
            if (usr == DataTransfer.CurrentAccount
                || (DataTransfer.Draft != null && DataTransfer.Draft.Accounts.Contains(usr)))
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            TweetBox.Focus();
            base.OnNavigatedTo(e);
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New && NavigationContext.QueryString.ContainsKey("voiceCommandName"))
                StartSpeechToText();
        }

        private async void StartSpeechToText()
        {
            var recognizer = new VoiceReco.VoiceRecognizer();

            var result = await recognizer.GetDictatedText();

            if (result.SuccesfulRecognition)
            {
                viewModel.TweetText = result.Text;

                if (!result.UserCancelled && viewModel.SendTweet.CanExecute(null))
                    viewModel.SendTweet.Execute(null);
            }
        }

        private void UserSuggestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = UserSuggestions.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selected))
                return;

            _completer.UserChoseElement(selected);

            UserSuggestions.SelectedItem = null;
        }

    }
}