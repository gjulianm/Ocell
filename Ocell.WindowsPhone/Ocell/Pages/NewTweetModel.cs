using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows.Input;
using System.Xml.Linq;
using DanielVaughan.Windows;
using Hammock;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.Localization;
using TweetSharp;
using Sharplonger;
using System.Windows.Media;

namespace Ocell.Pages
{
    public class NewTweetModel : ExtendedViewModelBase
    {
        #region Fields
        IEnumerable<UserToken> accountList;
        public IEnumerable<UserToken> AccountList
        {
            get { return accountList; }
            set { Assign("AccountList", ref accountList, value); }
        }

        bool isDM;
        public bool IsDM
        {
            get { return isDM; }
            set { Assign("IsDM", ref isDM, value); }
        }

        string tweetText;
        public string TweetText
        {
            get { return tweetText; }
            set { Assign("TweetText", ref tweetText, value); }
        }

        int remainingChars;
        public int RemainingChars
        {
            get { return remainingChars; }
            set { Assign("RemainingChars", ref remainingChars, value); }
        }

        string remainingCharsStr;
        public string RemainingCharsStr
        {
            get { return remainingCharsStr; }
            set { Assign("RemainingCharsStr", ref remainingCharsStr, value); }
        }

        Brush remainingCharsColor;
        public Brush RemainingCharsColor
        {
            get { return remainingCharsColor; }
            set { Assign("RemainingCharsColor", ref remainingCharsColor, value); }
        }

        bool usesTwitlonger;
        public bool UsesTwitlonger
        {
            get { return usesTwitlonger; }
            set { Assign("UsesTwitlonger", ref usesTwitlonger, value); }
        }

        bool isScheduled;
        public bool IsScheduled
        {
            get { return isScheduled; }
            set { Assign("IsScheduled", ref isScheduled, value); }
        }

        DateTime scheduledDate;
        public DateTime ScheduledDate
        {
            get { return scheduledDate; }
            set { Assign("ScheduledDate", ref scheduledDate, value); }
        }

        DateTime scheduledTime;
        public DateTime ScheduledTime
        {
            get { return scheduledTime; }
            set { Assign("ScheduledTime", ref scheduledTime, value); }
        }

        bool sendingDM;
        public bool SendingDM
        {
            get { return sendingDM; }
            set { Assign("SendingDM", ref sendingDM, value); }
        }

        IList selectedAccounts;
        public IList SelectedAccounts
        {
            get { return selectedAccounts; }
            set { Assign("SelectedAccounts", ref selectedAccounts, value); }
        }

        bool isGeotagged;
        public bool IsGeotagged
        {
            get { return isGeotagged; }
            set { Assign("IsGeotagged", ref isGeotagged, value); }
        }

        bool geotagEnabled;
        public bool GeotagEnabled
        {
            get { return geotagEnabled; }
            set { Assign("GeotagEnabled", ref geotagEnabled, value); }
        }
        #endregion

        #region Commands
        DelegateCommand sendTweet;
        public ICommand SendTweet
        {
            get { return sendTweet; }
        }

        DelegateCommand scheduleTweet;
        public ICommand ScheduleTweet
        {
            get { return scheduleTweet; }
        }

        DelegateCommand saveDraft;
        public ICommand SaveDraft
        {
            get { return saveDraft; }
        }

        DelegateCommand selectImage;
        public ICommand SelectImage
        {
            get { return selectImage; }
        }
        #endregion

        GeoCoordinateWatcher geoWatcher = new GeoCoordinateWatcher();
        int requestsLeft;

        public NewTweetModel()
            : base("NewTweet")
        {
            SelectedAccounts = new List<object>();
            AccountList = Config.Accounts.ToList();
            IsGeotagged = Config.EnabledGeolocation == true &&
                (Config.TweetGeotagging == true || Config.TweetGeotagging == null);
            GeotagEnabled = Config.EnabledGeolocation == true;

            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "IsLoading":
                        RaiseExecuteChanged();
                        break;
                    case "SelectedAccounts":
                        RaiseExecuteChanged();
                        break;
                    case "TweetText":
                        RemainingChars = 140 - TweetText.Length;
                        break;
                    case "UsesTwitlonger":
                        RaiseExecuteChanged();
                        break;
                    case "IsGeotagged":
                        Config.TweetGeotagging = IsGeotagged;
                        break;
                    case "RemainingChars":
                        SetRemainingChars();
                        break;
                }
            };

            IsDM = DataTransfer.ReplyingDM;

            // Avoid that ugly 01/01/0001 by default.
            var date = DateTime.Now.AddHours(1);
            ScheduledDate = date;
            ScheduledTime = date;

            TryLoadDraft();

            if (Config.EnabledGeolocation == true)
                geoWatcher.Start();

            SetupCommands();
        }

        Brush redBrush = new SolidColorBrush(Colors.Red);
        void SetRemainingChars()
        {
            if (RemainingChars >= 0)
            {
                RemainingCharsStr = RemainingChars.ToString();
                RemainingCharsColor = App.Current.Resources["PhoneSubtleBrush"] as Brush;
            }
            else if (RemainingChars >= -10)
            {
                RemainingCharsStr = RemainingChars.ToString();
                UsesTwitlonger = true;
                RemainingCharsColor = redBrush;
            }
            else
            {
                RemainingCharsStr = "Twitlonger";
                UsesTwitlonger = true;
                RemainingCharsColor = App.Current.Resources["PhoneSubtleBrush"] as Brush;
            }
        }

        public void TryLoadDraft()
        {
            TwitterDraft draft = DataTransfer.Draft;
            if (draft != null)
            {
                TweetText = draft.Text;

                if (draft.Scheduled != null)
                {
                    IsScheduled = true;
                    ScheduledTime = draft.Scheduled.GetValueOrDefault();
                    ScheduledDate = draft.Scheduled.GetValueOrDefault();
                }
            }
            else
            {
                TweetText = DataTransfer.Text == null ? "" : DataTransfer.Text;
            }
        }

        void SetupCommands()
        {
            sendTweet = new DelegateCommand(Send, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !IsLoading);
            scheduleTweet = new DelegateCommand(Schedule, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !IsLoading);
            selectImage = new DelegateCommand(StartImageChooser, (param) => SelectedAccounts.Count > 0 && !IsLoading);
            saveDraft = new DelegateCommand(SaveAsDraft, (param) => !IsLoading);
        }

        void RaiseExecuteChanged()
        {
            sendTweet.RaiseCanExecuteChanged();
            scheduleTweet.RaiseCanExecuteChanged();
            selectImage.RaiseCanExecuteChanged();
            saveDraft.RaiseCanExecuteChanged();
        }

        void Send(object param)
        {
            if (!CheckProtectedAccounts())
                return;

            requestsLeft = 0;

            BarText = Resources.SendingTweet;
            IsLoading = true;

            if (IsDM)
            {
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).SendDirectMessage((int)DataTransfer.DMDestinationId, TweetText, ReceiveDM);
            }
            else
            {
                if (IsGeotagged)
                {
                    var location = geoWatcher.Position.Location;

                    foreach (UserToken account in SelectedAccounts.Cast<UserToken>())
                    {
                        ServiceDispatcher.GetService(account).SendTweet(TweetText, DataTransfer.ReplyId,
                            location.Latitude, location.Longitude, ReceiveResponse);
                        requestsLeft++;
                    }
                }
                else if (UsesTwitlonger)
                {
                    if (!EnsureTwitlonger())
                    {
                        IsLoading = false;
                        return;
                    }

                    BarText = Resources.UploadingTwitlonger;
                    foreach (UserToken account in SelectedAccounts.Cast<UserToken>())
                    {
                        ServiceDispatcher.GetTwitlongerService(account).PostUpdate(TweetText, ReceiveTLResponse);
                        requestsLeft++;
                    }
                }
                else
                {
                    foreach (UserToken account in SelectedAccounts.Cast<UserToken>())
                    {
                        ServiceDispatcher.GetService(account).SendTweet(TweetText, DataTransfer.ReplyId, ReceiveResponse);
                        requestsLeft++;
                    }
                }
            }

            if (DataTransfer.Draft != null)
            {
                if (Config.Drafts.Contains(DataTransfer.Draft))
                    Config.Drafts.Remove(DataTransfer.Draft);

                DataTransfer.Draft = null;
                Config.SaveDrafts();
            }
        }

        bool EnsureTwitlonger()
        {
            return MessageService.AskOkCancelQuestion(Resources.AskTwitlonger);
        }

        object dicLock = new object();
        Dictionary<string, string> TwitlongerIds = new Dictionary<string, string>();

        void ReceiveTLResponse(TwitlongerPost post, TwitlongerResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK || post == null || post.Post == null || string.IsNullOrEmpty(post.Post.Content) || response.Sender == null)
            {
                IsLoading = false;
                MessageService.ShowError(Resources.ErrorCreatingTwitlonger);
                return;
            }

            BarText = Resources.SendingTweet;

            string name = response.Sender.Username;

            var account = Config.Accounts.FirstOrDefault(x => x.ScreenName == name);

            if (account == null)
            {
                IsLoading = false;
                MessageService.ShowError(Resources.ErrorCreatingTwitlonger);
                return;
            }

            lock (dicLock)
                TwitlongerIds.Add(name, post.Post.Id);

            if (IsGeotagged)
            {
                var location = geoWatcher.Position.Location;
                ServiceDispatcher.GetService(account).SendTweet(post.Post.Content, DataTransfer.ReplyId,
                    location.Latitude, location.Longitude, ReceiveResponse);
            }
            else
            {
                ServiceDispatcher.GetService(account).SendTweet(post.Post.Content, DataTransfer.ReplyId, ReceiveResponse);
            }
        }

        void ReceiveResponse(TwitterStatus status, TwitterResponse response)
        {
            requestsLeft--;

            if (requestsLeft <= 0)
                IsLoading = false;

            if (response.StatusCode == HttpStatusCode.Forbidden)
                MessageService.ShowError(Resources.ErrorDuplicateTweet);
            else if (response.StatusCode != HttpStatusCode.OK)
                MessageService.ShowError(Resources.ErrorMessage);
            else 
            {
                TryAssociateWithTLId(status.Author.ScreenName, status.Id);
                if (requestsLeft <= 0)
                {
                    TweetText = "";
                    DataTransfer.Text = "";
                    GoBack();
                }
            }
        }

        void TryAssociateWithTLId(string name, long tweetId)
        {
            if (!UsesTwitlonger)
                return;

            string id = null;
            lock (dicLock)
                TwitlongerIds.Where(x => x.Key == name).Select(x => x.Value).FirstOrDefault();

            if (id != null)
                ServiceDispatcher.GetTwitlongerService(name).SetId(id, tweetId, null);
        }

        void ReceiveDM(TwitterDirectMessage DM, TwitterResponse response)
        {
            IsLoading = false;
            BarText = "";

            if (response.StatusCode == HttpStatusCode.Forbidden)
                MessageService.ShowError(Resources.ErrorDuplicateTweet);
            else if (response.StatusCode != HttpStatusCode.OK)
                MessageService.ShowError(Resources.ErrorMessage);
            else
            {
                TweetText = "";
                DataTransfer.Text = "";
                GoBack();
                DataTransfer.ReplyingDM = false;
            }
        }

        void Schedule(object param)
        {
            if (!CheckProtectedAccounts())
                return;

            TwitterStatusTask task = new TwitterStatusTask
            {
                InReplyTo = DataTransfer.ReplyId
            };

            task.Text = TweetText;

            if (ScheduledDate == null || ScheduledTime == null)
            {
                MessageService.ShowError(Resources.SelectDateTimeToSchedule);
                return;
            }

            task.Scheduled = new DateTime(
                ScheduledDate.Year,
                ScheduledDate.Month,
                ScheduledDate.Day,
                ScheduledTime.Hour,
                ScheduledTime.Minute,
                0);

            task.Accounts = new List<UserToken>();

            foreach (var user in SelectedAccounts.OfType<UserToken>())
                task.Accounts.Add(user);

            Config.TweetTasks.Add(task);
            Config.SaveTweetTasks();

            MessageService.ShowMessage(Resources.MessageScheduled, "");
            GoBack();
        }

        void StartImageChooser(object param)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.ShowCamera = true;
            chooser.Completed += new EventHandler<PhotoResult>(ChooserCompleted);
            chooser.Show();
        }

        void ChooserCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;

            IsLoading = true;
            BarText = Resources.UploadingPicture;

            TwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount) as TwitterService;

            if (srv == null)
                return; // Dirty trick: it will never be null if we're not testing.

            RestRequest req = srv.PrepareEchoRequest();
            RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "1" };

            req.AddFile("media", e.OriginalFileName, e.ChosenPhoto);
            req.AddField("key", "1abb1622666934158f4c2047f0822d0a");
            req.AddField("message", TweetText);
            req.AddField("consumer_token", Ocell.Library.SensitiveData.ConsumerToken);
            req.AddField("consumer_secret", SensitiveData.ConsumerSecret);
            req.AddField("oauth_token", DataTransfer.CurrentAccount.Key);
            req.AddField("oauth_secret", DataTransfer.CurrentAccount.Secret);
            req.Path = "upload.xml";
            //req.Method = Hammock.Web.WebMethod.Post;

            client.BeginRequest(req, (RestCallback)uploadCompleted);
        }

        void uploadCompleted(RestRequest request, RestResponse response, object userstate)
        {
            IsLoading = false;
            BarText = "";

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageService.ShowError(Resources.ErrorUploadingImage);
                return;
            }

            XDocument doc = XDocument.Parse(response.Content);
            XElement node = doc.Descendants("url").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(node.Value) || !node.Value.Contains("http://"))
            {
                MessageService.ShowError(Resources.ErrorUploadingImage);
                return;
            }

            TweetText += " " + node.Value + " ";
        }

        public void SaveAsDraft(object param)
        {
            TwitterDraft draft = CreateDraft();

            Config.Drafts.Add(draft);
            Config.Drafts = Config.Drafts;

            MessageService.ShowMessage(Resources.DraftSaved);
        }

        public TwitterDraft CreateDraft()
        {
            var draft = new TwitterDraft();
            draft.Text = TweetText;

            if (IsScheduled == true)
            {
                draft.Scheduled = new DateTime(
                ScheduledDate.Year,
                ScheduledDate.Month,
                ScheduledDate.Day,
                ScheduledTime.Hour,
                ScheduledTime.Minute,
                0);
            }
            else
                draft.Scheduled = null;

            draft.CreatedAt = DateTime.Now;
            draft.Accounts = new List<UserToken>();

            foreach (var acc in SelectedAccounts.OfType<UserToken>())
                draft.Accounts.Add(acc as UserToken);

            draft.ReplyId = DataTransfer.ReplyId;

            return draft;
        }

        bool CheckProtectedAccounts()
        {
            foreach (var user in SelectedAccounts.OfType<UserToken>())
            {
                if (user != null && ProtectedAccounts.IsProtected(user))
                {
                    var result = MessageService.AskYesNoQuestion(String.Format(Resources.AskTweetProtectedAccount, user.ScreenName), "");
                    if (!result)
                        return false;
                }
            }

            return true;
        }
    }
}
