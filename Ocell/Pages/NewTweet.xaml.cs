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

namespace Ocell.SPpages
{
    public partial class NewTweet : PhoneApplicationPage
    {
        protected bool SendingDM;
        public ApplicationBarIconButton SendButton;
        private IEnumerable<TwitterUser> _users;
        private bool _isWritingUser;
        private string _tweetText;
        private TwitterService _service;

        public NewTweet()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            Loaded += NewTweet_Loaded;
            Unloaded += NewTweet_Unloaded;

            AccountsList.DataContext = Config.Accounts;
            AccountsList.ItemsSource = Config.Accounts;

            SendingDM = DataTransfer.ReplyingDM;

            _isWritingUser = false;

            if (DataTransfer.DMDestinationId == 0 && DataTransfer.DM != null)
                DataTransfer.DMDestinationId = DataTransfer.DM.SenderId;

            InitalizeAppBar();
        }

        private void InitalizeAppBar()
        {
            // Crappy Application bar design is crap. I have to initialise the buttons here in order to access them later.
            ApplicationBar appBar = new ApplicationBar();

            SendButton = new ApplicationBarIconButton(new Uri("/Images/Icons_White/appbar.check.rest.png", UriKind.Relative));
            SendButton.Click += new EventHandler(SendButton_Click);
            SendButton.Text = "send";
            appBar.Buttons.Add(SendButton);

            ApplicationBarIconButton addPhotoButton = new ApplicationBarIconButton(new Uri("/Images/Icons_White/appbar.feature.camera.rest.png", UriKind.Relative));
            addPhotoButton.Click += new EventHandler(AddPhotoButton_Click);
            addPhotoButton.Text = "add photo";
            appBar.Buttons.Add(addPhotoButton);

            ApplicationBar = appBar;
        }

        void NewTweet_Unloaded(object sender, RoutedEventArgs e)
        {
            DataTransfer.Text = Tweet.Text;
        }

        void NewTweet_Loaded(object sender, RoutedEventArgs e)
        {
            if (SendingDM)
            {
                AccountsList.Visibility = Visibility.Collapsed;
                TextBlockAccounts.Visibility = Visibility.Collapsed;
            }

            string RemoveBack;
            if (NavigationContext.QueryString.TryGetValue("removeBack", out RemoveBack) || RemoveBack == "1")
            {
                NavigationService.RemoveBackEntry();
            }

            Tweet.Text = DataTransfer.Text == null ? "" : DataTransfer.Text;
            Tweet.SelectionStart = (Tweet.Text.Length - 1) < 0 ? 0 : (Tweet.Text.Length - 1);
            Tweet.Focus();

            int Index = Config.Accounts.IndexOf(DataTransfer.CurrentAccount);
            AccountsList.SelectedIndex = Index;

            

            if (DataTransfer.CurrentAccount == null)
                _service = ServiceDispatcher.GetDefaultService();
            else
                _service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);

            _service.ListFriends(-1, ReceiveFriends);
        }

        private void ReceiveFriends(TwitterCursorList<TwitterUser> list, TwitterResponse Response)
        {
            if (list != null && Response.StatusCode == HttpStatusCode.OK)
            {
                if (_users == null)
                    _users = list;
                else
                    _users = _users.Concat(list);

                if (list.NextCursor != 0 && list.NextCursor != null)
                    _service.ListFriends((long)list.NextCursor, ReceiveFriends);
            }
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

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                TryBackgroundSend();
                return;
            }

            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);

            if (DataTransfer.ReplyingDM)
            {
                TwitterService Service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
                Dispatcher.BeginInvoke(() =>
                {
                    pBar.Text = "Sending message...";
                });
                Service.SendDirectMessage((int)DataTransfer.DMDestinationId, Tweet.Text, ReceiveDM);
            }
            else
                SendTweet();
        }

        private void TryBackgroundSend()
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBoxResult Result;
                Result = MessageBox.Show("Seems that you're not connected to the Internet. Do you want to automatically send this tweet later?", "", MessageBoxButton.OKCancel);
                if (Result == MessageBoxResult.OK)
                {
                    CreateTweetTask();
                }
            });
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
                	Tweet.Text = "";
                    DataTransfer.Text = "";
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    else
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
        }

        private void CreateTweetTask()
        {
            ITweetableTask Task;

            if (SendingDM)
            {
                Task = new TwitterDMTask
                {
                    DestinationId = DataTransfer.DMDestinationId,
                    Text = Tweet.Text
                };
            }
            else
            {
                Task = new TwitterStatusTask
                {
                    InReplyTo = DataTransfer.ReplyId,
                    Text = Tweet.Text
                };
            }

            Task.Accounts = AccountsList.SelectedItems.Cast<UserToken>();
            Config.TweetTasks.Add(Task);
            Config.SaveTasks();
        }

        private void SendTweet()
        {
            TwitterService srv;
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
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
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

            TwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
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
            CheckAutocompleteUser();
            Dispatcher.BeginInvoke(() =>
            {
                SendButton.IsEnabled = (Tweet.Text.Length <= 140);
                Count.Text = (140 - Tweet.Text.Length).ToString();
            });
        }

        private void CheckAutocompleteUser()
        {
            if (Tweet.Text.Length > 0 && Tweet.Text.Last() == '@')
                _isWritingUser = true;

            if (_isWritingUser)
                UpdateAutocomplete();
        }

        private void UpdateAutocomplete()
        {
            if (_tweetText == Tweet.Text)
                return;

            _tweetText = Tweet.Text;

            if (string.IsNullOrWhiteSpace(_tweetText))
                return;

            if (_tweetText.IndexOf('@') == -1)
            {
                _isWritingUser = false;
                return;
            }

            if (_tweetText.LastIndexOf(' ') > _tweetText.LastIndexOf('@'))
            {
                _isWritingUser = false;
                if (Tweet.SelectionStart < _tweetText.Length)
                {
                    _tweetText = _tweetText.Remove(Tweet.SelectionStart);
                    Tweet.Text = _tweetText;
                }
                return;
            }
            if (Tweet.SelectionStart < _tweetText.Length)
            {
                _tweetText = _tweetText.Remove(Tweet.SelectionStart);
            }

            string Written = _tweetText.Substring(Tweet.Text.LastIndexOf('@') + 1);
            string first;
            if (_users == null)
                first = "";
            else
        	{
        		TwitterUser FirstUser = _users.FirstOrDefault(item => item != null && item.ScreenName.IndexOf(Written) == 0);
                if (FirstUser != null)
                    first = FirstUser.ScreenName;
                else
                    first = ""; 
        	}

            if (Written.Length >= first.Length)
            {
                Tweet.Text = _tweetText;
                Tweet.SelectionStart = _tweetText.Length;
                return;
            }
        	
        	int selectStart;
            selectStart = _tweetText.Length - 1;
        	first = first.Substring(Math.Min(Written.Length , first.Length - 1));
            _tweetText = _tweetText.Insert(Tweet.SelectionStart, first);
            Tweet.Text = _tweetText;
            Tweet.SelectionStart = Math.Min(selectStart + 1, _tweetText.Length - 1);
        	Tweet.SelectionLength = 0;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
                return;

            UpdateOpacity(img);
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

            if ((img.Tag as UserToken) == DataTransfer.CurrentAccount)
                UpdateOpacity(img);
        }


    }
}