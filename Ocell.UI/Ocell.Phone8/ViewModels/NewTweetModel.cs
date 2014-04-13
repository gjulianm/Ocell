using AncoraMVVM.Base;
using AncoraMVVM.Base.Diagnostics;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
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
    [ImplementPropertyChanged]
    public class NewTweetModel : ExtendedViewModelBase
    {
        #region Fields
        public IEnumerable<UserToken> AccountList { get; set; }

        public bool IsDM { get; set; }

        public string TweetText { get; set; }

        public int RemainingChars { get; set; }

        public string RemainingCharsStr { get; set; }

        public Brush RemainingCharsColor { get; set; }

        public bool UsesTwitlonger { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime ScheduledDate { get; set; }

        public DateTime ScheduledTime { get; set; }

        public bool SendingDM { get; set; }

        public IList SelectedAccounts { get; set; }

        public bool IsGeotagged { get; set; }

        public bool GeotagEnabled { get; set; }

        public bool IsSuggestingUsers { get; set; }

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

        public Autocompleter Completer { get; set; }

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
        {
            SelectedAccounts = new List<object>();
            AccountList = Config.Accounts.Value.ToList();
            IsGeotagged = Config.EnabledGeolocation.Value == true &&
                (Config.TweetGeotagging.Value == true || Config.TweetGeotagging.Value == null);
            GeotagEnabled = Config.EnabledGeolocation.Value == true;

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
                        Config.TweetGeotagging.Value = IsGeotagged;
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

            if (Config.EnabledGeolocation.Value == true)
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
            sendTweet = new DelegateCommand(Send, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !Progress.IsLoading);
            scheduleTweet = new DelegateCommand(Schedule, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !Progress.IsLoading);
            selectImage = new DelegateCommand(StartImageChooser, (param) => SelectedAccounts.Count > 0 && !Progress.IsLoading);
            saveDraft = new DelegateCommand(SaveAsDraft, (param) => !Progress.IsLoading);
            sendWithBuffer = new DelegateCommand(SendBufferUpdate, (param) => !Progress.IsLoading && SelectedAccounts.Count > 0);

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
            var result = Notificator.Prompt(Resources.NoBufferConfigured);

            if (result)
            {
                Ocell.Settings.OAuth.Type = Ocell.Settings.AuthType.Buffer;
                Navigator.Navigate(Uris.LoginPage);
            }
        }

        private void SendBufferUpdate(object param)
        {
            if (!TrialInformation.IsFullFeatured)
            {
                TrialInformation.ShowBuyDialog();
                return;
            }

            if (Config.BufferProfiles.Value.Count == 0)
            {
                AskBufferLogin();
                return;
            }

            List<string> profiles = new List<string>();

            foreach (var account in SelectedAccounts.Cast<UserToken>())
            {
                var profile = Config.BufferProfiles.Value.Where(x => x.ServiceUsername == account.ScreenName).FirstOrDefault();

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

            Progress.IsLoading = true;
            var response = await service.PostUpdate(TweetText, profiles);
            Progress.IsLoading = false;

            if (!response.Succeeded)
            {
                Notificator.ShowError(Resources.ErrorCreatingBuffer);
                return;
            }

            TweetText = "";
            DataTransfer.Text = "";
            Notificator.ShowMessage(Resources.BufferUpdateSent);
            Navigator.GoBack();
        }

        private void Send(object param)
        {
            if (!CheckProtectedAccounts())
                return;

            requestsLeft = 0;

            if (SelectedAccounts.Count == 0)
            {
                Notificator.ShowError(Resources.SelectAccount);
                return;
            }

            Progress.Text = Resources.SendingTweet;
            Progress.IsLoading = true;

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
                        Progress.IsLoading = false;
                        return;
                    }

                    Progress.Text = Resources.UploadingTwitlonger;
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
                if (Config.Drafts.Value.Contains(DataTransfer.Draft))
                    Config.Drafts.Value.Remove(DataTransfer.Draft);

                DataTransfer.Draft = null;
                Config.SaveDrafts();
            }
        }

        private async void SendDirectMessage()
        {
            var service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            var response = await service.SendDirectMessageAsync(new SendDirectMessageOptions { UserId = (int)DataTransfer.DMDestinationId, Text = TweetText });

            Progress.IsLoading = false;
            Progress.Text = "";

            if (response.StatusCode == HttpStatusCode.Forbidden)
                Notificator.ShowError(Resources.ErrorDuplicateTweet);
            else if (response.StatusCode != HttpStatusCode.OK)
                Notificator.ShowError(Resources.ErrorMessage);
            else
            {
                TweetText = "";
                DataTransfer.Text = "";
                Navigator.GoBack();
                DataTransfer.ReplyingDM = false;
            }
        }

        private bool EnsureTwitlonger()
        {
            return Notificator.Prompt(Resources.AskTwitlonger);
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
                Progress.IsLoading = false;
                Notificator.ShowError(Resources.ErrorCreatingTwitlonger);
                return;
            }

            var post = response.Content;

            Progress.Text = Resources.SendingTweet;

            try
            {
                lock (dicLock)
                    TwitlongerIds.Add(account.ScreenName, post.Post.Id);
            }
            catch (Exception e)
            {
                AncoraLogger.Instance.LogException("Unkown exception saving Twitlonger Ids", e);
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
                Progress.IsLoading = false;

            if (response == null)
                Notificator.ShowError(Resources.Error);
            else if (response.StatusCode == HttpStatusCode.Forbidden)
                Notificator.ShowError(Resources.ErrorDuplicateTweet);
            else if (!response.RequestSucceeded)
            {
                var errorMsg = response.Error != null ? response.Error.Message : "";
                Notificator.ShowError(String.Format("{0}: {1} ({2})", Resources.ErrorMessage, errorMsg, response.StatusCode));
            }
            else
            {
                TryAssociateWithTLId(status.Author.ScreenName, status.Id);
                if (requestsLeft <= 0)
                {
                    TweetText = "";
                    DataTransfer.Text = "";
                    Navigator.GoBack();
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
                ScheduleWithServer(ScheduledTime);
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
                Notificator.ShowError(Resources.SelectDateTimeToSchedule);
                return;
            }

            task.Scheduled = scheduleTime;

            task.Accounts = new List<UserToken>();

            foreach (var user in SelectedAccounts.OfType<UserToken>())
                task.Accounts.Add(user);

            Config.TweetTasks.Value.Add(task);
            Config.SaveTweetTasks();

            Notificator.ShowMessage(Resources.MessageScheduled);
            Navigator.GoBack();
        }

        private bool error;
        private void ScheduleWithServer(DateTime scheduleTime)
        {
            requestsLeft = 0;
            error = false;
            foreach (var user in SelectedAccounts.OfType<UserToken>())
            {
                Progress.IsLoading = true;
                requestsLeft++;

                var scheduler = new Scheduler(user.Key, user.Secret);

                scheduler.ScheduleTweet(TweetText, scheduleTime, (sender, response) =>
                {
                    requestsLeft--;
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                    {
                        error = true;
                        Notificator.ShowError(string.Format(Resources.ScheduleError, user.ScreenName));
                    }

                    if (requestsLeft <= 0)
                    {
                        Progress.IsLoading = false;
                        if (!error)
                        {
                            Notificator.ShowMessage(Resources.MessageScheduled);
                            Navigator.GoBack();
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

            Notificator.ShowError("Woops, not supported.");
        }

        public void SaveAsDraft(object param)
        {
            TwitterDraft draft = CreateDraft();

            Config.Drafts.Value.Add(draft);
            Config.Drafts.Value = Config.Drafts.Value;

            Notificator.ShowMessage(Resources.DraftSaved);
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
                    var result = Notificator.Prompt(String.Format(Resources.AskTweetProtectedAccount, user.ScreenName));
                    if (!result)
                        return false;
                }
            }

            return true;
        }

        #region User suggestions
        private void UpdateAutocompleter()
        {
            RaisePropertyChanged("Suggestions");
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