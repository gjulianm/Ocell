using Hammock;
using Ocell.Compatibility;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.LightTwitterService;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Phone.Shell;
using TweetSharp;

namespace Ocell.BackgroundAgent.Library
{
    public class BaseScheduledAgent
    {
        public Func<long> ApplicationMemoryLimit { get; set; }
        public Func<long> ApplicationMemoryUsage { get; set; }
        public TileManager TileManager { get; set; }

        public void Start()
        {
            CompleteAction(SendScheduledTweets);
            CompleteAction(NotifyMentionsAndMessages);
        }

        bool IsMemoryUsageHigh()
        {
            double highPercentage = 0.97;
            double highMemory = ApplicationMemoryLimit() * highPercentage;
            return ApplicationMemoryUsage() > highMemory;
        }

        [Conditional("DEBUG")]
        void WriteMemUsage(string message)
        {
            long percentage;
            long used = ApplicationMemoryUsage() / 1024;
            if (ApplicationMemoryLimit() != 0)
                percentage = ApplicationMemoryUsage() * 100 / ApplicationMemoryLimit();
            else
                percentage = 0;
            string toWrite = string.Format("{3}: {0} - {1} KB ({2}% of available memory)",
                message, used, percentage, DateTime.Now.ToString("HH:mm:ss.ff"));
            Debug.WriteLine(toWrite);
            Logger.Add(toWrite);
        }

        private void CompleteAction(Action action)
        {
            WriteMemUsage("Start " + action.Method.Name);
            action.Invoke();
            WriteMemUsage("End " + action.Method.Name);

            if (IsMemoryUsageHigh() && !System.Diagnostics.Debugger.IsAttached)
            {
                WriteMemUsage("High memory usage. Recovering memory...");
                Thread.Sleep(100);
                Config.ClearStaticValues();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                WriteMemUsage("Memory recovery completed");

                if (IsMemoryUsageHigh())
                {
                    WriteMemUsage("Not enough memory to continue. Terminating...");
                    throw new OutOfMemoryException("Not enough memory to continue with the ScheduledAgent");
                }
            }
        }

        private void SendScheduledTweets()
        {
            var copyList = new List<TwitterStatusTask>(Config.TweetTasks);
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            int tasks = copyList.Count;
            foreach (TwitterStatusTask task in copyList)
            {
                Config.TweetTasks.Remove(task);
                var executor = new TaskExecutor(task);
                executor.Completed += (sender, e) => waitHandle.Set();
                executor.Error += (sender, e) =>
                {
                    Config.TweetTasks.Add(task);
                    Config.SaveTweetTasks();
                    waitHandle.Set();
                };
                executor.Execute();

                waitHandle.WaitOne(TimeSpan.FromSeconds(1)); // Do work sequentially
            }
            Config.SaveTweetTasks();
        }

        #region Notifications
        private int requestsPending = 0;
        private AutoResetEvent notificationsWaitHandle = new AutoResetEvent(false);
        private List<TileNotification> tileNotifications = new List<TileNotification>();
        private List<TileNotification> toastNotifications = new List<TileNotification>();
        private object notsSync = new object();

        private void NotifyMentionsAndMessages()
        {
            foreach (var user in Config.Accounts)
                CheckNotificationsForUser(user);

            if (Interlocked.CompareExchange(ref requestsPending, 0, 0) != 0)
                notificationsWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

            NotifyToast();
            TileManager.SetNotifications(tileNotifications);
        }

        private bool TwitterObjectIsOlderThan(TwitterObject item, DateTime date)
        {
            string content;
            if (!item.TryGetProperty("created_at", out content))
                return false;

            DateTime objDate;
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            if (!DateTime.TryParseExact(content,
            format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out objDate))
                return false;

            var d = objDate.ToUniversalTime();

            return date < d;
        }

        private TileNotification TwitterObjectToNotification(TweetType type, string name, TwitterObject item)
        {
            var not = new TileNotification();

            string userstring = "";
            string from = "no_name";

            if (item.TryGetProperty("user", out userstring))
                new TwitterObject(userstring).TryGetProperty("screen_name", out from);
            else
                item.TryGetProperty("sender_screen_name", out from);

            not.From = from;
            not.Type = type;
            not.To = name;

            return not;
        }

        private NotificationType PreferencesForType(TweetType type, UserToken user)
        {
            if (type == TweetType.Mention)
                return user.Preferences.MentionsPreferences;
            else
                return user.Preferences.MessagesPreferences;
        }

        private void ReceiveTweetObjects(TweetType type, UserToken user, TwitterObjectCollection statuses, RestResponse response)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            WriteMemUsage("Received " + type.ToString());
            if (statuses == null || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                WriteMemUsage(type.ToString() + ": exit with error " + response.StatusDescription);
                return;
            }

            var CheckDate = DateSync.GetLastCheckDate();
            var ToastDate = DateSync.GetLastToastNotificationDate();

            if (CheckDate > ToastDate)
                ToastDate = CheckDate;

            var newStatuses = statuses.Where(item => TwitterObjectIsOlderThan(item, CheckDate));
            var newToastStatuses = statuses.Where(item => TwitterObjectIsOlderThan(item, ToastDate));

            var tileStatuses = newStatuses.Select(item => TwitterObjectToNotification(type, user.ScreenName, item));
            var toastStatuses = newToastStatuses.Select(item => TwitterObjectToNotification(type, user.ScreenName, item));

            var notPrefs = PreferencesForType(type, user);

            if (notPrefs == NotificationType.Tile || notPrefs == NotificationType.TileAndToast)
                lock (notsSync)
                    tileNotifications.AddRange(tileStatuses);

            if (notPrefs == NotificationType.Toast || notPrefs == NotificationType.TileAndToast)
                lock (notsSync)
                    toastNotifications.AddRange(toastStatuses);

        }

        private void CheckNotificationsForUser(UserToken user)
        {
            var service = new LightTwitterClient(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, user.Key, user.Secret);

            if (user.Preferences.MentionsPreferences != NotificationType.None)
            {
                Interlocked.Increment(ref requestsPending);
                service.ListMentions(10, (statuses, response) =>
                {
                    ReceiveTweetObjects(TweetType.Mention, user, statuses, response);
                    if (Interlocked.Decrement(ref requestsPending) == 0)
                        notificationsWaitHandle.Set();
                });
            }

            if (user.Preferences.MessagesPreferences != NotificationType.None)
            {
                Interlocked.Increment(ref requestsPending);
                service.ListMessages(10, (statuses, response) =>
                {
                    ReceiveTweetObjects(TweetType.Message, user, statuses, response);
                    if (Interlocked.Decrement(ref requestsPending) == 0)
                        notificationsWaitHandle.Set();
                });
            }
        }

        private void NotifyToast()
        {
            if(Config.PushEnabled == true || !toastNotifications.Any())
                return;

            string toastContent = "";

            if (toastNotifications.Count == 1)
            {
                var not = toastNotifications.First();
                if (not.Type == TweetType.Mention)
                    toastContent = String.Format(Resources.NewMention, not.From);
                else
                    toastContent = String.Format(Resources.NewMessage, not.From);
            }
            else
            {
                if (toastNotifications.Any(x => x.Type == TweetType.Mention) && toastNotifications.Any(x => x.Type == TweetType.Message))
                    toastContent = String.Format(Resources.NewXMentionsMessages, toastNotifications.Count);
                else if (toastNotifications.Any(x => x.Type == TweetType.Mention))
                    toastContent = String.Format(Resources.NewXMentions, toastNotifications.Count);
                else if (toastNotifications.Any(x => x.Type == TweetType.Message))
                    toastContent = String.Format(Resources.NewXMessages, toastNotifications.Count);
            }

            ShellToast toast = new ShellToast();
            toast.NavigationUri = new Uri("/MainPage.xaml", UriKind.Relative);
            toast.Title = GetChainOfNames(toastNotifications.Select(x => x.To).Distinct().ToList());
            toast.Content = toastContent;

            toast.Show();
        }

        string GetChainOfNames(List<string> names)
        {
            string content = "";
            if (names == null || !names.Any())
                return content;

            int i = 0;
            content += names[i];
            i++;

            for (; i < names.Count - 1; i++)
                content += ", " + names[i];

            if (i == names.Count - 1)
                content += " " + Resources.And + " " + names[i];

            return content;
        }
        #endregion

        #region Tile updating
        void UpdateTiles()
        {
            if (Config.BackgroundLoadColumns == true)
            {
                foreach (var column in FindColumnsToUpdate())
                    Load(column);
            }
        }

        private IEnumerable<TwitterResource> FindColumnsToUpdate()
        {
            string column;
            string url;
            int paramIndex;
            TwitterResource Resource;
            foreach (var tile in ShellTile.ActiveTiles)
            {
                url = tile.NavigationUri.ToString();
                paramIndex = url.IndexOf("column=");
                if (paramIndex != -1)
                {
                    column = Uri.UnescapeDataString(url.Substring(url.IndexOf("=") + 1));
                    Resource = Config.Columns.FirstOrDefault(item => item.String == column);
                    if (Resource != null && Resource.String == column)
                        yield return Resource;
                }
            }
        }

       

        protected void Load(TwitterResource resource)
        {
            var service = new TwitterService(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, resource.User.Key, resource.User.Secret);

            switch (resource.Type)
            {
                case ResourceType.Home:
                    service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions { Count = 1, IncludeEntities = true }, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Mentions:
                    service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions { Count = 1, IncludeEntities = true }, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Messages:
                    service.ListDirectMessagesReceived(new ListDirectMessagesReceivedOptions { Count = 1 }, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Favorites:
                    service.ListFavoriteTweets(new ListFavoriteTweetsOptions { Count = 1 }, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.List:
                    service.ListTweetsOnList(new ListTweetsOnListOptions
                    {
                        IncludeRts = false,
                        Count = 1,
                        OwnerScreenName = resource.Data.Substring(1, resource.Data.IndexOf('/') - 1),
                        Slug = resource.Data.Substring(resource.Data.IndexOf('/') + 1)
                    }, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Search:
                    service.Search(new SearchOptions { Count = 1, IncludeEntities = true, Q = resource.Data }, (status, response) => ReceiveTweetable(status.Statuses.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Tweets:
                    service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { Count = 1, ScreenName = resource.Data, IncludeRts = true }, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
            }
        }

        protected void ReceiveTweetable(IEnumerable<ITweetable> statuses, TwitterResponse response, TwitterResource Resource)
        {
            WriteMemUsage("Received tweet for column.");
            if (statuses == null || !statuses.Any() || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                WriteMemUsage("Exit with error");
                return;
            }

            string tileString = Uri.EscapeDataString(Resource.String);
            ITweetable tweet = statuses.FirstOrDefault();

            TileManager.SetColumnTweet(tileString, tweet.CleanText, tweet.AuthorName);            
        }
        #endregion
    }
}
