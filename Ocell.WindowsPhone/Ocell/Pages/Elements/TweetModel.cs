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

namespace Ocell.Pages.Elements
{
    public class TweetModel : ExtendedViewModelBase
    {
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

        string hdAvatar;
        public string HdAvatar
        {
            get { return hdAvatar; }
            set { Assign("HdAvatar", ref hdAvatar, value); }
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

        string imageSource;
        public string ImageSource
        {
            get { return imageSource; }
            set { Assign("ImageSource", ref imageSource, value); }
        }

        Uri ImageNavigationUri;

        public void Initialize()
        {
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

            HdAvatar = String.Format("https://api.twitter.com/1/users/profile_image?screen_name={0}&size=bigger", Tweet.Author.ScreenName);

            HasReplies = (Tweet.InReplyToStatusId != null);
            HasImage = (Tweet.Entities != null && Tweet.Entities.Media.Any());
            IsFavorited = Tweet.IsFavorited;

            var service = new ConversationService(DataTransfer.CurrentAccount);
            service.CheckIfReplied(Tweet, (replied) =>
                {
                    if (replied)
                        HasReplies = true;
                });

            if (Tweet.User == null || Tweet.User.Name == null)
                FillUser();

            UsersWhoRetweeted = new ObservableCollection<ITweeter>();

            UsersWhoRetweeted.CollectionChanged += (s, e) =>
            {
                RetweetCount = UsersWhoRetweeted.Count;
            };

            ServiceDispatcher.GetDefaultService().Retweets(Tweet.Id, (statuses, response) =>
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

            CreateCommands();

            SetImage();
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
                ServiceDispatcher.GetService(user).DeleteTweet(Tweet.Id, (s, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        MessageService.ShowMessage(Localization.Resources.TweetDeleted, "");
                    else
                        MessageService.ShowError(Localization.Resources.ErrorDeletingTweet);
                });
            }, (obj) =>
                Config.Accounts.Any(item => item != null && item.ScreenName == Tweet.Author.ScreenName));


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
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfavoriteTweet(param.Id, (sts, resp) =>
                    {
                        MessageService.ShowLightNotification(Localization.Resources.Unfavorited);
                        IsFavorited = false;
                    });
                else
                    ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FavoriteTweet(param.Id, (sts, resp) =>
                    {
                        MessageService.ShowLightNotification(Localization.Resources.Favorited);
                        IsFavorited = true;
                    });
            }, parameter => (parameter is TwitterStatus) && Config.Accounts.Count > 0 && DataTransfer.CurrentAccount != null);
        }

        void FillUser()
        {
            ServiceDispatcher.GetDefaultService().GetUserProfileFor(Tweet.Author.ScreenName, (user, response) =>
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
            if (ImageNavigationUri != null)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    var task = new Microsoft.Phone.Tasks.WebBrowserTask { Uri = ImageNavigationUri };
                    task.Show();
                });
        }

        void SetImage()
        {
            if (Tweet.Entities == null)
                return;

            if (Tweet.Entities.Media != null && Tweet.Entities.Media.Any())
            {
                var photo = Tweet.Entities.Media.First();
                ImageNavigationUri = new Uri(photo.ExpandedUrl, UriKind.Absolute);
                ImageSource = photo.MediaUrl;

            }
            else if (Tweet.Entities.Urls != null && Tweet.Entities.Urls.Any())
            {
                foreach (var i in Tweet.Entities.Urls)
                {
                    if (i.EntityType == TwitterEntityType.Url)
                    {
                        var url = i as TwitterUrl;
                        if (url != null && !string.IsNullOrWhiteSpace(url.ExpandedValue))
                        {
                            if (url.ExpandedValue.Contains("http://yfrog.com/"))
                            {
                                ImageNavigationUri = new Uri(url.ExpandedValue, UriKind.Absolute);
                                ImageSource = url.ExpandedValue + ":iphone";
                            }
                            else if (url.ExpandedValue.Contains("http://twitpic.com/"))
                            {
                                ImageNavigationUri = new Uri(url.ExpandedValue, UriKind.Absolute);
                                ImageSource = "http://twitpic.com/show/thumb" + url.ExpandedValue.Substring(url.ExpandedValue.LastIndexOf('/'));
                            }
                            else if (url.ExpandedValue.Contains("http://instagr.am/"))
                            {
                                ImageNavigationUri = new Uri(url.ExpandedValue, UriKind.Absolute);
                                string idcode;

                                if (url.ExpandedValue.Last() == '/')
                                    idcode = url.ExpandedValue.Substring(0, url.ExpandedValue.Length - 1);
                                else
                                    idcode = url.ExpandedValue;

                                idcode = idcode.Substring(idcode.LastIndexOf('/') + 1);
                                ImageSource = "http://instagr.am/p/" + idcode + "/media/?size=m";
                            }
                        }
                    }
                }
            }

            if (ImageNavigationUri != null)
            {
                HasImage = true;
                IsLoading = true;
                BarText = Localization.Resources.DownloadingImage;
            }
        }
    }
}
