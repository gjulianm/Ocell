using AncoraMVVM.Base;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Ocell.Commands;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public ObservableCollection<ITweeter> UsersWhoRetweeted { get; set; }

        public int RetweetCount { get; set; }


        public bool HasRetweets { get; set; }

        public string WhoRetweeted { get; set; }

        public string Avatar { get; set; }

        public string ReplyText { get; set; }

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

        public string ImageSource { get; set; }

        public SafeObservable<ITweetable> Replies { get; set; }

        public SafeObservable<string> Images { get; set; }

        public event EventHandler<EventArgs<ITweetable>> TweetSent;

        Uri ImageNavigationUri;

        public void Initialize()
        {
            AppBarMode = ApplicationBarMode.Default;

            if (DataTransfer.Status == null)
            {
                Notificator.ShowError(Localization.Resources.ErrorLoadingTweet);
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
            SetAvatar();

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

            CreateCommands();
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

                if (response.RequestSucceeded && statuses.Any())
                {
                    HasRetweets = true;
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var rt in statuses)
                            UsersWhoRetweeted.Add(rt.Author);
                    });
                }

            }
        }

        public TweetModel()
        {
            Initialize();
        }

        public void OnLoad()
        {
            ThreadPool.QueueUserWorkItem((c) =>
            {
                GetRetweets();
                GetReplies();
                CreateCommands();
                SetImage();
            });
        }

        private void CreateCommands()
        {
            deleteTweet = new DelegateCommand(async (obj) =>
            {
                var user = Config.Accounts.FirstOrDefault(item => item != null && item.ScreenName == Tweet.Author.ScreenName);

                var response = await ServiceDispatcher.GetService(user).DeleteTweetAsync(new DeleteTweetOptions { Id = Tweet.Id });
                if (response.RequestSucceeded)
                    Notificator.ShowMessage(Localization.Resources.TweetDeleted);
                else
                    Notificator.ShowError(Localization.Resources.ErrorDeletingTweet);
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

            // TODO: These are the same commands that are used globally. WTF.
            // TODO: And responses aren't checked again. Holy shit.
            favorite = new DelegateCommand(async (parameter) =>
            {
                TwitterStatus param = (TwitterStatus)parameter;
                if (IsFavorited)
                {
                    await ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfavoriteTweetAsync(new UnfavoriteTweetOptions { Id = param.Id });
                    Notificator.ShowProgressIndicatorMessage(Localization.Resources.Unfavorited);
                    IsFavorited = false;
                }
                else
                {
                    await ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FavoriteTweetAsync(new FavoriteTweetOptions { Id = param.Id });
                    Notificator.ShowProgressIndicatorMessage(Localization.Resources.Favorited);
                    IsFavorited = true;
                }
            }, parameter => (parameter is TwitterStatus) && Config.Accounts.Count > 0 && DataTransfer.CurrentAccount != null);

            sendTweet = new DelegateCommand(async (parameter) =>
            {
                Progress.IsLoading = true;
                BarText = Resources.SendingTweet;
                var response = await ServiceDispatcher.GetCurrentService().SendTweetAsync(new SendTweetOptions { InReplyToStatusId = Tweet.Id, Status = ReplyText });

                Progress.IsLoading = false;
                BarText = "";
                if (!response.RequestSucceeded)
                    Notificator.ShowError(response.Error != null ? response.Error.Message : Resources.UnknownValue);
                else if (TweetSent != null)
                    TweetSent(this, new EventArgs<ITweetable>(response.Content));

            });
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

        public void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Progress.IsLoading = false;
            Notificator.ShowError(Localization.Resources.ErrorDownloadingImage);
        }

        public void ImageOpened(object sender, RoutedEventArgs e)
        {
            Progress.IsLoading = false;
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
                Progress.IsLoading = true;
                BarText = Localization.Resources.DownloadingImage;
            }
        }

        public void ReplyBoxGotFocus()
        {
            ReplyText = ReplyAllCommand.GetReplied(Tweet);
        }
    }
}
