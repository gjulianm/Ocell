using AncoraMVVM.Base;
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using Ocell.Pages.Elements;
using Ocell.Pages.Search;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TweetSharp;

namespace Ocell.Helpers
{
    public class TweetRTBFormatter
    {
        private RichTextBox Textbox;
        private ITweetable Tweet;

        private Dictionary<TwitterEntityType, Func<TwitterEntity, Inline>> linkCreators;

        public TweetRTBFormatter(ITweetable tweet, RichTextBox textbox)
        {
            if (tweet == null)
                throw new ArgumentNullException("tweet can't be null");

            if (textbox == null)
                throw new ArgumentNullException("textbox can't be null");

            this.Tweet = tweet;
            this.Textbox = textbox;

            linkCreators = new Dictionary<TwitterEntityType, Func<TwitterEntity, Inline>>
            {
                { TwitterEntityType.HashTag , CreateHashtagLink},
                { TwitterEntityType.Media , CreateMediaLink},
                { TwitterEntityType.Mention , CreateMentionLink},
                { TwitterEntityType.Url , CreateUrlLink}
            };
        }

        public void Format()
        {
            var paragraph = new Paragraph();
            var runs = new List<Inline>();

            Textbox.Blocks.Clear();

            string TweetText = Tweet.Text;
            string PreviousText;
            int i = 0;

            if (Tweet.Entities != null)
            {
                foreach (var Entity in Tweet.Entities)
                {
                    if (Entity.StartIndex > i)
                    {
                        PreviousText = TweetText.Substring(i, Entity.StartIndex - i);
                        runs.Add(new Run { Text = HttpUtility.HtmlDecode(PreviousText) });
                    }

                    i = Entity.EndIndex;

                    Func<TwitterEntity, Inline> creator;

                    if (linkCreators.TryGetValue(Entity.EntityType, out creator))
                        runs.Add(creator(Entity));
                }
            }

            if (i < TweetText.Length)
                runs.Add(new Run { Text = HttpUtility.HtmlDecode(TweetText.Substring(i)) });

            paragraph.Inlines.AddListRange(runs);
            Textbox.Blocks.Add(paragraph);
            Textbox.UpdateLayout();
        }

        Inline CreateBaseLink(string content, string copyContentHeader, string contentToClipboard, string menuItemHeader, RoutedEventHandler menuItemClick)
        {
            var link = new HyperlinkButton
            {
                Content = content,
                FontSize = Textbox.FontSize,
                FontWeight = Textbox.FontWeight,
                FontStretch = Textbox.FontStretch,
                FontFamily = Textbox.FontFamily,
                TargetName = contentToClipboard,
                Margin = new Thickness(-10, -5, -10, -8)
            };

            link.Click += new RoutedEventHandler(OnClickLink);

            MenuItem copyLinkItem = new MenuItem
            {
                Header = copyContentHeader,
                Tag = contentToClipboard,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            copyLinkItem.Click += CopyLink;

            MenuItem menuItem = new MenuItem
            {
                Header = menuItemHeader,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            menuItem.Click += menuItemClick;

            ContextMenu menu = new ContextMenu();

            menu.Items.Add(copyLinkItem);
            menu.Items.Add(menuItem);

            ContextMenuService.SetContextMenu(link, menu);

            InlineUIContainer container = new InlineUIContainer();
            container.Child = link;
            return container;
        }

        void CopyLink(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item == null)
                return;

            var copyToClipboard = item.Tag as string;

            if (!string.IsNullOrWhiteSpace(copyToClipboard))
                Clipboard.SetText(item.Tag as string);
        }


        void OnClickLink(object sender, RoutedEventArgs e)
        {
            HyperlinkButton link = sender as HyperlinkButton;
            Uri uri;
            WebBrowserTask browser;
            INavigationService navigator = Dependency.Resolve<INavigationService>();
            IMessager messager = Dependency.Resolve<IMessager>();

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
            {
                messager.SendTo<UserModel, string>(link.TargetName);
                navigator.Navigate<UserModel>();
            }
            else if (link.TargetName[0] == '#')
            {
                var resourceToView = new TwitterResource
                {
                    Type = ResourceType.Search,
                    Data = link.TargetName
                };

                messager.SendTo<ResourceViewModel, TwitterResource>(resourceToView);
                navigator.Navigate<ResourceViewModel>();
            }
        }

        Inline CreateHashtagLink(TwitterEntity entity)
        {
            TwitterHashTag hashtag = entity as TwitterHashTag;

            RoutedEventHandler clickHandler = (sender, e) =>
            {
                var filter = FilterManager.SetupMute(FilterType.Text, "#" + hashtag.Text);
                Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")));
            };

            return CreateBaseLink("#" + hashtag.Text, Localization.Resources.CopyHashtag, "#" + hashtag.Text, Localization.Resources.MuteHashtag, clickHandler);
        }


        Inline CreateMediaLink(TwitterEntity entity)
        {
            TwitterMedia media = entity as TwitterMedia;

            RoutedEventHandler clickHandler = (sender, e) =>
            {
                Uri uri;
                if (Uri.TryCreate(media.DisplayUrl, UriKind.Absolute, out uri))
                {
                    var filter = FilterManager.SetupMute(FilterType.Text, uri.Host);
                    Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")));
                }
                else
                    Dependency.Resolve<INotificationService>().ShowError(Localization.Resources.NotValidURL);
            };

            return CreateBaseLink(media.DisplayUrl, Localization.Resources.CopyLink, media.DisplayUrl, Localization.Resources.MuteDomain, clickHandler);
        }

        Inline CreateUrlLink(TwitterEntity entity)
        {
            TwitterUrl url = entity as TwitterUrl;
            RoutedEventHandler clickHandler = (sender, e) =>
            {
                Uri uri;
                if (Uri.TryCreate(url.ExpandedValue, UriKind.Absolute, out uri))
                {
                    var filter = FilterManager.SetupMute(FilterType.Text, uri.Host);
                    Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")));
                }
                else
                    Dependency.Resolve<INotificationService>().ShowError(Localization.Resources.NotValidURL);
            };

            string value = string.IsNullOrWhiteSpace(url.ExpandedValue) ? url.Value : url.ExpandedValue;

            return CreateBaseLink(TweetTextConverter.TrimUrl(value), Localization.Resources.CopyLink, value, Localization.Resources.MuteDomain, clickHandler);
        }

        Inline CreateMentionLink(TwitterEntity entity)
        {
            TwitterMention mention = entity as TwitterMention;

            RoutedEventHandler clickHandler = (sender, e) =>
            {
                var filter = FilterManager.SetupMute(FilterType.User, mention.ScreenName);
                Dependency.Resolve<INotificationService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")));
            };

            return CreateBaseLink("@" + mention.ScreenName, Localization.Resources.CopyUsername, "@" + mention.ScreenName, Localization.Resources.MuteUser, clickHandler);
        }
    }
}
