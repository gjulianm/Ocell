using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ocell.Pages
{
    [ViewModel(typeof(NewTweetModel))]
    public partial class NewTweet : PhoneApplicationPage
    {
        protected bool SendingDM;
        public ApplicationBarIconButton SendButton;
        private Autocompleter completer;
        NewTweetModel viewModel;

        public NewTweet()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                if (ApplicationBar != null)
                    ApplicationBar.MatchOverriddenTheme();
            };

            Loaded += NewTweet_Loaded;
            TweetBox.TextChanged += OnTextBoxTextChanged;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Update the binding source
            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();

            viewModel.TextboxSelectionStart = textBox.SelectionStart;
        }

        void NewTweet_Loaded(object sender, RoutedEventArgs e)
        {
            string RemoveBack;
            if (NavigationContext.QueryString.TryGetValue("removeBack", out RemoveBack) || RemoveBack == "1")
            {
                NavigationService.RemoveBackEntry();
            }

            viewModel = DataContext as NewTweetModel;
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
            if (viewModel.SelectedAccounts.Contains(usr))
                UpdateOpacity(img);
        }

        private void aboutScheduling_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.AboutSchedulingMessage));
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

            viewModel.Completer.UserChoseElement(selected);

            UserSuggestions.SelectedItem = null;
        }

    }
}