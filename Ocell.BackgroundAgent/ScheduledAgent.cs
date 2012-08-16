using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Info;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using TweetSharp;
using Ocell.LightTwitterService;
using Ocell.Localization;


namespace Ocell.BackgroundAgent
{
    public class ScheduledAgent : ScheduledTaskAgent, IDisposable
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// Constructor de ScheduledAgent que inicializa el controlador UnhandledException
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Suscribirse al controlador de excepciones administradas
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }

            _threads = 0;
        }

        ~ScheduledAgent()
        {
            Dispose();
        }

        public void Dispose()
        {
            _agentWaitHandle.Dispose();
            _stepWaitHandle.Dispose();
            GC.SuppressFinalize(this);
        }

        /// Código para ejecutar en excepciones no controladas
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Se ha producido una excepción no controlada; interrumpir el depurador
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Threading management
        EventWaitHandle _agentWaitHandle = new AutoResetEvent(false);
        EventWaitHandle _stepWaitHandle = new AutoResetEvent(false);
        int _threads;

        private void SignalThreadStart()
        {
            Interlocked.Increment(ref _threads);
        }

        private void SignalThreadEnd()
        {
            Interlocked.Decrement(ref _threads);
            if (_threads <= 1)
                _stepWaitHandle.Set();
            if (_threads <= 0)
                _agentWaitHandle.Set();
        }

        private void WaitForTaskToEnd()
        {
            if (_threads > 1)
                _stepWaitHandle.WaitOne(5000);
        }

        bool IsMemoryUsageHigh()
        {
            double highPercentage = 0.97;
            double highMemory = DeviceStatus.ApplicationMemoryUsageLimit * highPercentage;
            return DeviceStatus.ApplicationCurrentMemoryUsage > highMemory;
        }

        void TerminateAll()
        {
            // Allow the scheduled agent to terminate without taking into account other threads.
            _threads = 0;
            _agentWaitHandle.Set();
        }
        #endregion

        void WriteMemUsage(string message)
        {
#if DEBUG
            long percentage;
            long used = DeviceStatus.ApplicationCurrentMemoryUsage / 1024;
            if (DeviceStatus.ApplicationMemoryUsageLimit != 0)
                percentage = DeviceStatus.ApplicationCurrentMemoryUsage * 100 / DeviceStatus.ApplicationMemoryUsageLimit;
            else
                percentage = 0;
            string toWrite = string.Format("{3}: {0} - {1} KB ({2}% of available memory)",
                message, used, percentage, DateTime.Now.ToString("HH:mm:ss.ff"));
            Debug.WriteLine(toWrite);
            DebugWriter.Add(toWrite);
#endif
        }

        /// <summary>
        /// Agente que ejecuta una tarea programada
        /// </summary>
        /// <param name="task">
        /// Tarea invocada
        /// </param>
        /// <remarks>
        /// Se llama a este método cuando se invoca una tarea periódica o con uso intensivo de recursos
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            DateTime start, end;
            start = DateTime.Now;
            SignalThreadStart();
#if DEBUG
            DebugWriter.Add("");
#endif
            try
            {
                DoWork();
            }
            catch (Exception)
            {
                TerminateAll();
            }

            SignalThreadEnd();

            end = DateTime.Now;
            int maxTimeout = 24000 - (int)((end - start).TotalMilliseconds);

            if (maxTimeout < 0)
                maxTimeout = 1000;

#if DEBUG
            if (Debugger.IsAttached)
                _agentWaitHandle.WaitOne();
            else
#endif
                _agentWaitHandle.WaitOne(maxTimeout);

            WriteMemUsage("Exit with " + _threads.ToString() + " running");
#if DEBUG
            DebugWriter.Save();
#endif
            NotifyComplete();
        }

        void CompleteAction(Action action)
        {
            WriteMemUsage("Start " + action.Method.Name);
            action.Invoke();
            WaitForTaskToEnd();
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

        void DoWork()
        {
            CompleteAction(SendScheduledTweets);
            CompleteAction(NotifyMentionsAndMessages);
            CompleteAction(UpdateTiles);
        }

        void SendScheduledTweets()
        {
            var copyList = new List<TwitterStatusTask>(Config.TweetTasks);
            foreach (TwitterStatusTask task in copyList)
            {
                Config.TweetTasks.Remove(task);
                var executor = new TaskExecutor(task);
                executor.Completed += (sender, e) => SignalThreadEnd();
                executor.Error += (sender, e) =>
                    {
                        Config.TweetTasks.Add(task);
                        Config.SaveTasks();
                        SignalThreadEnd();
                    };
                SignalThreadStart();
                executor.Execute();
            }
            Config.SaveTasks();
        }

        #region Notification
        int notifications;
        bool newMessages, newMentions;
        string from;
        List<string> usersWithNotifications;

        void CreateToast(string type, int count, string from, string to)
        {
            string toastContent = "";

            if (count == 1)
            {
                if (type == "mention")
                    toastContent = String.Format(Resources.NewMention, from);
                else
                    toastContent = String.Format(Resources.NewMessage, from);
            }
            else
            {
                if (type == "mention")
                    toastContent = String.Format(Resources.NewXMentions, count);
                else
                    toastContent = String.Format(Resources.NewXMessages, count);
            }

            ShellToast toast = new ShellToast();
            toast.NavigationUri = new Uri("/MainPage.xaml", UriKind.Relative);
            toast.Title = to;
            toast.Content = toastContent;

            toast.Show();
        }

        void BuildTile()
        {
            StandardTileData mainTile = new StandardTileData();
            if (notifications == 1)
            {
                if (newMentions)
                    mainTile.BackContent = (char)8203 +String.Format(Resources.NewMention, from);
                else
                    mainTile.BackContent = (char)8203 + String.Format(Resources.NewMessage, from);

                mainTile.BackTitle = usersWithNotifications[0];
            }
            else
            {
                string content = "";

                if (newMentions && newMessages)
                    content = Resources.NewMentionsMessages;
                else if (newMentions)
                    content = Resources.NewMentions;
                else if (newMessages)
                    content = Resources.NewMessages;

                mainTile.BackTitle = String.Format(Resources.ForX, GetChainOfNames(usersWithNotifications));
                mainTile.BackContent = content;
            }
            mainTile.Count = notifications;
            ShellTile.ActiveTiles.FirstOrDefault().Update(mainTile);
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

        void NotifyMentionsAndMessages()
        {
            usersWithNotifications = new List<string>();
            notifications = 0;
            from = "";
            newMessages = false;
            newMentions = false;

            foreach (var user in Config.Accounts)
            {
                CheckNotificationsForUser(user);
            }
        }

        private void CheckNotificationsForUser(UserToken user)
        {
            var service = new LightTwitterClient(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, user.Key, user.Secret);
            if (user.Preferences.MentionsPreferences == NotificationType.TileAndToast
                || user.Preferences.MentionsPreferences == NotificationType.Tile)
            {
                SignalThreadStart();

                service.ListMentions(10, (statuses, response) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    WriteMemUsage("Received mentions");
                    if (statuses == null || response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        WriteMemUsage("Mentions: exit with error " + response.StatusDescription);
                        SignalThreadEnd();
                        return;
                    }

                    var CheckDate = DateSync.GetLastCheckDate();
                    var ToastDate = DateSync.GetLastToastNotificationDate();

                    if (CheckDate > ToastDate)
                        ToastDate = CheckDate;

                    var newStatuses = statuses.Where(item =>
                    {
                        return TwitterObjectIsOlderThan(item, CheckDate);
                    });

                    var newToastStatuses = statuses.Where(item =>
                    {
                        return TwitterObjectIsOlderThan(item, ToastDate);
                    });

                    if (newStatuses.Count() > 0)
                    {
                        newMentions = true;

                        if (newStatuses.Count() == 1)
                        {
                            string userstring = "";
                            if (newStatuses.FirstOrDefault().TryGetProperty("user", out userstring))
                                new TwitterObject(userstring).TryGetProperty("screen_name", out from);
                            else
                                from = "no_name";
                        }

                        if (!usersWithNotifications.Contains(user.ScreenName))
                            usersWithNotifications.Add(user.ScreenName);

                        notifications += newStatuses.Count();

                        if (user.Preferences.MentionsPreferences == NotificationType.TileAndToast && newToastStatuses.Count() > 1)
                        {
                            string toastFrom, userstring = "";
                            if (newToastStatuses.FirstOrDefault().TryGetProperty("user", out userstring))
                                new TwitterObject(userstring).TryGetProperty("screen_name", out toastFrom);
                            else
                                toastFrom = "no_name";

                            CreateToast("mention", newToastStatuses.Count(), toastFrom, user.ScreenName);
                            DateSync.WriteLastToastNotificationDate(DateTime.Now.ToUniversalTime());
                        }

                        BuildTile();

                    }

                    WriteMemUsage("Mentions: Exit with " + newStatuses.Count().ToString() + " new statuses.");
                    SignalThreadEnd();
                });
            }
            if (user.Preferences.MessagesPreferences == NotificationType.TileAndToast
                || user.Preferences.MessagesPreferences == NotificationType.Tile)
            {
                SignalThreadStart();

                service.ListMessages(10, (statuses, response) =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    WriteMemUsage("Received messages");

                    if (statuses == null || response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        WriteMemUsage("Messages: exit with error " + response.StatusDescription);
                        SignalThreadEnd();
                        return;
                    }

                    var CheckDate = DateSync.GetLastCheckDate();
                    var ToastDate = DateSync.GetLastToastNotificationDate();

                    var newStatuses = statuses.Where(item =>
                    {
                        return TwitterObjectIsOlderThan(item, CheckDate);
                    });

                    var newToastStatuses = statuses.Where(item =>
                    {
                        return TwitterObjectIsOlderThan(item, ToastDate);
                    });

                    if (newStatuses.Count() > 0)
                    {
                        newMessages = true;

                        if (newStatuses.Count() == 1)
                            newStatuses.FirstOrDefault().TryGetProperty("sender_screen_name", out from);

                        if (!usersWithNotifications.Contains(user.ScreenName))
                            usersWithNotifications.Add(user.ScreenName);

                        notifications += newStatuses.Count();

                        if (user.Preferences.MessagesPreferences == NotificationType.TileAndToast && newToastStatuses.Count() > 0)
                        {
                            string toastFrom;
                            newStatuses.FirstOrDefault().TryGetProperty("sender_screen_name", out toastFrom);

                            CreateToast("message", newToastStatuses.Count(), toastFrom, user.ScreenName);
                            DateSync.WriteLastToastNotificationDate(DateTime.Now.ToUniversalTime());
                        }

                        BuildTile();

                    }

                    WriteMemUsage("Messages: Exit with " + newStatuses.Count().ToString() + " new statuses.");
                    SignalThreadEnd();
                });
            }
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
        #endregion

        #region Tile updating
        void UpdateTiles()
        {
            if (Config.BackgroundLoadColumns == true)
            {
                foreach (var column in FindColumnsToUpdate())
                    UpdateColumn(column);
            }
        }

        protected void UpdateColumn(TwitterResource Resource)
        {
            SignalThreadStart();
            Load(Resource);
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

        private string RemoveMention(string Tweet)
        {
            if (Tweet[0] == '@')
                Tweet = (char)8203 + Tweet;
            return Tweet;
        }

        protected void Load(TwitterResource resource)
        {
            var service = new TwitterService(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, resource.User.Key, resource.User.Secret);

            switch (resource.Type)
            {
                case ResourceType.Home:
                    service.ListTweetsOnHomeTimeline(1, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Mentions:
                    service.ListTweetsMentioningMe(1, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Messages:
                    service.ListDirectMessagesReceived(1, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Favorites:
                    service.ListFavoriteTweets((status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.List:
                    service.ListTweetsOnList(resource.Data.Substring(1, resource.Data.IndexOf('/') - 1),
                            resource.Data.Substring(resource.Data.IndexOf('/') + 1), 1, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Search:
                    service.Search(resource.Data, 1, 20, (status, response) => ReceiveTweetable(status.Statuses.Cast<ITweetable>(), response, resource));
                    break;
                case ResourceType.Tweets:
                    service.ListTweetsOnSpecifiedUserTimeline(resource.Data, 1, true, (status, response) => ReceiveTweetable(status.Cast<ITweetable>(), response, resource));
                    break;
            }
        }

        protected void ReceiveTweetable(IEnumerable<ITweetable> statuses, TwitterResponse response, TwitterResource Resource)
        {
            WriteMemUsage("Received tweet for column.");
            if (statuses == null || !statuses.Any() || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                WriteMemUsage("Exit with error");
                SignalThreadEnd();
                return;
            }

            string TileString = Uri.EscapeDataString(Resource.String);
            ITweetable Tweet = statuses.FirstOrDefault();
            ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(item => item.NavigationUri.ToString().Contains(TileString));

            if (Tile != null)
            {
                StandardTileData TileData = new StandardTileData
                {
                    BackContent = RemoveMention(Tweet.Text),
                    BackTitle = Tweet.Author.ScreenName
                };
                Tile.Update(TileData);
                WriteMemUsage("Updated tile.");
            }
            SignalThreadEnd();
        }
        #endregion
    }
}