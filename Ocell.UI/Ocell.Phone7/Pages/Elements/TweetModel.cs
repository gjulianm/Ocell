using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Collections.Generic;
using System.Windows.Controls;
using Ocell.Commands;
using Ocell.Localization;
using Microsoft.Phone.Shell;

namespace Ocell.Pages.Elements
{
    public class TweetModel : ExtendedViewModelBase
    {
        ApplicationBarMode appBarMode;
        public ApplicationBarMode AppBarMode
        {
            get { return appBarMode; }
            set { Assign("AppBarMode", ref appBarMode, value); }
        }

        bool completed;
        public bool Completed
        {
            get { return completed; }
            set { Assign("Completed", ref completed, value); }
        }

        bool isMuting;
        public bool IsMuting
        {
            get { return isMuting; }
            set { Assign("IsMuting", ref isMuting, value); }
        }

        TwitterStatus tweet;
        public TwitterStatus Tweet
        {
            get { return tweet; }
            set { Assign("Tweet", ref tweet, value); }
        }

        bool hasReplies;
        public bool HasReplies
        {
            get { return hasReplies; }
            set { Assign("HasReplies", ref hasReplies, value); }
        }

        bool isFavorited;
        public bool IsFavorited
        {
            get { return isFavorited; }
            set { Assign("IsFavorited", ref isFavorited, value); }
        }

        bool hasImage;
        public bool HasImage
        {
            get { return hasImage; }
            set { Assign("HasImage", ref hasImage, value); }
        }

        ObservableCollection<ITweeter> usersWhoRetweeted;
        public ObservableCollection<ITweeter> UsersWhoRetweeted
        {
            get { return usersWhoRetweeted; }
            set { Assign("UsersWhoRetweeted", ref usersWhoRetweeted, value); }
        }

        int retweetCount;
        public int RetweetCount
        {
            get { return retweetCount; }
            set { Assign("RetweetCount", ref retweetCount, value); }        }


        bool hasRetweets;
        public bool HasRetweets
        {
            get { return hasRetweets; }
            set { Assign("HasRetweets", ref hasRetweets, value); }
        }

        string whoRetweeted;
        public string WhoRetweeted
        {
            get { return whoRetweeted; }
            set { Assign("WhoRetweeted", ref whoRetweeted, value); }
        }

        string avatar;
        public string Avatar
        {
            get { return avatar; }
            set { Assign("Avatar", ref avatar, value); }
        }

        string replyText;
        public string ReplyText
        {
            get { return replyText; }
            set { Assign("ReplyText", ref replyText, value); }
        }

        DelegateCommand deleteTweet;
        public ICommand DeleteTweet
        {
            get { return deleteTweet; }
        }

        DelegateCommand share;
        public ICommand Share
        {
            get { return share; }
        }

        DelegateCommand quote;
        public ICommand Quote
        {
            get { return quote; }
        }

        DelegateCommand favorite;
        public ICommand Favorite
        {
            get { return favorite; }
        }

        DelegateCommand sendTweet;
        public ICommand SendTweet
        {
            get { return sendTweet; }
        }

        string imageSource;
        public string ImageSource
        {
            get { return imageSource; }
            set { Assign("ImageSource", ref imageSource, value); }
        }

        SafeObservable<ITweetable> replies;
        public SafeObservable<ITweetable> Replies
        {
            get { return replies; }
            protected set { Assign("Replies", ref replies, value); }
        }

        SafeObservable<string> images;
        public SafeObservable<string> Images
        {
            get { return images; }
            protected set { Assign("Images", ref images, value); }
        }

        public event EventHandler<EventArgs<ITweetable>> TweetSent;

        Uri ImageNavigationUri;

        public void Initialize()
        {
            AppBarMode = ApplicationBarMode.Default;

            if (DataTransfer.Status == null)
            {
                MessageService.ShowError(Localization.Resources.ErrorLoadingTweet);
                GoBack();
                return;
            }

            if (DataTransfer.Status.RetweetedStatus != null)
            {
                Tweet = DataTransfer.Status.RetweetedStatus;
                WhoRetweeted = " " + String.Format(Localization.Resources.RetweetBy, DataTransfer.Status.Author.ScreenName);
                HasRetweets = true;
            }
            else
            {
                Tweet = DataTransfer.Status;
                WhoRetweeted = "";
            }

            Avatar = String.Format("https://api.twitter.com/1/users/profile_image?screen_name={0}&size=original", Tweet.Author.ScreenName);

            HasReplies = (Tweet.InReplyToStatusId != null);
            HasImage = (Tweet.Entities != null && Tweet.Entities.Media.Any());
            IsFavorited = Tweet.IsFavorited;
            RetweetCount = Tweet.RetweetCount;

            if (Tweet.User == null || Tweet.User.Name == null)
                FillUser();

            UsersWhoRetweeted = new ObservableCollection<ITweeter>();
            Replies = new SafeObservable<ITweetable>();
            Images = new SafeObservable<string>();

            UsersWhoRetweeted.CollectionChanged += (s, e) =>
            {
                RetweetCount = UsersWhoRetweeted.Count;
            };

            GetRetweets();

            GetReplies();

            CreateCommands();

            SetImage();
        }

        private void GetReplies()
        {
            var convService = new ConversationService(DataTransfer.CurrentAccount);
            convService.Finished += (sender, e) => IsLoading = false;
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

        private void GetRetweets()
        {
            ServiceDispatcher.GetDefaultService().Retweets(new RetweetsOptions { Id = Tweet.Id }, (statuses, response) =>
            {
                if (statuses != null && statuses.Any())
                {
                    HasRetweets = true;
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var rt in statuses)
                            UsersWhoRetweeted.Add(rt.Author);
                    });
                }
            });
        }

        public TweetModel()
            : base("Tweet")
        {
            Initialize();
        }

        private void CreateCommands()
        {
            deleteTweet = new DelegateCommand((obj) =>
            {
                var user = Config.Accounts.FirstOrDefault(item => item != null && item.ScreenName == Tweet.Author.ScreenName);

                ServiceDispatcher.GetService(user).DeleteTweet(new DeleteTweetOptions { Id = Tweet.Id }, (s, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        MessageService.ShowMessage(Localization.Resources.TweetDeleted, "");
                    else
                        MessageService.ShowError(Localization.Resources.ErrorDeletingTweet);
                });
            }, (obj) => Tweet != null && Tweet.Author != null && Config.Accounts.Any(item => item != null && item.ScreenName == Tweet.Author.ScreenName));


            share = new DelegateCommand((obj) => Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();

                emailComposeTask.Subject = String.Format(Localization.Resources.TweetFrom, Tweet.Author.ScreenName);
                emailComposeTask.Body = "@" + Tweet.Author.ScreenName + ": " + Tweet.Text +
                    Environment.NewLine + Environment.NewLine + Tweet.CreatedDate.ToString();

                emailComposeTask.Show();
            }), obj => Tweet != null);

            quote = new DelegateCommand((obj) =>
            {
                DataTransfer.Text = "RT @" + Tweet.Author.ScreenName + ": " + Tweet.Text;
                Navigate(Uris.WriteTweet);
            }, obj => Config.Accounts.Any() && Tweet != null);

            favorite = new DelegateCommand((parameter) =>
            {
                TwitterStatus param = (TwitterStatus)parameter;
                if (IsFavorited)
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfavoriteTweet(new UnfavoriteTweetOptions { Id = param.Id }, (sts, resp) =>
                    {
                        MessageService.ShowLightNotification(Localization.Resources.Unfavorited);
                        IsFavorited = false;
                    });
                else
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FavoriteTweet(new FavoriteTweetOptions { Id = param.Id }, (sts, resp) =>
                    {
                        MessageService.ShowLightNotification(Localization.Resources.Favorited);
                        IsFavorited = true;
                    });
            }, parameter => (parameter is TwitterStatus) && Config.Accounts.Count > 0 && DataTransfer.CurrentAccount != null);

            sendTweet = new DelegateCommand((parameter) =>
            {
                IsLoading = true;
                BarText = Resources.SendingTweet;
                ServiceDispatcher.GetCurrentService().SendTweet(new SendTweetOptions
                {
                    InReplyToStatusId = Tweet.Id,
                    Status = ReplyText
                }, (status, response) =>
                {
                    IsLoading = false;
                    BarText = "";
                    if (response.StatusCode != HttpStatusCode.OK)
                        MessageService.ShowError(response.Error != null ? response.Error.Message : Resources.UnknownValue);
                    else if (TweetSent != null)
                        TweetSent(this, new EventArgs<ITweetable>(status));
                });
            });
        }

        void FillUser()
        {
            ServiceDispatcher.GetDefaultService().GetUserProfileFor(new GetUserProfileForOptions { ScreenName = Tweet.Author.ScreenName }, (user, response) =>
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        MessageService.ShowError(Localization.Resources.ErrorGettingProfile);
                    Tweet.User = user;
                });
        }

        public void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            IsLoading = false;
            MessageService.ShowError(Localization.Resources.ErrorDownloadingImage);
        }

        public void ImageOpened(object sender, RoutedEventArgs e)
        {
            IsLoading = false;
            BarText = "";
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
                foreach (var i in Tweet.Entities.Urls)
                {
                    if (i.EntityType == TwitterEntityType.Url)
                    {
                        var url = i as TwitterUrl;
                        if (url != null && !string.IsNullOrWhiteSpace(url.ExpandedValue))
                        {
                            string photoUrl;
                            if (parser.TryGetMediaUrl(url.ExpandedValue, out photoUrl))
                                Images.Add(photoUrl);
                        }
                    }
                }
            }

            if (Images.Count > 0)
            {
                HasImage = true;
                IsLoading = true;
                BarText = Localization.Resources.DownloadingImage;
            }
        }

        public void ReplyBoxGotFocus()
        {
            ReplyText = ReplyAllCommand.GetReplied(Tweet);
        }

        public void ReplyBoxLostFocus()
        {
            ReplyText = String.Format("{0}...", Resources.Reply);
        }
    }
}
