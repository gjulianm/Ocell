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

        public NewTweet()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(NewTweet_Loaded);
            this.Unloaded += new RoutedEventHandler(NewTweet_Unloaded);

            AccountsList.DataContext = Config.Accounts;
            AccountsList.ItemsSource = Config.Accounts;

            SendingDM = DataTransfer.ReplyingDM;

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

            ApplicationBarIconButton AddPhotoButton = new ApplicationBarIconButton(new Uri("/Images/Icons_White/appbar.feature.camera.rest.png", UriKind.Relative));
            AddPhotoButton.Click += new EventHandler(AddPhotoButton_Click);
            AddPhotoButton.Text = "add photo";
            appBar.Buttons.Add(AddPhotoButton);

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
        }

        private void SendButton_Click(object sender, EventArgs e)
        {

            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                TryBackgroundSend();
                return;
            }

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
                    DestinationID = DataTransfer.DMDestinationId,
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
            pBar.IsVisible = false;
            if (response.StatusCode == HttpStatusCode.Forbidden)
                Dispatcher.BeginInvoke(() => MessageBox.Show("That tweet is duplicated."));
            else if (response.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => MessageBox.Show("Ooops. An error has ocurred, try again."));
            else
                Dispatcher.BeginInvoke(() =>
                {
                    pBar.IsVisible = false;
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