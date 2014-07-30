using AncoraMVVM.Base;
using AncoraMVVM.Base.Diagnostics;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TweetSharp;

namespace Ocell.Pages
{
    public enum TweetType { Tweet, DirectMessage };

    public class NewTweetArgs
    {
        public string Text { get; set; }
        public TweetType Type { get; set; }
        public long? ReplyToId { get; set; }
    }

    public class TweetImage
    {
        public TweetImage(string filename, Stream file)
        {
            this.Name = filename.Split('\\').Last();
            this.Filename = filename;
            this.File = file;
        }

        public string Name { get; set; }
        public string Filename { get; set; }
        public Stream File { get; set; }
    }

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
        public SafeObservable<UserToken> SelectedAccounts { get; set; }
        public bool IsGeotagged { get; set; }
        public bool GeotagEnabled { get; set; }
        public bool IsSuggestingUsers { get; set; }
        public Autocompleter Completer { get; set; }
        public int TextboxSelectionStart { get; set; }
        public SafeObservable<TweetImage> Images { get; set; } // I don't want to create a SafeObservableDictionary.
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

        public DelegateCommand RemoveImage { get; set; }

        #endregion Commands

        private GeoCoordinateWatcher geoWatcher = new GeoCoordinateWatcher();
        private NewTweetArgs args;
        private const int ShortUrlLength = 20;
        private Brush redBrush = new SolidColorBrush(Colors.Red);
        private static string savedText = "";
        private static IEnumerable<TweetImage> savedImages = null;
        private TwitterDraft draft;

        #region Constructor and event managers
        public NewTweetModel()
        {
            SelectedAccounts = new SafeObservable<UserToken>();
            Images = new SafeObservable<TweetImage>();
            AccountList = Config.Accounts.Value.ToList();
            IsGeotagged = Config.EnabledGeolocation.Value == true &&
                (Config.TweetGeotagging.Value == true || Config.TweetGeotagging.Value == null);
            GeotagEnabled = Config.EnabledGeolocation.Value == true;

            SetupCommands();

            args = ReceiveMessage<NewTweetArgs>() ?? new NewTweetArgs();

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

            IsDM = args.Type == TweetType.DirectMessage;

            // Avoid that ugly 01/01/0001 by default.
            var date = DateTime.Now.AddHours(1);
            ScheduledDate = date;
            ScheduledTime = date;

            draft = ReceiveMessage<TwitterDraft>();

            FillTweetText();

            if (draft != null)
            {
                foreach (var account in draft.Accounts)
                    SelectedAccounts.Add(account);
            }
            else
            {
                SelectedAccounts.Add(DataTransfer.CurrentAccount);
            }

            if (Config.EnabledGeolocation.Value == true)
                geoWatcher.Start();

            SetUpAutocompleter();
        }

        public override void OnLoad()
        {
            FillTweetText();

            if (savedImages != null)
                Images = new SafeObservable<TweetImage>(savedImages);

            base.OnLoad();
        }


        public override void OnNavigating(System.ComponentModel.CancelEventArgs e)
        {
            TrySaveDraft();

            if (Images != null && Images.Any())
                savedImages = Images;
            else
                savedImages = null;

            base.OnNavigating(e);
        }
        #endregion

        private void SetUpAutocompleter()
        {
            Completer = new Autocompleter(new UsernameProvider(Config.Accounts.Value));
            Completer.Trigger = '@';

            Completer.PropertyChanged += (snd, ev) =>
            {
                if (ev.PropertyName == "InputText" && Completer.InputText != TweetText)
                    TweetText = Completer.InputText;
                else if (ev.PropertyName == "SelectionStart" && Completer.SelectionStart != TextboxSelectionStart)
                    TextboxSelectionStart = Completer.SelectionStart;
            };

            this.PropertyChanged += (snd, ev) =>
            {
                if (ev.PropertyName == "TweetText")
                    Completer.TextChanged(TweetText, TextboxSelectionStart);
            };
        }

        private void FillTweetText()
        {
            if (draft != null)
                LoadDraft(draft);
            else if (args.Text != null)
                TweetText = args.Text;
            else
                TweetText = savedText;
        }

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

        private bool commandsSet = false;
        private void SetupCommands()
        {
            sendTweet = new DelegateCommand(Send, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !Progress.IsLoading);
            scheduleTweet = new DelegateCommand(Schedule, (param) => (RemainingChars >= 0 || UsesTwitlonger) && SelectedAccounts.Count > 0 && !Progress.IsLoading);
            selectImage = new DelegateCommand(StartImageChooser, (param) => !Progress.IsLoading);
            saveDraft = new DelegateCommand(SaveAsDraft, (param) => !Progress.IsLoading);
            sendWithBuffer = new DelegateCommand(SendBufferUpdate, (param) => !Progress.IsLoading && SelectedAccounts.Count > 0);
            RemoveImage = new DelegateCommand((param) =>
            {
                if (param is TweetImage)
                    Images.Remove(param as TweetImage);
            });

            sendTweet.BindCanExecuteToProperty(this, "RemainingChars", "UsesTwitlonger");
            sendTweet.BindCanExecuteToProperty(Progress, "IsLoading");
            scheduleTweet.BindCanExecuteToProperty(this, "RemainingChars", "UsesTwitlonger");
            scheduleTweet.BindCanExecuteToProperty(Progress, "IsLoading");
            sendWithBuffer.BindCanExecuteToProperty(Progress, "IsLoading");

            SelectedAccounts.CollectionChanged += (sender, e) =>
            {
                sendTweet.RaiseCanExecuteChanged();
                scheduleTweet.RaiseCanExecuteChanged();
                sendWithBuffer.RaiseCanExecuteChanged();
            };

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

        #region Buffer
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
            Notificator.ShowMessage(Resources.BufferUpdateSent);
            Navigator.GoBack();
        }
        #endregion Buffer

        #region Twitlonger
        private bool EnsureTwitlonger()
        {
            return Notificator.Prompt(Resources.AskTwitlonger);
        }

        private object dicLock = new object();
        private Dictionary<string, string> TwitlongerIds = new Dictionary<string, string>();

        private async Task<bool> SendTwitlongerPost(UserToken account)
        {
            Progress.Loading(Resources.SendingToTwitlonger);
            var response = await ServiceDispatcher.GetTwitlongerService(account).PostUpdate(TweetText);
            Progress.Finished();

            if (!response.Succeeded)
            {
                Notificator.ShowError(Resources.ErrorCreatingTwitlonger);
                return false;
            }

            var post = response.Content;

            try
            {
                lock (dicLock)
                    TwitlongerIds.Add(account.ScreenName, post.Post.Id);
            }
            catch (Exception e)
            {
                AncoraLogger.Instance.LogException("Unkown exception saving Twitlonger Ids", e);
            }

            return await SendTweetToTwitter(post.Post.Content, account);
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

        #endregion

        #region Tweet sending
        private async void Send(object param)
        {
            if (!CheckProtectedAccounts())
                return;

            if (SelectedAccounts.Count == 0)
            {
                Notificator.ShowError(Resources.SelectAccount);
                return;
            }

            var accounts = SelectedAccounts.Cast<UserToken>();
            IEnumerable<Task<bool>> tasks;

            if (IsDM)
            {
                tasks = new List<Task<bool>> { SendDirectMessage() };
            }
            else if (UsesTwitlonger)
            {
                if (!EnsureTwitlonger())
                    return;

                tasks = accounts.Select(a => SendTwitlongerPost(a)).ToList();
            }
            else
            {
                tasks = accounts.Select(a => SendTweetToTwitter(TweetText, a)).ToList();
            }


            var success = (await TaskEx.WhenAll(tasks)).All(x => x);

            Progress.ClearIndicator();

            if (success)
            {
                TweetText = "";
                savedText = "";
                Navigator.GoBack();

                if (draft != null)
                {
                    if (Config.Drafts.Value.Contains(draft))
                        Config.Drafts.Value.Remove(draft);

                    Config.SaveDrafts();
                }
            }
        }

        private async Task<bool> SendDirectMessage()
        {
            Progress.Loading(Resources.SendingTweet);

            var service = ServiceDispatcher.GetService(DataTransfer.CurrentAccount);
            var response = await service.SendDirectMessageAsync(new SendDirectMessageOptions { UserId = (int)args.ReplyToId, Text = TweetText });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                    Notificator.ShowError(Resources.ErrorDuplicateTweet);
                else
                    Notificator.ShowError(Resources.ErrorMessage);

                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task<bool> SendTweetToTwitter(string post, UserToken account)
        {
            double? latitude = null, longitude = null;

            if (IsGeotagged && !geoWatcher.Position.Location.IsUnknown)
            {
                GeoCoordinate location = geoWatcher.Position.Location;
                latitude = location.Latitude;
                longitude = location.Longitude;
            }

            var sendOptions = new SendTweetOptions
            {
                Status = post,
                InReplyToStatusId = args.ReplyToId,
                Lat = latitude,
                Long = longitude
            };

            var mediaOptions = new SendTweetWithMediaOptions
            {
                Status = post,
                InReplyToStatusId = args.ReplyToId,
                Lat = latitude,
                Long = longitude,
                Images = Images.ToDictionary(x => x.Name, x => x.File)
            };

            Progress.Loading(Resources.SendingTweet);

            TwitterResponse<TwitterStatus> response;
            if (Images.Any())
                response = await ServiceDispatcher.GetService(account).SendTweetWithMediaAsync(mediaOptions);
            else
                response = await ServiceDispatcher.GetService(account).SendTweetAsync(sendOptions);

            var status = response.Content;

            if (!response.RequestSucceeded)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                    Notificator.ShowError(Resources.ErrorDuplicateTweet);
                else
                {
                    var errorMsg = response.Error != null ? response.Error.Message : "";
                    Notificator.ShowError(String.Format("{0}: {1} ({2})", Resources.ErrorMessage, errorMsg, response.StatusCode));
                }

                return false;
            }
            else
            {
                TryAssociateWithTLId(status.Author.ScreenName, status.Id);

                return true;
            }
        }
        #endregion Tweet sending

        #region Tweet scheduling
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
                InReplyTo = args.ReplyToId ?? 0
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
            foreach (var user in SelectedAccounts.OfType<UserToken>())
            {
                Progress.IsLoading = true;

                var scheduler = new Scheduler(user.Key, user.Secret);

                scheduler.ScheduleTweet(TweetText, scheduleTime, (sender, response) =>
                {
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                    {
                        error = true;
                        Notificator.ShowError(string.Format(Resources.ScheduleError, user.ScreenName));
                    }

                    // TODO: To task.
                    Progress.IsLoading = false;
                    if (!error)
                    {
                        Notificator.ShowMessage(Resources.MessageScheduled);
                        Navigator.GoBack();
                    }
                });
            }
        }
        #endregion

        #region Pictures
        private void StartImageChooser(object param)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.ShowCamera = true;
            chooser.Completed += ChooserCompleted;
            chooser.Show();
        }

        private async void ChooserCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK || string.IsNullOrWhiteSpace(e.OriginalFileName))
                return;

            var name = e.OriginalFileName.Split('\\').Last();
            string filePath = "";
            string img_dir = "img_cache";

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (!store.DirectoryExists(img_dir))
                        store.CreateDirectory(img_dir);

                    using (var file = store.OpenFile(img_dir + "\\" + name, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite))
                    {
                        await e.ChosenPhoto.CopyToAsync(file);
                        filePath = file.Name;
                    }
                }
                catch (Exception ex)
                {
                    AncoraLogger.Instance.LogException("Error copying image", ex);
                    Notificator.ShowError(Resources.ErrorUploadingImage);
                    return;
                }
            }

            if (Images.Any(x => x.Filename == filePath) || string.IsNullOrEmpty(filePath))
            {
                AncoraLogger.Instance.LogEvent("Repeated image");
                Notificator.ShowError(Resources.ImageAlreadySelected);
                return;
            }

            Images.Add(new TweetImage(filePath, e.ChosenPhoto));

            Notificator.ShowMessage(Resources.ImageUploadOnSend);
        }
        #endregion Pictures

        #region Drafts
        private void TrySaveDraft()
        {
            savedText = TweetText;

            if (draft != null)
            {
                draft.Text = TweetText;
                Config.SaveDrafts();
            }
        }

        private void LoadDraft(TwitterDraft draft)
        {
            TweetText = draft.Text;

            if (draft.Scheduled != null)
            {
                IsScheduled = true;
                ScheduledTime = draft.Scheduled.GetValueOrDefault();
                ScheduledDate = draft.Scheduled.GetValueOrDefault();
            }

            foreach (var account in draft.Accounts.Where(x => x != null))
                SelectedAccounts.Add(account);
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

            draft.ReplyId = args.ReplyToId;

            return draft;
        }
        #endregion Drafts

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
    }
}