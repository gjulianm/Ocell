using AncoraMVVM.Base;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Commands;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    [ImplementPropertyChanged]
    public class TweetModel : ExtendedViewModelBase
    {
        public ApplicationBarMode AppBarMode { get; set; }
        public bool Completed { get; set; }
        public bool IsMuting { get; set; }
        public TwitterStatus Tweet { get; set; }
        public bool HasReplies { get; set; }
        public bool IsFavorited { get; set; }
        public bool HasImage { get; set; }
        public ObservableCollection<ITweeter> UserList { get; set; }
        public int RetweetCount { get; set; }
        public int FavoriteCount { get; set; }
        public List<ITweeter> RetweetingUsers { get; set; }
        public bool ShowRetweeters { get; set; }
        public bool ShowFavoriters { get; set; } // Hey, I'm sorry dictionary.
        public string WhoRetweeted { get; set; }
        public string Avatar { get; set; }
        public string ReplyText { get; set; }
        public string ImageSource { get; set; }
        public SafeObservable<ITweetable> Replies { get; set; }
        public SafeObservable<string> Images { get; set; }
        public string WebUrl { get; set; }
        public bool ShowWebLink { get; set; }

        public DelegateCommand DeleteTweet { get; set; }
        public DelegateCommand Share { get; set; }
        public DelegateCommand Quote { get; set; }
        public DelegateCommand Favorite { get; set; }
        public DelegateCommand SendTweet { get; set; }
        public DelegateCommand NavigateToAuthor { get; set; }
        public DelegateCommand MuteUser { get; set; }
        public DelegateCommand MuteHashtags { get; set; }
        public DelegateCommand MuteSource { get; set; }
        public DelegateCommand MuteDialogToggle { get; set; }
        public event EventHandler<EventArgs<ITweetable>> TweetSent;

        private List<string> ImagesOriginalUrls = new List<string>();

        #region Initialization and parsing
        public TweetModel()
        {
            UserList = new ObservableCollection<ITweeter>();
            Replies = new SafeObservable<ITweetable>();
            Images = new SafeObservable<string>();

            AppBarMode = ApplicationBarMode.Default;

            Tweet = ReceiveMessage<TwitterStatus>();

            if (Tweet == null)
            {
                Notificator.ShowError(Localization.Resources.ErrorLoadingTweet);
                Navigator.GoBack();
                return;
            }

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ShowRetweeters")
                {
                    if (ShowRetweeters)
                    {
                        ShowFavoriters = false;
                        UserList = new ObservableCollection<ITweeter>(RetweetingUsers ?? new List<ITweeter>());
                        this.PropertyChanged += UpdateRetweetingUsers;
                    }
                    else
                    {
                        UserList = null;
                        this.PropertyChanged -= UpdateRetweetingUsers;
                    }
                }
                else if (e.PropertyName == "ShowFavoriters")
                {
                    if (ShowFavoriters)
                        ShowFavoriters = false; // Change this whenever Twitter lets us get the users favoriting a tweet.
                }
            };

            CheckRetweeted();
            SetAvatar();
            SetImage();
            SetupCommands();
            ParseForWebLinks();

            HasReplies = (Tweet.InReplyToStatusId != null);
            HasImage = (Tweet.Entities != null && Tweet.Entities.Media.Any());
            IsFavorited = Tweet.IsFavorited;
            RetweetCount = Tweet.RetweetCount;
            FavoriteCount = Tweet.FavoriteCount;

            if (Tweet.User == null || Tweet.User.Name == null)
                FillUser();

            Replies.CollectionChanged += (s, e) => HasReplies = Replies.Any();
        }

        private void SetAvatar()
        {
            if (Tweet.User != null && Tweet.User.ProfileImageUrl != null)
                Avatar = Tweet.User.ProfileImageUrl.Replace("_normal", "");
        }

        private void GetReplies()
        {
            var convService = new ConversationService(DataTransfer.CurrentAccount);
            convService.Finished += (sender, e) => Progress.IsLoading = false;
            convService.GetConversationForStatus(Tweet, (statuses, response) =>
            {
                if (statuses != null)
                {
                    var statuses_noRepeat = statuses.Cast<ITweetable>().Except(Replies).ToList();
                    foreach (var status in statuses_noRepeat)
                        Replies.Add(status);
                }
            });

        }

        private async void GetRetweets()
        {
            var service = ServiceDispatcher.GetCurrentService();

            if (service != null && Tweet != null)
            {
                var response = await service.RetweetsAsync(new RetweetsOptions { Id = Tweet.Id });
                var statuses = response.Content;

                if (response.RequestSucceeded)
                    RetweetingUsers = statuses.Select(x => x.Author).ToList();
            }
        }

        private void ParseForWebLinks()
        {
            if (Config.WebOptions.Value == EmbeddedWebOptions.None)
                return;

            var candidateLink = Tweet.Entities.Urls.Select(x => x.ExpandedValue).Except(ImagesOriginalUrls).FirstOrDefault();

            ShowWebLink = candidateLink != null;

            if (Config.WebOptions.Value == EmbeddedWebOptions.FullWeb)
                WebUrl = candidateLink;
            else if (Config.WebOptions.Value == EmbeddedWebOptions.Readability)
                WebUrl = "http://www.readability.com/m?url=" + candidateLink;
        }

        private void CheckRetweeted()
        {
            if (Tweet.RetweetedStatus != null)
            {
                Tweet = Tweet.RetweetedStatus;
                WhoRetweeted = " " + String.Format(Localization.Resources.RetweetBy, Tweet.Author.ScreenName);
            }
        }

        private void SetupCommands()
        {
            DeleteTweet = new DelegateCommand(async (obj) =>
            {
                var user = Config.Accounts.Value.FirstOrDefault(item => item != null && item.ScreenName == Tweet.Author.ScreenName);

                var response = await ServiceDispatcher.GetService(user).DeleteTweetAsync(new DeleteTweetOptions { Id = Tweet.Id });
                if (response.RequestSucceeded)
                    Notificator.ShowMessage(Localization.Resources.TweetDeleted);
                else
                    Notificator.ShowError(Localization.Resources.ErrorDeletingTweet);
            }, (obj) => Tweet != null && Tweet.Author != null && Config.Accounts.Value.Any(item => item != null && item.ScreenName == Tweet.Author.ScreenName));


            Share = new DelegateCommand((obj) => Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();

                emailComposeTask.Subject = String.Format(Localization.Resources.TweetFrom, Tweet.Author.ScreenName);
                emailComposeTask.Body = "@" + Tweet.Author.ScreenName + ": " + Tweet.Text +
                    Environment.NewLine + Environment.NewLine + Tweet.CreatedDate.ToString();

                emailComposeTask.Show();
            }), obj => Tweet != null);

            Quote = new DelegateCommand((obj) =>
            {
                Navigator.MessageAndNavigate<NewTweetModel, NewTweetArgs>(new NewTweetArgs
                {
                    Text = String.Format("RT @{0}: {1}", Tweet.Author.ScreenName, Tweet.Text),
                    ReplyToId = Tweet.Id
                });
            },
            obj => Config.Accounts.Value.Any() && Tweet != null);

            Favorite = new DelegateCommand(async () =>
            {
                var favCmd = new FavoriteCommand();
                var result = await favCmd.ExecuteAsync(Tweet);

                if (result)
                {
                    IsFavorited = !IsFavorited;
                    Tweet.IsFavorited = IsFavorited;
                }
            }, () => Tweet != null && Config.Accounts.Value.Count > 0 && DataTransfer.CurrentAccount != null);

            Favorite.BindCanExecuteToProperty(this, "Tweet", "IsFavorited");

            SendTweet = new DelegateCommand(async (parameter) =>
            {
                Progress.IsLoading = true;
                Progress.Text = Resources.SendingTweet;
                var response = await ServiceDispatcher.GetCurrentService().SendTweetAsync(new SendTweetOptions { InReplyToStatusId = Tweet.Id, Status = ReplyText });

                Progress.IsLoading = false;
                Progress.Text = "";
                if (!response.RequestSucceeded)
                    Notificator.ShowError(response.Error != null ? response.Error.Message : Resources.UnknownValue);
                else if (TweetSent != null)
                    TweetSent(this, new EventArgs<ITweetable>(response.Content));

            });

            NavigateToAuthor = new DelegateCommand((param) =>
            {
                Navigator.MessageAndNavigate<UserModel, TargetUser>(new TargetUser { Username = Tweet.AuthorName, User = Tweet.Author as TwitterUser });
            }, p => Tweet != null && (Tweet.Author != null || !string.IsNullOrWhiteSpace(Tweet.AuthorName)));

            NavigateToAuthor.BindCanExecuteToProperty(this, "Tweet");

            SetupMuteCommands();
        }

        async void FillUser()
        {
            var response = await ServiceDispatcher.GetDefaultService().GetUserProfileForAsync(new GetUserProfileForOptions { ScreenName = Tweet.Author.ScreenName });

            var user = response.Content;

            if (!response.RequestSucceeded)
                Notificator.ShowError(Localization.Resources.ErrorGettingProfile);

            Tweet.User = user;
            SetAvatar();
        }

        public override void OnLoad()
        {
            GetRetweets();
            GetReplies();
        }
        #endregion

        private void UpdateRetweetingUsers(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RetweetingUsers")
                UserList = new ObservableCollection<ITweeter>(RetweetingUsers ?? new List<ITweeter>());
        }

        #region Image management
        public void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Progress.IsLoading = false;
            Notificator.ShowError(Localization.Resources.ErrorDownloadingImage);
        }

        public void ImageOpened(object sender, RoutedEventArgs e)
        {
            Progress.IsLoading = false;
            Progress.Text = "";
        }

        public void ImageTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            if (img != null)
            {
                var url = img.Tag as string;
                if (url != null && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var task = new Microsoft.Phone.Tasks.WebBrowserTask { Uri = new Uri(url, UriKind.Absolute) };
                        task.Show();
                    });
                }
            }
        }

        void SetImage()
        {
            if (Tweet.Entities == null)
                return;

            if (Tweet.Entities.Media != null && Tweet.Entities.Media.Any())
            {
                var photo = Tweet.Entities.Media.First();
                Images.Add(photo.MediaUrl);

            }

            if (Tweet.Entities.Urls != null && Tweet.Entities.Urls.Any())
            {
                var parser = new MediaLinkParser();

                foreach (var url in Tweet.Entities.Urls)
                {
                    if (url != null && !string.IsNullOrWhiteSpace(url.ExpandedValue))
                    {
                        string photoUrl;
                        if (parser.TryGetMediaUrl(url.ExpandedValue, out photoUrl) && !Images.Contains(photoUrl))
                        {
                            Images.Add(photoUrl);
                            ImagesOriginalUrls.Add(url.ExpandedValue);
                        }
                    }
                }
            }

            if (Images.Count > 0)
            {
                HasImage = true;
                Progress.IsLoading = true;
                Progress.Text = Localization.Resources.DownloadingImage;
            }
        }
        #endregion

        public void ReplyBoxGotFocus()
        {
            ReplyText = ReplyAllCommand.GetReplied(Tweet);
        }


        #region Filters and muting
        private void CreateAndSetupFilter(Func<TimeSpan, ElementFilter<ITweetable>> filterCreator)
        {
            var filter = filterCreator((TimeSpan)Config.DefaultMuteTime.Value);

            Config.GlobalFilter.Value.Add(filter);
            Config.SaveGlobalFilter();

            Notificator.ShowMessage(string.Format(Resources.MutedUntil, filter.Filter, filter.IsValidUntil.ToString("f")));
            IsMuting = false;
        }

        private void SetupMuteCommands()
        {
            MuteHashtags = new DelegateCommand(() =>
            {
                foreach (var hashtag in Tweet.Entities.HashTags.Select(x => x.Text))
                    CreateAndSetupFilter(ts => new HashtagFilter(hashtag, ts));
            }, () => Tweet != null && Tweet.Entities != null && Tweet.Entities.HashTags != null && Tweet.Entities.HashTags.Any());

            MuteHashtags.BindCanExecuteToProperty(this, "Tweet");

            MuteSource = new DelegateCommand(() => CreateAndSetupFilter(ts => new SourceFilter(Tweet.Source, ts)), () => Tweet != null);
            MuteSource.BindCanExecuteToProperty(this, "Tweet");

            MuteUser = new DelegateCommand(() => CreateAndSetupFilter(ts => new UserFilter(Tweet.AuthorName, ts)), () => Tweet != null);
            MuteUser.BindCanExecuteToProperty(this, "Tweet");

            MuteDialogToggle = new DelegateCommand(() => IsMuting = !IsMuting);
        }
        #endregion
    }
}
