using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Diagnostics;
using Microsoft.Phone.Info;


namespace ScheduledAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private List<TwitterStatus> TileNewMentions;
        private List<TwitterDirectMessage> TileNewMessages;
        private int PendingCalls;
        private bool execEnded;

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

            TileNewMentions = new List<TwitterStatus>();
            TileNewMessages = new List<TwitterDirectMessage>();
            PendingCalls = 0;
            execEnded = false;
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
            Debug.WriteLine("Memory at start: {0} KB", DeviceStatus.ApplicationCurrentMemoryUsage / (1024));
            
            foreach (UserToken User in Config.Accounts)
                GetMentionsFor(User);

            Debug.WriteLine("Memory when retrieved mentions: {0} KB", DeviceStatus.ApplicationCurrentMemoryUsage / (1024));

            Config.Dispose();

            foreach (TwitterStatusTask Task in Config.TweetTasks)
            {
                if (Task.Scheduled <= DateTime.Now)
                {
                    PendingCalls++;
                    Task.Completed += new EventHandler(OnTaskCompleted);
                    Task.Error += new EventHandler(Task_Error);
                    Task.Execute();
                }
            }

            Debug.WriteLine("Launched tweet tasks: {0} KB", DeviceStatus.ApplicationCurrentMemoryUsage / (1024));

            if (Config.BackgroundLoadColumns == true)
            {
                foreach (var column in FindColumnsToUpdate())
                    UpdateColumn(column);
            }

            Debug.WriteLine("Updated background columns: {0} KB", DeviceStatus.ApplicationCurrentMemoryUsage / (1024));

            Config.Dispose();

            if (PendingCalls == 0)
            {
                NotifyComplete();
            }
            else
            {
                while (!execEnded)
                {
                    Thread.Sleep(100);
                }
                NotifyComplete();
            }

            Debug.WriteLine("End: {0} KB", DeviceStatus.ApplicationCurrentMemoryUsage / (1024));
        }

        void Task_Error(object sender, EventArgs e)
        {
            PendingCalls--;
            if (PendingCalls <= 0)
                InternalNotifyComplete();
        }

        protected void OnTaskCompleted(object sender, EventArgs e)
        {
            PendingCalls--;
            Config.TweetTasks.Remove((TwitterStatusTask)sender);
            Config.SaveTasks();
            if (PendingCalls <= 0)
                InternalNotifyComplete();
        }

        protected void GetMentionsFor(UserToken User)
        {
            ITwitterService Service = ServiceDispatcher.GetService(User);

            if (User.Preferences.MentionsPreferences == NotificationType.Tile)
            {
                PendingCalls++;
                Service.ListTweetsMentioningMe(10, ReceiveTweetsToTile);
            }
            else if (User.Preferences.MentionsPreferences == NotificationType.TileAndToast)
            {
                PendingCalls++;
                Service.ListTweetsMentioningMe(10, ReceiveTweetsToToast);
            }

            if (User.Preferences.MessagesPreferences == NotificationType.Tile)
            {
                PendingCalls++;
                Service.ListDirectMessagesReceived(10, ReceiveMessagesToTile);
            }
            else if (User.Preferences.MentionsPreferences == NotificationType.TileAndToast)
            {
                PendingCalls++;
                Service.ListDirectMessagesReceived(10, ReceiveMessagesToToast);
            }

        }

        protected void ReceiveTweetsToTile(IEnumerable<TwitterStatus> Statuses, TwitterResponse Response)
        {
            if (Response.StatusCode != System.Net.HttpStatusCode.OK || Statuses == null)
            {
                PendingCalls--;
                ProcessNotifications();
                return;
            }

            DateTime LastChecked = DateSync.GetLastCheckDate();

            foreach (TwitterStatus status in Statuses)
            {
                if (status.CreatedDate > LastChecked)
                    TileNewMentions.Add(status);
            }
            PendingCalls--;
            ProcessNotifications();
        }

        protected void ReceiveTweetsToToast(IEnumerable<TwitterStatus> Statuses, TwitterResponse Response)
        {
            if (Statuses != null && Statuses.Count() > 0)
            {
                DateTime LastChecked = DateSync.GetLastCheckDate();
                int newMentions = 0;

                foreach (TwitterStatus status in Statuses)
                {
                    if (status.CreatedDate > LastChecked)
                        newMentions++;
                }

                ShellToast Toast = new ShellToast();
                Toast.Title = "Ocell";
                Toast.NavigationUri = new Uri("/MainPage.xaml", UriKind.Relative);

                if (newMentions > 0)
                {
                    string UserName = Statuses.First().InReplyToScreenName;

                    if (newMentions == 1)
                        Toast.Content = "@" + Statuses.First().Author.ScreenName + " mentioned you (@" + UserName + ")";
                    else
                        Toast.Content = "@" + UserName + " has " + newMentions + " new mentions";

                    Toast.Show();
                }
            }

            ReceiveTweetsToTile(Statuses, Response);
        }

        protected void ReceiveMessagesToTile(IEnumerable<TwitterDirectMessage> Messages, TwitterResponse Response)
        {

            if (Response.StatusCode != System.Net.HttpStatusCode.OK || Messages == null)
            {
                PendingCalls--;
                ProcessNotifications();
                return;
            }

            DateTime LastChecked = DateSync.GetLastCheckDate();

            foreach (TwitterDirectMessage status in Messages)
            {
                if (status.CreatedDate > LastChecked)
                    TileNewMessages.Add(status);
            }
            PendingCalls--;
            ProcessNotifications();
        }

        protected void ReceiveMessagesToToast(IEnumerable<TwitterDirectMessage> Messages, TwitterResponse Response)
        {
            if (Messages != null && Messages.Count() > 0)
            {
                DateTime LastChecked = DateSync.GetLastCheckDate();

                int newMessages = 0;

                foreach (TwitterDirectMessage status in Messages)
                {
                    if (status.CreatedDate > LastChecked)
                        newMessages++;
                }


                ShellToast Toast = new ShellToast();
                Toast.Title = "Ocell";
                Toast.NavigationUri = new Uri("/MainPage.xaml", UriKind.Relative);

                if (newMessages > 0)
                {
                    string UserName = Messages.First().RecipientScreenName;

                    if (newMessages == 1)
                        Toast.Content = "@" + Messages.First().Author.ScreenName + " sent a DM to @" + UserName;
                    else
                        Toast.Content = "@" + UserName + " has " + newMessages + " new messages";

                    Toast.Show();
                }
            }
            ReceiveMessagesToTile(Messages, Response);
        }

        protected void ProcessNotifications()
        {
            if (PendingCalls > 0)
                return;

            TileManager.UpdateTile(TileNewMentions, TileNewMessages);

            InternalNotifyComplete();
        }

        protected void InternalNotifyComplete()
        {
            execEnded = true;
        }

        protected void UpdateColumn(TwitterResource Resource)
        {
            PendingCalls++;
            TweetLoader Loader = new TweetLoader(Resource, false);
            Loader.Cached = false;
            Loader.TweetsToLoadPerRequest = 1;
            Loader.LoadFinished += new EventHandler(Loader_LoadFinished);
            Loader.Error += new TweetLoader.OnError(Loader_Error);
            Loader.Load();
        }

        void Loader_Error(TwitterResponse response)
        {
            ServiceDispatcher.Dispose();
            PendingCalls--;
            if (PendingCalls <= 0)
                InternalNotifyComplete();
        }

        void Loader_LoadFinished(object sender, EventArgs e)
        {
            TweetLoader Loader = sender as TweetLoader;
            if (Loader != null && Loader.Source.Count > 0)
            {
                string TileString = Uri.EscapeDataString(Loader.Resource.String);
                ITweetable Tweet = Loader.Source[0];
                ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(item => item.NavigationUri.ToString().Contains(TileString));

                Loader.Dispose();

                if (Tile != null)
                {
                    StandardTileData TileData = new StandardTileData
                    {
                        BackContent = RemoveMention(Tweet.Text),
                        BackTitle = Tweet.Author.ScreenName
                    };
                    Tile.Update(TileData);
                }
            }
            PendingCalls--;
            if (PendingCalls <= 0)
                InternalNotifyComplete();
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
                Tweet = Tweet.Remove(0, Tweet.IndexOf(' ') + 1);
            return Tweet;
        }
    }
}