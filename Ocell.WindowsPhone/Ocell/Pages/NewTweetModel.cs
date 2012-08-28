using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Threading;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using System.Linq;
using System.Collections;
using Ocell.Localization;
using System.Net;
using Ocell.Library.Twitter;
using Ocell.Library.Tasks;
using Microsoft.Phone.Tasks;
using Hammock;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using TweetSharp;
using System.Xml;
using System.Windows.Data;
using System.Xml.Linq;

namespace Ocell.Pages
{
    public class NewTweetModel : ExtendedViewModelBase
    {
        bool uploadingPhoto;

        #region Fields
        IEnumerable<UserToken> accountList;
        public IEnumerable<UserToken> AccountList
        {
            get { return accountList; }
            set { Assign("AccountList", ref accountList, value); }
        }

        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        string barText;
        public string BarText
        {
            get { return barText; }
            set { Assign("BarText", ref barText, value); }
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

        public NewTweetModel()
            : base("NewTweet")
        {
            SelectedAccounts = new List<object>();
            AccountList = Config.Accounts.ToList();

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
                }
            };

            IsDM = DataTransfer.ReplyingDM;

            // Avoid that ugly 01/01/0001 by default.
            var date = DateTime.Now.AddHours(1);
            ScheduledDate = date;
            ScheduledTime = date;

            TryLoadDraft();

            SetupCommands();
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

            BarText = Resources.SendingTweet;
            IsLoading = true;

            if (IsDM)
            {
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).SendDirectMessage((int)DataTransfer.DMDestinationId, TweetText, ReceiveDM);
            }
            else
            {
                foreach (UserToken account in SelectedAccounts.Cast<UserToken>())
                    ServiceDispatcher.GetService(account).SendTweet(TweetText, DataTransfer.ReplyId, ReceiveResponse);
            }

            if (DataTransfer.Draft != null)
            {
                if (Config.Drafts.Contains(DataTransfer.Draft))
                    Config.Drafts.Remove(DataTransfer.Draft);

                DataTransfer.Draft = null;
                Config.SaveDrafts();
            }
        }

        void ReceiveResponse(TwitterStatus status, TwitterResponse response)
        {
            IsLoading = false;
            if (response.StatusCode == HttpStatusCode.Forbidden)
                MessageService.ShowError(Resources.ErrorDuplicateTweet);
            else if (response.StatusCode != HttpStatusCode.OK)
                MessageService.ShowError(Resources.ErrorMessage);
            else
            {
                TweetText = "";
                DataTransfer.Text = "";
                GoBack();
            }
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
            Config.SaveTasks();

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

            ITwitterService srv = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            RestRequest req = srv.PrepareEchoRequest();
            RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "1" };

            req.AddFile("media", e.OriginalFileName, e.ChosenPhoto);
            req.AddField("key", "1abb1622666934158f4c2047f0822d0a");
            req.AddField("message", TweetText);
            req.AddField("consumer_token", SensitiveData.ConsumerToken);
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
