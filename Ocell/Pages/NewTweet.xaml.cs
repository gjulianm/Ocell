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

namespace Ocell.Pages
{
    public partial class NewTweet : PhoneApplicationPage
    {
        protected bool SendingDM;
        public ApplicationBarIconButton SendButton;
        private UsernameProvider _provider;
        private Autocompleter _completer;
        private bool _isScheduled;
        private TwitterDraft draft;

        public NewTweet()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            Loaded += NewTweet_Loaded;
            Unloaded += NewTweet_Unloaded;

            AccountsList.DataContext = Config.Accounts;
            AccountsList.ItemsSource = Config.Accounts;

            SendingDM = DataTransfer.ReplyingDM;

            if (DataTransfer.DMDestinationId == 0 && DataTransfer.DM != null)
                DataTransfer.DMDestinationId = DataTransfer.DM.SenderId;

            _isScheduled = false;

            InitalizeAppBar();
        }

        private void InitalizeAppBar()
        {
            // Crappy Application bar design is crap. I have to initialise the buttons here in order to access them later.
            IApplicationBar appBar = ApplicationBar;

            SendButton = new ApplicationBarIconButton(new Uri("/Images/Icons_White/appbar.sendmail.rest.png", UriKind.Relative));
            SendButton.Click += new EventHandler(SendButton_Click);
            SendButton.Text = "send";
            appBar.Buttons.Add(SendButton);

            ApplicationBarIconButton saveButton = new ApplicationBarIconButton();
            saveButton.IconUri = new Uri("/Images/Icons_White/appbar.save.rest.png", UriKind.Relative);
            saveButton.Text = "save as draft";
            saveButton.Click += new EventHandler(saveButton_Click);
            appBar.Buttons.Add(saveButton);

            ApplicationBarIconButton addPhotoButton = new ApplicationBarIconButton(new Uri("/Images/Icons_White/appbar.feature.camera.rest.png", UriKind.Relative));
            addPhotoButton.Click += new EventHandler(AddPhotoButton_Click);
            addPhotoButton.Text = "add photo";
            appBar.Buttons.Add(addPhotoButton);

            ApplicationBar = appBar;
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            SaveDraft();
        }

        private TwitterDraft SaveDraft()
        {
            TwitterDraft draft = new TwitterDraft();
            draft.Text = Tweet.Text;

            if (Scheduled_cb.IsChecked == true)
            {
                draft.Scheduled = new DateTime(
                datePicker.Value.Value.Year,
                datePicker.Value.Value.Month,
                datePicker.Value.Value.Day,
                timePicker.Value.Value.Hour,
                timePicker.Value.Value.Minute,
                0);
            }
            else
                draft.Scheduled = null;

            draft.CreatedAt = DateTime.Now;
            draft.Accounts = new List<UserToken>();
            foreach (var acc in AccountsList.SelectedItems)
            {
                if (acc is UserToken)
                    draft.Accounts.Add(acc as UserToken);
            }
            draft.ReplyId = DataTransfer.ReplyId;

            Config.Drafts.Add(draft);
            Config.Drafts = Config.Drafts;

            Dispatcher.BeginInvoke(() => MessageBox.Show("Draft saved!"));

            return draft;
        }

        void NewTweet_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_provider != null)
            {
                _provider.Stop();
                _provider.Usernames.Clear();
            }
            DataTransfer.Text = Tweet.Text;

            if (DataTransfer.Draft != null)
                DataTransfer.Draft = SaveDraft();
        }

        void NewTweet_Loaded(object sender, RoutedEventArgs e)
        {
            if (SendingDM)
            {
                Scheduled_cb.Visibility = Visibility.Collapsed;
                AccountsList.Visibility = Visibility.Collapsed;
                TextBlockAccounts.Visibility = Visibility.Collapsed;
            }

            string RemoveBack;
            if (NavigationContext.QueryString.TryGetValue("removeBack", out RemoveBack) || RemoveBack == "1")
            {
                NavigationService.RemoveBackEntry();
            }

            draft = DataTransfer.Draft;

            if (draft != null)
            {
                Tweet.Text = draft.Text;
                Tweet.SelectionStart = (Tweet.Text.Length - 1) < 0 ? 0 : (Tweet.Text.Length);
                Tweet.Focus();

                foreach (var account in draft.Accounts)
                    AccountsList.SelectedItems.Add(account);

                if (draft.Scheduled != null)
                {
                    Scheduled_cb.IsChecked = true;
                    timePicker.Value = draft.Scheduled;
                    datePicker.Value = draft.Scheduled;
                }
            }
            else
            {
                Tweet.Text = DataTransfer.Text == null ? "" : DataTransfer.Text;
                Tweet.SelectionStart = (Tweet.Text.Length - 1) < 0 ? 0 : (Tweet.Text.Length);
                Tweet.Focus();

                int Index = Config.Accounts.IndexOf(DataTransfer.CurrentAccount);
                AccountsList.SelectedIndex = Index;
            }

            _provider = new UsernameProvider();
            _provider.User = DataTransfer.CurrentAccount;
            _completer = new Autocompleter();
            _completer.Textbox = Tweet;
            _completer.Trigger = '@';
            _completer.Strings = _provider.Usernames;
            _provider.Start();
        }

        private bool CheckProtectedAccounts()
        {
            UserToken User;
            foreach (var item in AccountsList.SelectedItems)
            {
                User = item as UserToken;
                if (User != null && ProtectedAccounts.IsProtected(User))
                {
                        MessageBoxResult Result;
                        Result = MessageBox.Show("Are you sure you want to tweet from the protected account @" + User.ScreenName + "?", "", MessageBoxButton.OKCancel);
                        if (Result != MessageBoxResult.OK)
                            return false;
                }
            }

            return true;
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (!CheckProtectedAccounts())
                return;

            if (_isScheduled)
            {
                ScheduleTweet();
            }
            else
            {

                Dispatcher.BeginInvoke(() => pBar.IsVisible = true);

                if (DataTransfer.ReplyingDM)
                {
                    ITwitterService Service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
                    Dispatcher.BeginInvoke(() =>
                    {
                        pBar.Text = "Sending message...";
                    });
                    Service.SendDirectMessage((int)DataTransfer.DMDestinationId, Tweet.Text, ReceiveDM);
                    
                }
                else
                    SendTweet();
            }
        }

        private void ScheduleTweet()
        {
            TwitterStatusTask task = new TwitterStatusTask
                {
                    InReplyTo = DataTransfer.ReplyId
                };

            task.Text = Tweet.Text;

            if (datePicker.Value == null || timePicker.Value == null)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Please select a date and time to schedule the tweet."));
                return;
            }

            task.Scheduled = new DateTime(
                datePicker.Value.Value.Year,
                datePicker.Value.Value.Month,
                datePicker.Value.Value.Day,
                timePicker.Value.Value.Hour,
                timePicker.Value.Value.Minute,
                0);

            task.Accounts = new List<UserToken>();

            if (AccountsList.SelectedItems.Count == 0)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("You have to select an account to schedule the tweet."));
                return;
            }

            foreach (var user in AccountsList.SelectedItems)
            {
                if(user is UserToken)
                    task.Accounts.Add((UserToken)user);
            }

            Config.TweetTasks.Add(task);
            Config.SaveTasks();
            Dispatcher.BeginInvoke(() => MessageBox.Show("Message scheduled!"));
            NavigationService.GoBack();
        }

        private void ReceiveDM(TwitterDirectMessage DM, TwitterResponse response)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
            if (response.StatusCode == HttpStatusCode.Forbidden)
                Dispatcher.BeginInvoke(() => MessageBox.Show("That tweet is duplicated."));
            else if (response.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => MessageBox.Show("An error has occurred."));
            else
                Dispatcher.BeginInvoke(() =>
                {
                    DataTransfer.ReplyingDM = false;
                	Tweet.Text = "";
                    DataTransfer.Text = "";
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    else
                        NavigationService.Navigate(Uris.MainPage);
                });
        }

        private void SendTweet()
        {
            ITwitterService srv;
            Dispatcher.BeginInvoke(() =>
            {
                pBar.Text = "Sending tweet...";
            });
            if (AccountsList.SelectedItems.Count == 0)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("You haven't select any account to tweet."));
                return;
            }
            foreach (UserToken Account in AccountsList.SelectedItems.Cast<UserToken>())
            {
                srv = ServiceDispatcher.GetService(Account);
                srv.SendTweet(Tweet.Text, DataTransfer.ReplyId, ReceiveResponse);
            }

            if (DataTransfer.Draft != null)
            {
                if (Config.Drafts.Contains(DataTransfer.Draft))
                    Config.Drafts.Remove(DataTransfer.Draft);

                DataTransfer.Draft = null;
                draft = null;
                Config.SaveDrafts();
            }
        }

        private void ReceiveResponse(TwitterStatus status, TwitterResponse response)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
            if (response.StatusCode == HttpStatusCode.Forbidden)
                Dispatcher.BeginInvoke(() => MessageBox.Show("That tweet is duplicated."));
            else if (response.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => MessageBox.Show("Ooops. An error has ocurred, try again."));
            else
                Dispatcher.BeginInvoke(() =>
                {
                	Tweet.Text = "";
                    DataTransfer.Text = "";
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    else
                        NavigationService.Navigate(Uris.MainPage);
                });
        }

        private void AddPhotoButton_Click(object sender, EventArgs e)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.ShowCamera = true;
            chooser.Completed += new EventHandler<PhotoResult>(chooser_Completed);
            chooser.Show();
        }

        void chooser_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;

            Dispatcher.BeginInvoke(() =>
            {
                pBar.IsVisible = true;
                pBar.Text = "uploading photo";
                SendButton.IsEnabled = false;
            });

            ITwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            RestRequest req = srv.PrepareEchoRequest();
            RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "1" };

            req.AddFile("media", e.OriginalFileName, e.ChosenPhoto);
            req.AddField("key", "1abb1622666934158f4c2047f0822d0a");
            req.AddField("message", Tweet.Text);
            req.AddField("consumer_token", SensitiveData.ConsumerToken);
            req.AddField("consumer_secret", SensitiveData.ConsumerSecret);
            req.AddField("oauth_token", DataTransfer.CurrentAccount.Key);
            req.AddField("oauth_secret", DataTransfer.CurrentAccount.Secret);
            req.Path = "upload.xml";
            //req.Method = Hammock.Web.WebMethod.Post;

            client.BeginRequest(req, (RestCallback)uploadCompleted);
        }

        private void uploadCompleted(RestRequest request, RestResponse response, object userstate)
        {
            Dispatcher.BeginInvoke(() =>
            {
                pBar.IsVisible = false;
                pBar.Text = "";
                SendButton.IsEnabled = true;
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Error uploading picture. Try again."));
                return;
            }

            XDocument doc = XDocument.Parse(response.Content);
            XElement node = doc.Descendants("url").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(node.Value) || !node.Value.Contains("http://"))
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Error uploading picture. Try again"));
                return;
            }

            Dispatcher.BeginInvoke(() => { Tweet.Text += " " + node.Value + " "; });
        }

        private void Tweet_TextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                SendButton.IsEnabled = (Tweet.Text.Length <= 140);
                Count.Text = (140 - Tweet.Text.Length).ToString();
            });
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
                return;

            UpdateOpacity(img);

            if (img.Opacity == 1)
            {
                _provider.User = img.Tag as UserToken;
                _provider.Start();
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
            if (usr == DataTransfer.CurrentAccount || (draft != null && draft.Accounts.Contains(usr)))
                UpdateOpacity(img);
        }

        private void Scheduled_cb_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Scheduled_cb.IsChecked == true)
            {
                ScheduleGrid.Visibility = Visibility.Visible;
                _isScheduled = true;
                SendButton.IconUri = new Uri("/Images/Icons_White/appbar.alarm.rest.png", UriKind.Relative);
                SendButton.Text = "schedule";
            }
            else
            {
                ScheduleGrid.Visibility = System.Windows.Visibility.Collapsed;
                _isScheduled = false;
                SendButton.IconUri = new Uri("/Images/Icons_White/appbar.sendmail.rest.png", UriKind.Relative);
                SendButton.Text = "send";
            }
        }

        private void aboutScheduling_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        	Dispatcher.BeginInvoke(() => MessageBox.Show("Because of the system's limitations, we can only check your scheduled tweets every half hour, " +
			", so it's possible that your tweet will not be sent at the exact time you specified. Also, remember that if you have your phone in battery saving mode or turned off, we won't be able to send your tweet."));
        }

        private void draft_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.ManageDrafts);
        }
    }
}