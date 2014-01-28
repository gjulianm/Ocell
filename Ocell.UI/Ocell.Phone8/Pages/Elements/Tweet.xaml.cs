
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using DanielVaughan;
using DanielVaughan.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    public partial class Tweet : PhoneApplicationPage
    {
        TweetModel viewModel;
        Storyboard sbShow;
        Storyboard sbHide;
        bool conversationLoaded = false;

        public Tweet()
        {
            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            viewModel = new TweetModel();
            DataContext = viewModel;

            this.Loaded += new RoutedEventHandler(Tweet_Loaded);

            viewModel.TweetSent += (s, e) =>
            {
                conversation.Loader.Source.Insert(0, e.Payload);
                TBNoFocus();
            };



            panorama.SelectionChanged += (sender, e) =>
            {
                var pano = panorama.SelectedItem as PanoramaItem;

                if (pano == null)
                    return;

                string tag = pano.Tag as string;
                if (tag == "conversation" && !conversationLoaded)
                {
                    conversation.Load();
                    conversationLoaded = true;
                }
            };


        }

        void Tweet_Loaded(object sender, RoutedEventArgs e)
        {
            sbShow = this.Resources["sbShow"] as Storyboard;
            sbHide = this.Resources["sbHide"] as Storyboard;

            Initialize();
            viewModel.OnLoad();
            if (ApplicationBar != null)
                ApplicationBar.MatchOverriddenTheme();

            conversation.Loader.PropertyChanged += (s, ea) =>
            {
                if (ea.PropertyName == "IsLoading")
                    Dependency.Resolve<IProgressIndicator>().IsLoading = conversation.Loader.IsLoading;
            };
        }

        void Initialize()
        {
            CreateText(viewModel.Tweet);
            viewModel.Completed = true;

            if (DataTransfer.Status == null)
            {
                NavigationService.GoBack();
                return;
            }

            TwitterResource resource = new TwitterResource
            {
                Data = DataTransfer.Status.Id.ToString(),
                Type = ResourceType.Conversation,
                User = DataTransfer.CurrentAccount
            };

            if (conversation.Loader == null)
            {
                conversation.Loader = new TweetLoader(resource);
            }

            if (conversation.Loader.Resource != resource)
            {
                conversation.Loader.Source.Clear();
                conversation.Resource = resource;
            }

            conversation.Loader.Cached = false;
            conversation.AutoManageNavigation = true;
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

            if (viewModel.Tweet.Entities != null)
            {
                foreach (var Entity in viewModel.Tweet.Entities)
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
                    Dependency.Resolve<IMessageService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
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
                Dependency.Resolve<IMessageService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
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
                    Dependency.Resolve<IMessageService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
                }
                else
                    Dependency.Resolve<IMessageService>().ShowError(Localization.Resources.NotValidURL);
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
                    Dependency.Resolve<IMessageService>().ShowMessage(String.Format(Localization.Resources.MutedUntil, filter.IsValidUntil.ToString("f")), "");
                }
                else
                    Dependency.Resolve<IMessageService>().ShowError(Localization.Resources.NotValidURL);
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

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (viewModel != null && viewModel.Tweet != null)
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + viewModel.Tweet.AuthorName, UriKind.Relative));
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (viewModel.Tweet != null && viewModel.Tweet.Author != null)
            {
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + viewModel.Tweet.Author.ScreenName, UriKind.Relative));
            }
        }

        private void Replies_Tap(object sender, EventArgs e)
        {
            NavigationService.Navigate(Uris.Conversation);
        }

        private ITweetableFilter CreateNewFilter(FilterType type, string data)
        {
            if (Config.GlobalFilter == null)
                Config.GlobalFilter = new ColumnFilter();

            if (Config.DefaultMuteTime == null)
                Config.DefaultMuteTime = TimeSpan.FromHours(8);

            ITweetableFilter filter = new ITweetableFilter();
            filter.Type = type;
            filter.Filter = data;
            if (Config.DefaultMuteTime == TimeSpan.MaxValue)
                filter.IsValidUntil = DateTime.MaxValue;
            else
                filter.IsValidUntil = DateTime.Now + (TimeSpan)Config.DefaultMuteTime;
            filter.Inclusion = IncludeOrExclude.Exclude;

            Config.GlobalFilter.AddFilter(filter);
            Config.GlobalFilter = Config.GlobalFilter;

            return filter;
        }

        private void MuteUser_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var filter = FilterManager.SetupMute(FilterType.User, viewModel.Tweet.Author.ScreenName);
            Dependency.Resolve<IMessageService>().
                ShowMessage(String.Format(Localization.Resources.UserIsMutedUntil, viewModel.Tweet.Author.ScreenName, filter.IsValidUntil.ToString("f")), "");
            viewModel.IsMuting = false;
        }

        private void MuteHashtags_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ITweetableFilter filter = null;
            string message = "";
            foreach (var entity in viewModel.Tweet.Entities)
            {
                if (entity.EntityType == TwitterEntityType.HashTag)
                {
                    filter = FilterManager.SetupMute(FilterType.Text, ((TwitterHashTag)entity).Text);
                    message += ((TwitterHashTag)entity).Text + ", ";
                }
            }
            if (message == "")
                Dependency.Resolve<IMessageService>().ShowMessage(Localization.Resources.NoHashtagsToMute);
            else
                Dependency.Resolve<IMessageService>().
                ShowMessage(String.Format(Localization.Resources.HashtagsMutedUntil, message.Substring(0, message.Length - 2), filter.IsValidUntil.ToString("f")), "");
            viewModel.IsMuting = false;
        }

        private void Source_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RemoveHTML conv = new RemoveHTML();
            string source = conv.Convert(viewModel.Tweet.Source, null, null, null) as string;
            var filter = FilterManager.SetupMute(FilterType.Source, source);
            Dependency.Resolve<IMessageService>().ShowMessage(String.Format(Localization.Resources.SourceMutedUntil, source, filter.IsValidUntil.ToString("f")), "");
            viewModel.IsMuting = false;
        }

        void HideMuteGrid(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            viewModel.IsMuting = false;
            this.BackKeyPress -= HideMuteGrid;
        }

        private void MuteBtn_Click(object sender, EventArgs e)
        {
            this.BackKeyPress += HideMuteGrid;
            viewModel.IsMuting = true;
        }

        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            viewModel.ImageFailed(sender, e);
        }

        private void ImageOpened(object sender, RoutedEventArgs e)
        {
            viewModel.ImageOpened(sender, e);
        }

        private void ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            viewModel.ImageTapped(sender, e);
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TBFocus();
        }

        public void TBFocus()
        {
            sbShow.Begin();
            viewModel.ReplyBoxGotFocus();
            textBox.SelectionStart = textBox.Text.Length;
        }

        public void TBNoFocus()
        {
            Dispatcher.BeginInvoke(() =>
            {
                sbHide.Begin();
            });
        }
        bool suppressTBFocusLost = false;
        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((c) =>
            {
                // LostFocus gets triggered before the button click, so wait a little bit and let it be triggered.
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() =>
                {
                    if (!suppressTBFocusLost)
                        TBNoFocus();
                    else
                        suppressTBFocusLost = false;
                });
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            suppressTBFocusLost = true;
        }
    }
}