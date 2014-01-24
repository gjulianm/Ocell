using Microsoft.Phone.Shell;
using Ocell.Compatibility;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ocell.BackgroundAgent.Library
{
    public class BaseScheduledAgent
    {
        public Func<long> ApplicationMemoryLimit { get; set; }
        public Func<long> ApplicationMemoryUsage { get; set; }
        public TileManager TileManager { get; set; }

        public async void Start()
        {
            await CompleteAction(SendScheduledTweets);
            await CompleteAction(NotifyMentionsAndMessages);
            FileAbstractor.WriteLinesToFile(Logger.LogHistory, "BA_DEBUG");
        }

        private bool IsMemoryUsageHigh()
        {
            double highPercentage = 0.97;
            double highMemory = ApplicationMemoryLimit() * highPercentage;
            return ApplicationMemoryUsage() > highMemory;
        }

        [Conditional("DEBUG")]
        private void WriteMemUsage(string message)
        {
            long percentage;
            long used = ApplicationMemoryUsage() / 1024;
            if (ApplicationMemoryLimit() != 0)
                percentage = ApplicationMemoryUsage() * 100 / ApplicationMemoryLimit();
            else
                percentage = 0;
            string toWrite = string.Format("{3}: {0} - {1} KB ({2}% of available memory)",
                message, used, percentage, DateTime.Now.ToString("HH:mm:ss.ff"));

            Logger.Trace(toWrite);
        }

        private async Task CompleteAction(Func<Task> action)
        {
            WriteMemUsage("Start " + action.Method.Name);
            await action.Invoke();
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

        private async Task SendScheduledTweets()
        {
            var copyList = new List<TwitterStatusTask>(Config.TweetTasks);
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            int tasks = copyList.Count;
            foreach (TwitterStatusTask task in copyList)
            {
                Config.TweetTasks.Remove(task);
                var executor = new TaskExecutor(task);
                executor.Error += (sender, e) =>
                {
                    Config.TweetTasks.Add(task);
                    Config.SaveTweetTasks();
                };

                await executor.Execute();
            }
            Config.SaveTweetTasks();
        }

        #region Notifications
        private int requestsPending = 0;
        private AutoResetEvent notificationsWaitHandle = new AutoResetEvent(false);
        private List<TileNotification> tileNotifications = new List<TileNotification>();
        private List<TileNotification> toastNotifications = new List<TileNotification>();
        private object notsSync = new object();

        private async Task NotifyMentionsAndMessages()
        {
            foreach (var user in Config.Accounts)
                CheckNotificationsForUser(user);

            if (Interlocked.CompareExchange(ref requestsPending, 0, 0) != 0)
                notificationsWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

            NotifyToast();
            TileManager.SetNotifications(tileNotifications);
        }

        private bool TwitterObjectIsOlderThan(/*TwitterObject item, */DateTime date)
        {
            /*string content;
            if (!item.TryGetProperty("created_at", out content))
                return false;

            DateTime objDate;
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            if (!DateTime.TryParseExact(content,
            format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out objDate))
                return false;

            var d = objDate.ToUniversalTime();

            return date < d;*/

            return true;
        }

        private TileNotification TwitterObjectToNotification(TweetType type, string name)
        {
            // TODO: Solve this
            /*
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
            not.Message = item.GetProperty("text");

            return not;*/

            throw new NotImplementedException();
        }

        private NotificationType PreferencesForType(TweetType type, UserToken user)
        {
            if (type == TweetType.Mention)
                return user.Preferences.MentionsPreferences;
            else
                return user.Preferences.MessagesPreferences;
        }

        private void ReceiveTweetObjects(TweetType type, UserToken user /*, TwitterObjectCollection statuses*/)
        {
            // TODO: Adapt this.

            /*GC.Collect();
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
                    toastNotifications.AddRange(toastStatuses);*/

        }

        private async Task CheckNotificationsForUser(UserToken user)
        {
            // TODO: Check if we can reuse Tweetsharp here again.

            /*var service = new LightTwitterClient(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, user.Key, user.Secret);

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
            }*/
        }

        private void NotifyToast()
        {
            if (Config.PushEnabled == true || !toastNotifications.Any())
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

        private string GetChainOfNames(List<string> names)
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

        #endregion Notifications

        #region Tile updating

        private void UpdateTiles()
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
            // TODO: Receive data for columns. Use TweetLoader? Refactor to reuse the functions?
        }

        #endregion Tile updating
    }
}