using System.Windows;
using Microsoft.Phone.Scheduler;
using TweetSharp;
using Ocell.Library;
using System.Collections.Generic;
using System;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Threading;

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

            LittleWatson.ReportException(e.ExceptionObject, "Ocell ScheduledAgent Error");
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
            foreach (UserToken User in Config.Accounts)
                GetMentionsFor(User);

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
        }

        protected void GetMentionsFor(UserToken User)
        {
            TwitterService Service = ServiceDispatcher.GetService(User);

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
            PendingCalls--;
            if (Response.StatusCode != System.Net.HttpStatusCode.OK || Statuses == null)
            {  
                ProcessNotifications();
                return;
            }

            DateTime LastChecked = DateSync.GetLastCheckDate();

            foreach (TwitterStatus status in Statuses)
            {
                if (status.CreatedDate > LastChecked)
                    TileNewMentions.Add(status);
            }

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
            PendingCalls--;
            if (Response.StatusCode != System.Net.HttpStatusCode.OK || Messages == null)
            {
                ProcessNotifications();
                return;
            }

            DateTime LastChecked = DateSync.GetLastCheckDate();

            foreach (TwitterDirectMessage status in Messages)
            {
                if (status.CreatedDate > LastChecked)
                    TileNewMessages.Add(status);
            }

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

            Ocell.Library.TileManager.UpdateTile(TileNewMentions, TileNewMessages);

            InternalNotifyComplete();
        }
        
        protected void InternalNotifyComplete()
        {
        	execEnded = true;
        }
    }
}