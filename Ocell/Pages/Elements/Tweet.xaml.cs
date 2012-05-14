using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;
using Ocell.Library.Filtering;


namespace Ocell.Pages.Elements
{
    public partial class Tweet : PhoneApplicationPage
    {
        public TweetSharp.TwitterStatus status;
        private bool _favBtnState;
        private ApplicationBarIconButton _favButton;
        private ContextMenu _menuOpened;
        private ConversationService _conversation;
        private ApplicationBarMenuItem _removeTweet;

        public Tweet()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
            CreateFavButton();

            this.Loaded += new RoutedEventHandler(Tweet_Loaded);
        }

        void CreateFavButton()
        {
            _favButton = new ApplicationBarIconButton();
            _favButton.IconUri = new Uri("/Images/Icons_White/appbar.favs.addto.rest.png", UriKind.Relative);
            _favButton.Click += new EventHandler(favButton_Click);
            _favButton.Text = "add favorite";
            _favBtnState = true;
            ApplicationBar.Buttons.Add(_favButton);
        }

        void ToggleFavButton()
        {
            if (_favBtnState)
            {
                _favButton.Text = "remove favorite";
                _favButton.IconUri = new Uri("/Images/Icons_White/appbar.favs.remove.rest.png", UriKind.Relative);
                _favBtnState = false;
            }
            else
            {
                _favButton.Text = "add favorite";
                _favButton.IconUri = new Uri("/Images/Icons_White/appbar.favs.addto.rest.png", UriKind.Relative);
                _favBtnState = true;
            }
        }

        void Tweet_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataTransfer.Status == null)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Error loading the tweet. Sorry :("));
                NavigationService.GoBack();
                return;
            }

            _conversation = new ConversationService(DataTransfer.CurrentAccount);

            if (DataTransfer.Status.RetweetedStatus != null)
                status = DataTransfer.Status.RetweetedStatus;
            else
                status = DataTransfer.Status;

            _conversation.CheckIfReplied(status, (replied) =>
            {
                if (replied)
                    Dispatcher.BeginInvoke(() => Replies.Visibility = Visibility.Visible);
            });

            if (status.IsFavorited)
                Dispatcher.BeginInvoke(ToggleFavButton);

            if (status.User == null || status.User.Name == null)
                FillUser();

            SetBindings();
            CreateText(status);
            AdjustMargins();
            SetVisibilityOfRepliesAndImages();
            SetUsername();
            CheckForRetweets();
            SetImage();
            CheckIfCanDelete();

            ContentPanel.UpdateLayout();
        }

        private void CheckIfCanDelete()
        {
            UserToken user = Config.Accounts.FirstOrDefault(item => item != null && item.ScreenName == status.Author.ScreenName);
            if (user != null && _removeTweet == null)
            {
                _removeTweet = new ApplicationBarMenuItem("delete tweet");
                _removeTweet.Click += (sender, e) =>
                {
                    ServiceDispatcher.GetService(user).DeleteTweet(status.Id, (s, response) =>
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                            Dispatcher.BeginInvoke(() => MessageBox.Show("Tweet deleted! (Note that it could take a few minutes to disappear completely from your streams)"));
                        else
                            Dispatcher.BeginInvoke(() => MessageBox.Show("An error has occurred."));
                    });
                };
                ApplicationBar.MenuItems.Add(_removeTweet);
            }
        }

        private void CheckForRetweets()
        {
            TwitterService srv = ServiceDispatcher.GetCurrentService();
            srv.Retweets(status.Id, ReceiveRetweets);
        }

        private void ReceiveRetweets(IEnumerable<TwitterStatus> rts, TwitterResponse response)
        {
            if (rts != null && rts.Any())
            {
                var users = new System.Collections.ObjectModel.ObservableCollection<ITweeter>();
                foreach (var rt in rts)
                    users.Add(rt.Author);

                Dispatcher.BeginInvoke((() =>
                {
                    RTList.ItemsSource = users;
                    RTList.Visibility = System.Windows.Visibility.Visible;
                    usersText.Visibility = Visibility.Visible;
                }));
            }
        }

        private void FillUser()
        {
            ServiceDispatcher.GetDefaultService().GetUserProfileFor(status.Author.ScreenName, ReceiveUser);
        }

        private void ReceiveUser(TwitterUser user, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => MessageBox.Show("Couldn't get the full user profile."));
            status.User = user;
            Dispatcher.BeginInvoke(() =>
            {
                SetBindings();
                ContentPanel.UpdateLayout();
            });
        }

        private void SetBindings()
        {
            if (status != null)
            {
                ContentPanel.DataContext = null;
                ContentPanel.DataContext = status;
            }
        }

        private void SetImage()
        {
            if (status == null)
                return;

            Uri uriSource = null;
            Uri gotoUri = null;

            if (status.Entities.Media != null && status.Entities.Media.Any())
            {
                var photo = status.Entities.Media.First();
                gotoUri = new Uri(photo.ExpandedUrl, UriKind.Absolute);
                uriSource = new Uri(photo.MediaUrl, UriKind.Absolute);

            }
            else if (status.Entities.Urls != null && status.Entities.Urls.Any())
            {
                foreach (var i in status.Entities.Urls)
                {
                    if (i.EntityType == TwitterEntityType.Url)
                    {
                        var url = i as TwitterUrl;
                        if (url.ExpandedValue.Contains("http://yfrog.com/"))
                        {
                            gotoUri = new Uri(url.ExpandedValue, UriKind.Absolute);
                            uriSource = new Uri(url.ExpandedValue + ":iphone", UriKind.Absolute);
                        }
                        else if (url.ExpandedValue.Contains("http://twitpic.com/"))
                        {
                            gotoUri = new Uri(url.ExpandedValue, UriKind.Absolute);
                            uriSource = new Uri("http://twitpic.com/show/thumb" + url.ExpandedValue.Substring(url.ExpandedValue.LastIndexOf('/')), UriKind.Relative);
                        }
                        else if (url.ExpandedValue.Contains("http://instagr.am/"))
                        {
                            gotoUri = new Uri(url.ExpandedValue, UriKind.Absolute);
                            string idcode;
                            if(url.ExpandedValue.Last() == '/')
                                idcode = url.ExpandedValue.Substring(0, url.ExpandedValue.Length-1);
                            else
                                idcode = url.ExpandedValue;
                            idcode = idcode.Substring(idcode.LastIndexOf('/') + 1);
                            uriSource = new Uri("http://instagr.am/p/" + idcode + "/media/?size=m", UriKind.Absolute);
                        }
                    }
                }
            }

            if (gotoUri == null || uriSource == null)
                return;

            Dispatcher.BeginInvoke(() =>
            {
                pBar.Text = "Downloading image...";
                pBar.IsVisible = true;
            });
            img.MinWidth = 415.0;
            img.Source = new System.Windows.Media.Imaging.BitmapImage(uriSource);
            img.ImageOpened += (sender, e) => { 
                Dispatcher.BeginInvoke(() => { pBar.Text = ""; pBar.IsVisible = false; }); 
            };
            img.ImageFailed += (sender, e) =>
            {
                Dispatcher.BeginInvoke(() =>
                { 
                    pBar.Text = ""; 
                    pBar.IsVisible = false;
                    MessageBox.Show("Error downloading image.");
                }); 
            };
            img.Tap += (sender, e) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    var task = new Microsoft.Phone.Tasks.WebBrowserTask { Uri = gotoUri };
                    task.Show();
                });
            };
        }

        private void SetUsername()
        {
            string RTByText = "";
            if (DataTransfer.Status.RetweetedStatus != null)
                RTByText = " (RT by @" + DataTransfer.Status.Author.ScreenName + ")";

            SName.Text = "@" + status.Author.ScreenName + RTByText;
        }

        private void SetVisibilityOfRepliesAndImages()
        {
            if (status.InReplyToStatusId != null)
                Replies.Visibility = Visibility.Visible;

            if (status.Entities.Media.Count != 0)
                img.Visibility = Visibility.Visible;
        }

        private void AdjustMargins()
        {
            RemoveHTML conv = new RemoveHTML();
            RelativeDateTimeConverter dc = new RelativeDateTimeConverter();

            SecondBlock.Margin = new Thickness(SecondBlock.Margin.Left, Text.ActualHeight + Text.Margin.Top + 10,
                SecondBlock.Margin.Right, SecondBlock.Margin.Bottom);
            ViaDate.Text = (string)dc.Convert(status.CreatedDate, null, null, null) + " via " +
                conv.Convert(status.Source, null, null, null);
        }

        private void CreateText(ITweetable Status)
        {
            var paragraph = new Paragraph();
            var runs = new List<Inline>();

            Text.Blocks.Clear();

            string TweetText = Status.Text;
            string PreviousText;
            int i = 0;

            foreach (var Entity in status.Entities)
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

        Inline CreateHashtagLink(TwitterHashTag Hashtag)
        {
            var link = new Hyperlink();
            link.Inlines.Add(new Run() { Text = "#" + Hashtag.Text });
            link.FontWeight = FontWeights.Bold;
            link.TextDecorations = null;
            link.TargetName = "#" + Hashtag.Text;
            link.Click += new RoutedEventHandler(link_Click);

            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Click += new RoutedEventHandler(CopyLink);
            item.Header = "copy hashtag";
            item.Tag = "#" + Hashtag.Text;
            menu.Items.Add(item);
            ContextMenuService.SetContextMenu(link, menu);

            return link;
        }



        Inline CreateMentionLink(TwitterMention Mention)
        {
            var link = new Hyperlink();
            link.Inlines.Add(new Run() { Text = "@" + Mention.ScreenName });
            link.FontWeight = FontWeights.Bold;
            link.TextDecorations = null;
            link.Click += new RoutedEventHandler(link_Click);

            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Click += new RoutedEventHandler(CopyLink);
            item.Header = "copy user name";
            item.Tag = Mention.ScreenName;
            menu.Items.Add(item);
            ContextMenuService.SetContextMenu(link, menu);

            GestureListener listener = GestureService.GetGestureListener(link);
            if (listener != null)
            {
                listener.Hold += new EventHandler<GestureEventArgs>(OpenContextMenu);
            }

            return link;
        }

        Inline CreateUrlLink(TwitterUrl URL)
        {
            var link = new Hyperlink();
            link.Inlines.Add(new Run() { Text = TweetTextConverter.TrimUrl(URL.ExpandedValue) });
            link.TextDecorations = null;
            link.TargetName = URL.ExpandedValue;
            link.Click += new RoutedEventHandler(link_Click);

            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Click += new RoutedEventHandler(CopyLink);
            item.Header = "copy link";
            item.Tag = URL.ExpandedValue;
            menu.Items.Add(item);
            ContextMenuService.SetContextMenu(link, menu);

            GestureListener listener = GestureService.GetGestureListener(link);
            if (listener != null)
            {
                listener.Hold += new EventHandler<GestureEventArgs>(OpenContextMenu);
            }

            return link;
        }

        Inline CreateMediaLink(TwitterMedia Media)
        {
            var link = new Hyperlink();
            link.Inlines.Add(new Run() { Text = Media.DisplayUrl });
            link.FontWeight = FontWeights.Bold;
            link.TextDecorations = null;
            link.TargetName = Media.ExpandedUrl;
            link.Click += new RoutedEventHandler(link_Click);
            link.Foreground = (System.Windows.Media.Brush)Application.Current.Resources["PhoneAccentBrush"];

            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Click += new RoutedEventHandler(CopyLink);
            item.Header = "copy link";
            item.Tag = Media.DisplayUrl;
            menu.Items.Add(item);
            ContextMenuService.SetContextMenu(link, menu);

            GestureListener listener = GestureService.GetGestureListener(link);
            if (listener != null)
            {
                listener.Hold += new EventHandler<GestureEventArgs>(OpenContextMenu);
            }

            return link;
        }

        void OpenContextMenu(object sender, GestureEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            if (link == null)
                return;

            ContextMenu menu = ContextMenuService.GetContextMenu(link);
            if (menu == null)
                return;

            this.BackKeyPress += CloseContextMenu;
            menu.IsOpen = true;
        }

        void CloseContextMenu(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_menuOpened != null)
            {
                _menuOpened.IsOpen = false;
                _menuOpened = null;
            }

            this.BackKeyPress -= CloseContextMenu;
        }


        void CopyLink(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null && item.Tag is string && !(string.IsNullOrWhiteSpace(item.Tag as string)))
                Clipboard.SetText(item.Tag as string);
        }

        private void ImageClick(object sender, EventArgs e)
        {
            System.Windows.Controls.Image Img = sender as System.Windows.Controls.Image;
            if (Img != null)
                NavigationService.Navigate(new Uri("/Pages/Elements/ImageView.xaml?img=" + Img.Tag.ToString(), UriKind.Relative));
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
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + link.TargetName.Substring(0), UriKind.Relative));
            else if (link.TargetName[0] == '#')
            {
                DataTransfer.Search = link.TargetName;
                NavigationService.Navigate(new Uri("/Pages/Search/Search.xaml?q=" + link.TargetName, UriKind.Relative));
            }

        }

        private void replyButton_Click(object sender, EventArgs e)
        {
            DataTransfer.ReplyId = status.Id;
            DataTransfer.Text = "@" + status.Author.ScreenName + " ";
            DataTransfer.ReplyingDM = false;
            NavigationService.Navigate(Uris.WriteTweet);
        }

        private void replyAllButton_Click(object sender, EventArgs e)
        {
            DataTransfer.ReplyId = status.Id;
            DataTransfer.Text = "@" + status.Author.ScreenName + " ";
            DataTransfer.ReplyingDM = false;

            foreach (string user in StringManipulator.GetUserNames(status.Text))
                DataTransfer.Text += "@" + user + " ";

            NavigationService.Navigate(Uris.WriteTweet);
        }

        private void retweetButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            ServiceDispatcher.GetCurrentService().Retweet(status.Id, (Action<TwitterStatus, TwitterResponse>)receive);
        }

        private void favButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            if (_favBtnState)
            {
                ServiceDispatcher.GetCurrentService().FavoriteTweet(status.Id, (Action<TwitterStatus, TwitterResponse>)receiveFav);
            }
            else
            {
                ServiceDispatcher.GetCurrentService().UnfavoriteTweet(status.Id, receiveFav);
            }
        }

        private void receive(TwitterStatus status, TwitterResponse resp)
        {
            if (resp.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => { MessageBox.Show("An error has occurred :("); });
            Dispatcher.BeginInvoke(() => { pBar.IsVisible = false; Notificator.ShowMessage("Retweeted!", pBar); });
        }

        private void receiveFav(TwitterStatus status, TwitterResponse resp)
        {
            if (resp.StatusCode != HttpStatusCode.OK)
                Dispatcher.BeginInvoke(() => { MessageBox.Show("An error has occurred :("); });
            Dispatcher.BeginInvoke(() => { pBar.IsVisible = false; ToggleFavButton(); Notificator.ShowMessage("Done!", pBar); });
        }

        private void quoteButton_Click(object sender, EventArgs e)
        {
            DataTransfer.ReplyId = 0;
            DataTransfer.Text = "RT @" + status.User.ScreenName + " " + status.Text;

            NavigationService.Navigate(Uris.WriteTweet);
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
            NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + status.Author.ScreenName, UriKind.Relative));
        }

        private void Replies_Tap(object sender, EventArgs e)
        {
            NavigationService.Navigate(Uris.Conversation);
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var Img = sender as System.Windows.Controls.Image;
            if (Img != null && Img.Tag is ITweeter)
                NavigationService.Navigate(new Uri("/Pages/Elements/User.xaml?user=" + (Img.Tag as ITweeter).ScreenName, UriKind.Relative));
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
            var filter = CreateNewFilter(FilterType.User, status.Author.ScreenName);
            Dispatcher.BeginInvoke(() => MessageBox.Show("The user " + status.Author.ScreenName + " is now muted until " + filter.IsValidUntil.ToString("f") + "."));
            MuteGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void MuteHashtags_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ITweetableFilter filter = null;
            string message = "";
            foreach (var entity in status.Entities)
            {
                if (entity.EntityType == TwitterEntityType.HashTag)
                {
                    filter = CreateNewFilter(FilterType.Text, ((TwitterHashTag)entity).Text);
                    message += ((TwitterHashTag)entity).Text + ", ";
                }
            }
            if (message == "")
                Dispatcher.BeginInvoke(() => MessageBox.Show("No hashtags to mute"));
            else
                Dispatcher.BeginInvoke(() => MessageBox.Show("The hashtag(s) " + message.Substring(0, message.Length - 2) + " are now muted until " + filter.IsValidUntil.ToString("f") +"."));
            MuteGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Source_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            RemoveHTML conv = new RemoveHTML();
            string source = conv.Convert(status.Source, null, null, null) as string;
            var filter = CreateNewFilter(FilterType.Source, source);
            Dispatcher.BeginInvoke(() => MessageBox.Show("The source " + source + " is now muted until " + filter.IsValidUntil.ToString("f") + "."));
            MuteGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            MuteGrid.Visibility = System.Windows.Visibility.Visible;
        }
    }
}