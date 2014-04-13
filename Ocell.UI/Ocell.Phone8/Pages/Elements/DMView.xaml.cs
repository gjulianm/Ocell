using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Ocell.Helpers;
using Ocell.Library;
using System;
using System.Net;
using System.Windows;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    // TODO: WTH is this.
    public partial class DMView : PhoneApplicationPage
    {
        public TweetSharp.TwitterDirectMessage status;

        public DMView()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };


            this.Loaded += new RoutedEventHandler(Tweet_Loaded);
        }

        void Tweet_Loaded(object sender, RoutedEventArgs e)
        {
            RemoveHTML conv = new RemoveHTML();

            if (DataTransfer.DM == null)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show(Localization.Resources.ErrorLoadingTweet));
                NavigationService.GoBack();
                return;
            }


            status = DataTransfer.DM;

            RelativeDateTimeConverter dc = new RelativeDateTimeConverter();

            CreateText(status);

            Text.UpdateLayout();

            ContentPanel.DataContext = status;

            ViaDate.Margin = new Thickness(ViaDate.Margin.Left, Text.ActualHeight + Text.Margin.Top + 10,
                ViaDate.Margin.Right, ViaDate.Margin.Bottom);
            ViaDate.Text = (string)dc.Convert(status.CreatedDate, null, null, null);

            SName.Text = "@" + status.Author.ScreenName;
            ContentPanel.UpdateLayout();
        }


        private void CreateText(ITweetable Status)
        {
            if (Status == null)
                return;

            var formatter = new TweetRTBFormatter(Status, Text);
            formatter.Format();
        }

        private void replyButton_Click(object sender, EventArgs e)
        {
            DataTransfer.Text = "";
            DataTransfer.ReplyingDM = true;
            DataTransfer.DMDestinationId = status.SenderId;

            NavigationService.Navigate(Uris.WriteTweet);
        }

        private void receive(TwitterStatus status, TwitterResponse resp)
        {
            if (resp.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => { MessageBox.Show(Localization.Resources.ErrorMessage); });
            Dispatcher.BeginInvoke(() => { pBar.IsVisible = false; });
        }

        private void shareButton_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = String.Format(Localization.Resources.TweetFrom, status.Author.ScreenName);
            emailComposeTask.Body = "@" + status.Author.ScreenName + ": " + status.Text + Environment.NewLine + Environment.NewLine +
                status.CreatedDate.ToString();

            emailComposeTask.Show();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + status.Author.ScreenName, UriKind.Relative));
        }

    }

}