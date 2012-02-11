using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using TweetSharp;


namespace Ocell.SPpages
{
    public partial class Tweet : PhoneApplicationPage
    {
        public TweetSharp.TwitterStatus status;

        public Tweet()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Tweet_Loaded); 
        }

        void Tweet_Loaded(object sender, RoutedEventArgs e)
        {
            RemoveHTML conv = new RemoveHTML();
            if (DataTransfer.Status == null)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Error loading the tweet. Sorry :("));
                NavigationService.GoBack();
                return;
            }

            status = DataTransfer.Status;

            RelativeDateTimeConverter dc = new RelativeDateTimeConverter();

            var paragraph = new Paragraph();
            var runs = new List<Inline>();

            Text.Blocks.Clear();

            foreach (var word in status.Text.Split(' '))
            {
                Uri uri;

                if (string.IsNullOrWhiteSpace(word))
                    continue;

                if (Uri.TryCreate(word, UriKind.Absolute, out uri) ||
                   (word.StartsWith("www.") && Uri.TryCreate("http://" + word, UriKind.Absolute, out uri)) ||
                    word[0]=='#' || word[0] == '@')
                {
                    var link = new Hyperlink();
                    link.Inlines.Add(new Run() { Text = word });  
                    link.FontWeight = FontWeights.Bold;
                    link.TextDecorations = null;
                    link.TargetName = word;
                    link.Click += new RoutedEventHandler(link_Click);

                    runs.Add(link);
                }
                else
                {
                    runs.Add(new Run() { Text = word });
                }

                runs.Add(new Run() { Text = " " });
            }

            foreach (var run in runs)
                paragraph.Inlines.Add(run);
            
            Text.Blocks.Add(paragraph);

            Text.UpdateLayout();

            ContentPanel.DataContext = status;

            ViaDate.Margin = new Thickness(ViaDate.Margin.Left, Text.ActualHeight + Text.Margin.Top + 10,
                ViaDate.Margin.Right, ViaDate.Margin.Bottom);
            ViaDate.Text = (string)dc.Convert(status.CreatedDate, null, null, null) + " via " +
                conv.Convert(status.Source, null, null, null);


            Replies.Margin = new Thickness(Replies.Margin.Left, ViaDate.Margin.Top + 30,
                Replies.Margin.Right, Replies.Margin.Bottom);

            if (status.InReplyToStatusId != null)
                Replies.Visibility = Visibility.Visible;

            SName.Text = "@" + status.Author.ScreenName;
            ContentPanel.UpdateLayout();
        }

        void link_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Uri uri;
            WebBrowserTask browser;

            if (link == null || string.IsNullOrWhiteSpace(link.TargetName))
                return;

            if (Uri.TryCreate(link.TargetName, UriKind.Absolute, out uri) ||
                (link.TargetName.StartsWith("www.") && Uri.TryCreate("http://" + link.TargetName, UriKind.Absolute, out uri)))
            {
                browser = new WebBrowserTask();
                browser.Uri = uri;
                browser.Show();
            }
            else if (link.TargetName[0] == '@')
                NavigationService.Navigate(new Uri("/Pages/User.xaml?user=" + link.TargetName.Substring(0), UriKind.Relative));
            else if (link.TargetName[0] == '#')
            {
                DataTransfer.Search = link.TargetName;
                NavigationService.Navigate(new Uri("/Pages/Search.xaml?q=" + link.TargetName, UriKind.Relative));
            }
              
        }

        private void replyButton_Click(object sender, EventArgs e)
        {
            DataTransfer.ReplyId = status.Id;
            DataTransfer.Text = "@" + status.Author.ScreenName + " ";
            DataTransfer.ReplyingDM = false;
            NavigationService.Navigate(new Uri("/Pages/NewTweet.xaml", UriKind.Relative));
        }

        private void replyAllButton_Click(object sender, EventArgs e)
        {
            DataTransfer.ReplyId = status.Id;
            DataTransfer.Text = "@" + status.Author.ScreenName + " ";
            DataTransfer.ReplyingDM = false;

            foreach (string user in StringManipulator.GetUserNames(status.Text))
                DataTransfer.Text += "@" + user + " ";

            NavigationService.Navigate(new Uri("/Pages/NewTweet.xaml", UriKind.Relative));
        }

        private void retweetButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            ServiceDispatcher.GetCurrentService().Retweet(status.Id, (Action<TwitterStatus, TwitterResponse>)receive);
        }

        private void favButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            ServiceDispatcher.GetCurrentService().FavoriteTweet(status.Id, (Action<TwitterStatus, TwitterResponse>)receive);
        }

        private void receive(TwitterStatus status, TwitterResponse resp)
        {
            if (resp.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => { MessageBox.Show("An error has occurred :("); });
            Dispatcher.BeginInvoke(() => { pBar.IsVisible = false; });
        }

        private void quoteButton_Click(object sender, EventArgs e)
        {
            DataTransfer.ReplyId = 0;
            DataTransfer.Text = "\"@" + status.Text + "\"";

            NavigationService.Navigate(new Uri("/Pages/NewTweet.xaml", UriKind.Relative));
        }

        private void shareButton_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Tweet from @" + status.Author.ScreenName;
            emailComposeTask.Body = "@" + status.Author.ScreenName + ": " + status.Text + Environment.NewLine + Environment.NewLine +
                status.CreatedDate.ToString();

            emailComposeTask.Show();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/User.xaml?user=" + status.Author.ScreenName, UriKind.Relative));
        }
        
        private void Replies_Tap(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Conversation.xaml", UriKind.Relative));
        }
    }
}