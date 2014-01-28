using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TweetSharp;

namespace Ocell.Pages.Elements
{
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

            var paragraph = new Paragraph();
            var runs = new List<Inline>();

            Text.Blocks.Clear();

            string TweetText = Status.Text;
            string PreviousText;
            int i = 0;

            if (Status.Entities != null)
            {
                foreach (var Entity in Status.Entities)
                {
                    if (Entity.StartIndex > i)
                    {
                        PreviousText = TweetText.Substring(i, Entity.StartIndex - i);
                        runs.Add(new Run { Text = HttpUtility.HtmlDecode(PreviousText) });
                    }

                    i = Entity.EndIndex;

                    switch (Entity.EntityType)
                    {
                        case TwitterEntityType.HashTag:
                            runs.Add(CreateHashtagLink((TwitterHashTag)Entity));
                            break;

                        case TwitterEntityType.Mention:
                            runs.Add(CreateMentionLink((TwitterMention)Entity));
                            break;

                        case TwitterEntityType.Url:
                            runs.Add(CreateUrlLink((TwitterUrl)Entity));
                            break;
                        case TwitterEntityType.Media:
                            runs.Add(CreateMediaLink((TwitterMedia)Entity));
                            break;
                    }
                }
            }

            if (i < TweetText.Length)
                runs.Add(new Run
                {
                    Text = HttpUtility.HtmlDecode(TweetText.Substring(i))
                });

            foreach (var run in runs)
                paragraph.Inlines.Add(run);

            Text.Blocks.Add(paragraph);

            Text.UpdateLayout();
        }

        Inline CreateBaseLink(string content, string contextHeader, string contextTag, MenuItem customButton = null)
        {
            var link = new HyperlinkButton
            {
                Content = content,
                FontSize = Text.FontSize,
                FontWeight = Text.FontWeight,
                FontStretch = Text.FontStretch,
                FontFamily = Text.FontFamily,
                TargetName = contextTag,
                Margin = new Thickness(-10, -5, -10, -8)
            };

            link.Click += new RoutedEventHandler(link_Click);


            MenuItem item = new MenuItem
            {
                Header = contextHeader,
                Tag = contextTag,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            item.Click += new RoutedEventHandler(CopyLink);

            ContextMenu menu = new ContextMenu();
            menu.Items.Add(item);
            if (customButton != null)
                menu.Items.Add(customButton);

            ContextMenuService.SetContextMenu(link, menu);

            InlineUIContainer container = new InlineUIContainer();
            container.Child = link;
            return container;
        }

        Inline CreateHashtagLink(TwitterHashTag Hashtag)
        {
            MenuItem item = new MenuItem
            {
                Header = Localization.Resources.MuteHashtag,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            item.Click += (sender, e) =>
            {
                var filter = FilterManager.SetupMute(FilterType.Text, "#" + Hashtag.Text);
                Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
            };
            return CreateBaseLink("#" + Hashtag.Text, Localization.Resources.CopyHashtag, "#" + Hashtag.Text, item);
        }

        Inline CreateMentionLink(TwitterMention Mention)
        {
            MenuItem item = new MenuItem
            {
                Header = Localization.Resources.MuteUser,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            item.Click += (sender, e) =>
            {
                var filter = FilterManager.SetupMute(FilterType.User, Mention.ScreenName);
                Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
            };
            return CreateBaseLink("@" + Mention.ScreenName, Localization.Resources.CopyUsername, "@" + Mention.ScreenName, item);
        }

        Inline CreateUrlLink(TwitterUrl URL)
        {
            MenuItem item = new MenuItem
            {
                Header = Localization.Resources.MuteDomain,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            item.Click += (sender, e) =>
            {
                Uri uri;
                if (Uri.TryCreate(URL.ExpandedValue, UriKind.Absolute, out uri))
                {
                    var filter = FilterManager.SetupMute(FilterType.Text, uri.Host);
                    Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
                }
                else
                    Dependency.Resolve<INotificationService>().ShowError(Localization.Resources.NotValidURL);
            };

            string value = string.IsNullOrWhiteSpace(URL.ExpandedValue) ? URL.Value : URL.ExpandedValue;

            return CreateBaseLink(TweetTextConverter.TrimUrl(value), Localization.Resources.CopyLink, value, item);
        }

        Inline CreateMediaLink(TwitterMedia Media)
        {
            MenuItem item = new MenuItem
            {
                Header = Localization.Resources.MuteDomain,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            item.Click += (sender, e) =>
            {
                Uri uri;
                if (Uri.TryCreate(Media.DisplayUrl, UriKind.Absolute, out uri))
                {
                    var filter = FilterManager.SetupMute(FilterType.Text, uri.Host);
                    Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
                }
                else
                    Dependency.Resolve<INotificationService>().ShowError(Localization.Resources.NotValidURL);
            };

            return CreateBaseLink(Media.DisplayUrl, Localization.Resources.CopyLink, Media.DisplayUrl, item);
        }

        void CopyLink(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null && item.Tag is string && !(string.IsNullOrWhiteSpace(item.Tag as string)))
                Clipboard.SetText(item.Tag as string);
        }

        void link_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton link = sender as HyperlinkButton;
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
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + link.TargetName.Substring(0), UriKind.Relative));
            else if (link.TargetName[0] == '#')
            {
                Ocell.Pages.Search.ResourceViewModel.Resource = new TwitterResource
                {
                    User = DataTransfer.CurrentAccount,
                    Type = ResourceType.Search,
                    Data = link.TargetName
                };

                NavigationService.Navigate(Uris.ResourceView);
            }

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