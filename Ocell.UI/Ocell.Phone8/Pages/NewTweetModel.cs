using DanielVaughan.Windows;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows.Input;
using System.Windows.Media;
using TweetSharp;

namespace Ocell.Pages
{
    public class NewTweetModel : ExtendedViewModelBase
    {
        #region Fields
        private IEnumerable<UserToken> accountList;
        public IEnumerable<UserToken> AccountList
        {
            get { return accountList; }
            set { Assign("AccountList", ref accountList, value); }
        }

        private bool isDM;
        public bool IsDM
        {
            get { return isDM; }
            set { Assign("IsDM", ref isDM, value); }
        }

        private string tweetText;
        public string TweetText
        {
            get { return tweetText; }
            set { Assign("TweetText", ref tweetText, value); }
        }

        private int remainingChars;
        public int RemainingChars
        {
            get { return remainingChars; }
            set { Assign("RemainingChars", ref remainingChars, value); }
        }

        private string remainingCharsStr;
        public string RemainingCharsStr
        {
            get { return remainingCharsStr; }
            set { Assign("RemainingCharsStr", ref remainingCharsStr, value); }
        }

        private Brush remainingCharsColor;
        public Brush RemainingCharsColor
        {
            get { return remainingCharsColor; }
            set { Assign("RemainingCharsColor", ref remainingCharsColor, value); }
        }

        private bool usesTwitlonger;
        public bool UsesTwitlonger
        {
            get { return usesTwitlonger; }
            set { Assign("UsesTwitlonger", ref usesTwitlonger, value); }
        }

        private bool isScheduled;
        public bool IsScheduled
        {
            get { return isScheduled; }
            set { Assign("IsScheduled", ref isScheduled, value); }
        }

        private DateTime scheduledDate;
        public DateTime ScheduledDate
        {
            get { return scheduledDate; }
            set { Assign("ScheduledDate", ref scheduledDate, value); }
        }

        private DateTime scheduledTime;
        public DateTime ScheduledTime
        {
            get { return scheduledTime; }
            set { Assign("ScheduledTime", ref scheduledTime, value); }
        }

        private bool sendingDM;
        public bool SendingDM
        {
            get { return sendingDM; }
            set { Assign("SendingDM", ref sendingDM, value); }
        }

        private IList selectedAccounts;
        public IList SelectedAccounts
        {
            get { return selectedAccounts; }
            set { Assign("SelectedAccounts", ref selectedAccounts, value); }
        }

        private bool isGeotagged;
        public bool IsGeotagged
        {
            get { return isGeotagged; }
            set { Assign("IsGeotagged", ref isGeotagged, value); }
        }

        private bool geotagEnabled;
        public bool GeotagEnabled
        {
            get { return geotagEnabled; }
            set { Assign("GeotagEnabled", ref geotagEnabled, value); }
        }

        private bool isSuggestingUsers;
        public bool IsSuggestingUsers
        {
            get { return isSuggestingUsers; }
            set { Assign("IsSuggestingUsers", ref isSuggestingUsers, value); }
        }

        public SafeObservable<string> Suggestions
        {
            get
            {
                if (Completer != null)
                    return Completer.Suggestions;
                else
                    return new SafeObservable<string>();
            }
        }

        private Autocompleter completer;
        public Autocompleter Completer
        {
            get { return completer; }
            set { Assign("Completer", ref completer, value); }
        }

        #endregion Fields

        #region Commands
        private DelegateCommand sendTweet;
        public ICommand SendTweet
        {
            get { return sendTweet; }
        }

        private DelegateCommand scheduleTweet;
        public ICommand ScheduleTweet
        {
            get { return scheduleTweet; }
        }

        private DelegateCommand saveDraft;
        public ICommand SaveDraft
        {
            get { return saveDraft; }
        }

        private DelegateCommand selectImage;
        public ICommand SelectImage
        {
            get { return selectImage; }
        }

        private DelegateCommand sendWithBuffer;
        public ICommand SendWithBuffer
        {
            get { return sendWithBuffer; }
        }

        #endregion Commands

        private GeoCoordinateWatcher geoWatcher = new GeoCoordinateWatcher();
        private int requestsLeft;

        public NewTweetModel()
            : base("NewTweet")
        {
            SelectedAccounts = new List<object>();
            AccountList = Config.Accounts.ToList();
            IsGeotagged = Config.EnabledGeolocation == true &&
                (Config.TweetGeotagging == true || Config.TweetGeotagging == null);
            GeotagEnabled = Config.EnabledGeolocation == true;

            SetupCommands();

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
                        SetRemainingChars();
                        break;

                    case "UsesTwitlonger":
                        RaiseExecuteChanged();
                        break;

                    case "IsGeotagged":
                        Config.TweetGeotagging = IsGeotagged;
                        break;

                    case "Completer":
                        UpdateAutocompleter();
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
        }

        private const int ShortUrlLength = 20;
        private Brush redBrush = new SolidColorBrush(Colors.Red);
        private void SetRemainingChars()
        {
            var txtLen = TweetText == null ? 0 : TweetText.Length;

            foreach (var url in GetUrls(TweetText))
                if (url.Length > ShortUrlLength)
                    txtLen -= url.Length - ShortUrlLength;

            RemainingChars = 140 - txtLen;

            if (RemainingChars >= 0)
            {
                RemainingCharsStr = RemainingChars.ToString();
                RemainingCharsColor = App.Current.Resources["PhoneSubtleBrush"] as Brush;
                UsesTwitlonger = false;
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

        private bool commandsSet = false;
        private void SetupCommands()
        {
            sendTweet = new DelegateCommand(Send, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !IsLoading);
            scheduleTweet = new DelegateCommand(Schedule, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !IsLoading);
            selectImage = new DelegateCommand(StartImageChooser, (param) => SelectedAccounts.Count > 0 && !IsLoading);
            saveDraft = new DelegateCommand(SaveAsDraft, (param) => !IsLoading);
            sendWithBuffer = new DelegateCommand(SendBufferUpdate, (param) => !IsLoading && SelectedAccounts.Count > 0);

            commandsSet = true;
        }

        private void RaiseExecuteChanged()
        {
            if (!commandsSet)
                return;

            sendTweet.RaiseCanExecuteChanged();
            scheduleTweet.RaiseCanExecuteChanged();
            selectImage.RaiseCanExecuteChanged();
            saveDraft.RaiseCanExecuteChanged();
            sendWithBuffer.RaiseCanExecuteChanged();
        }

        private void AskBufferLogin()
        {
            var result = MessageService.AskYesNoQuestion(Resources.NoBufferConfigured);

            if (result)
            {
                Ocell.Settings.OAuth.Type = Ocell.Settings.AuthType.Buffer;
                Navigate(Uris.LoginPage);
            }
        }

        private void SendBufferUpdate(object param)
        {
            if (!TrialInformation.IsFullFeatured)
            {
                TrialInformation.ShowBuyDialog();
                return;
            }

            if (Config.BufferProfiles.Count == 0)
            {
                AskBufferLogin();
                return;
            }

            List<string> profiles = new List<string>();

            foreach (var account in SelectedAccounts.Cast<UserToken>())
            {
                var profile = Config.BufferProfiles.Where(x => x.ServiceUsername == account.ScreenName).FirstOrDefault();

                if (profile != null)
                    profiles.Add(profile.Id);
            }

            SendBufferUpdate(profiles);
        }

        private async void SendBufferUpdate(List<string> profiles)
        {
            var service = ServiceDispatcher.GetBufferService();

            if (service == null)
                return;

            IsLoading = true;
            var response = await service.PostUpdate(TweetText, profiles);
            IsLoading = false;

            if (!response.Succeeded)
            {
                MessageService.ShowError(Resources.ErrorCreatingBuffer);
                return;
            }

            TweetText = "";
            DataTransfer.Text = "";
            MessageService.ShowMessage(Resources.BufferUpdateSent);
            GoBack();
        }

        private void Send(object param)
        {
            if (!CheckProtectedAccounts())
                return;

            requestsLeft = 0;

            if (SelectedAccounts.Count == 0)
            {
                MessageService.ShowError(Resources.SelectAccount);
                return;
            }

            BarText = Resources.SendingTweet;
            IsLoading = true;

            if (IsDM)
            {
                SendDirectMessage();
            }
            else
            {

                if (UsesTwitlonger)
                {
                    if (!EnsureTwitlonger())
                    {
                        IsLoading = false;
                        return;
                    }

                    BarText = Resources.UploadingTwitlonger;
                    foreach (UserToken account in SelectedAccounts.Cast<UserToken>())
                    {
                        SendTwitlongerPost(account);
                    }
                }
                else
                {
                    foreach (UserToken account in SelectedAccounts.Cast<UserToken>())
                        SendTweetToTwitter(TweetText, account);
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

        private async void SendDirectMessage()
        {
            var service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            var response = await service.SendDirectMessageAsync(new SendDirectMessageOptions { UserId = (int)DataTransfer.DMDestinationId, Text = TweetText });

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

        private bool EnsureTwitlonger()
        {
            return MessageService.AskOkCancelQuestion(Resources.AskTwitlonger);
        }

        private object dicLock = new object();
        private Dictionary<string, string> TwitlongerIds = new Dictionary<string, string>();

        private async void SendTwitlongerPost(UserToken account)
        {
            requestsLeft++;
            var response = await ServiceDispatcher.GetTwitlongerService(account).PostUpdate(TweetText);
            requestsLeft--;

            if (!response.Succeeded)
            {
                IsLoading = false;
                MessageService.ShowError(Resources.ErrorCreatingTwitlonger);
                return;
            }

            var post = response.Content;

            BarText = Resources.SendingTweet;

            try
            {
                lock (dicLock)
                    TwitlongerIds.Add(account.ScreenName, post.Post.Id);
            }
            catch
            {
                // TODO: Sometimes, this gives a weird OutOfRange exception. Don't know why, investigate it.
            }

            SendTweetToTwitter(post.Post.Content, account);
        }

        private async void SendTweetToTwitter(string post, UserToken account)
        {
            double? latitude = null, longitude = null;

            if (IsGeotagged)
            {
                GeoCoordinate location = IsGeotagged ? geoWatcher.Position.Location : null;
                latitude = location.Latitude;
                longitude = location.Longitude;
            }

            var sendOptions = new SendTweetOptions
            {
                Status = post,
                InReplyToStatusId = DataTransfer.ReplyId,
                Lat = latitude,
                Long = longitude
            };

            requestsLeft++;

            var response = await ServiceDispatcher.GetService(account).SendTweetAsync(sendOptions);
            var status = response.Content;

            requestsLeft--;

            if (requestsLeft <= 0)
                IsLoading = false;

            if (response == null)
                MessageService.ShowError(Resources.Error);
            else if (response.StatusCode == HttpStatusCode.Forbidden)
                MessageService.ShowError(Resources.ErrorDuplicateTweet);
            else if (!response.RequestSucceeded)
            {
                var errorMsg = response.Error != null ? response.Error.Message : "";
                MessageService.ShowError(String.Format("{0}: {1} ({2})", Resources.ErrorMessage, errorMsg, response.StatusCode));
            }
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

        private void TryAssociateWithTLId(string name, long tweetId)
        {
            if (!UsesTwitlonger)
                return;

            string id = null;
            lock (dicLock)
                TwitlongerIds.TryGetValue(name, out id);

            if (id != null)
                ServiceDispatcher.GetTwitlongerService(name).SetId(id, tweetId);
        }

        private void Schedule(object param)
        {
            if (!CheckProtectedAccounts())
                return;

            var scheduleTime = new DateTime(
                ScheduledDate.Year,
                ScheduledDate.Month,
                ScheduledDate.Day,
                ScheduledTime.Hour,
                ScheduledTime.Minute,
                0);

            if (TrialInformation.IsFullFeatured)
                ScheduleWithServer(scheduledTime);
            else
                ScheduleWithBackgroundAgent(scheduleTime);
        }

        private void ScheduleWithBackgroundAgent(DateTime scheduleTime)
        {
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

            task.Scheduled = scheduleTime;

            task.Accounts = new List<UserToken>();

            foreach (var user in SelectedAccounts.OfType<UserToken>())
                task.Accounts.Add(user);

            Config.TweetTasks.Add(task);
            Config.SaveTweetTasks();

            MessageService.ShowMessage(Resources.MessageScheduled, "");
            GoBack();
        }

        private bool error;
        private void ScheduleWithServer(DateTime scheduleTime)
        {
            requestsLeft = 0;
            error = false;
            foreach (var user in SelectedAccounts.OfType<UserToken>())
            {
                IsLoading = true;
                requestsLeft++;

                var scheduler = new Scheduler(user.Key, user.Secret);

                scheduler.ScheduleTweet(TweetText, scheduleTime, (sender, response) =>
                {
                    requestsLeft--;
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                    {
                        error = true;
                        MessageService.ShowError(string.Format(Resources.ScheduleError, user.ScreenName));
                    }

                    if (requestsLeft <= 0)
                    {
                        IsLoading = false;
                        if (!error)
                        {
                            MessageService.ShowMessage(Resources.MessageScheduled);
                            GoBack();
                        }
                    }
                });
            }
        }

        private void StartImageChooser(object param)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.ShowCamera = true;
            chooser.Completed += new EventHandler<PhotoResult>(ChooserCompleted);
            chooser.Show();
        }

        private void ChooserCompleted(object sender, PhotoResult e)
        {
            // TODO: Complete.

            MessageService.ShowError("Woops, not supported.");
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

        private IEnumerable<string> GetUrls(string text)
        {
            if (text == null)
                yield break;

            foreach (var word in text.Split(' '))
                if (Uri.IsWellFormedUriString(word, UriKind.Absolute))
                    yield return word;
        }

        private bool CheckProtectedAccounts()
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

        #region User suggestions
        private void UpdateAutocompleter()
        {
            OnPropertyChanged("Suggestions");
            if (Completer != null)
            {
                Completer.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsAutocompleting")
                        IsSuggestingUsers = Completer.IsAutocompleting;
                };
            }
        }

        #endregion User suggestions
    }
}