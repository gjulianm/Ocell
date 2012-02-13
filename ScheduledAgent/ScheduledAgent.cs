using System.Windows;
using Microsoft.Phone.Scheduler;
using TweetSharp;
using Ocell.Library;
using System.Collections.Generic;
using System;
using Microsoft.Phone.Shell;
using System.Linq;

namespace ScheduledAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private List<TwitterStatus> NewMentions;
        private int PendingCalls;

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

            NewMentions = new List<TwitterStatus>();
            PendingCalls = 0;
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
            foreach (UserToken User in Config.Accounts)
                GetMentionsFor(User);
        }

        protected void GetMentionsFor(UserToken User)
        {
            TwitterService Service = ServiceDispatcher.GetService(User);
            PendingCalls++;
            Service.ListTweetsMentioningMe(ReceiveTweets);
        }

        protected void ReceiveTweets(IEnumerable<TwitterStatus> Statuses, TwitterResponse Response)
        {
            PendingCalls--;
            if (Response.StatusCode != System.Net.HttpStatusCode.OK || Statuses == null)
            {  
                UpdateTileData();
                return;
            }

            DateTime LastChecked = SchedulerSync.GetLastCheckDate();

            foreach (TwitterStatus status in Statuses)
            {
                if (status.CreatedDate > LastChecked)
                    NewMentions.Add(status);
            }
        }

        protected void UpdateTileData()
        {
            if (PendingCalls > 0)
                return;

            ShellTile MainTile = ShellTile.ActiveTiles.First();

            if (MainTile != null && NewMentions.Count > 0)
            {
                string BackText;

                if(NewMentions.Count == 1)
                    BackText = "New mention by " + NewMentions[0].Author.ScreenName;
                else
                    BackText = NewMentions.Count.ToString() + " new mentions";

                MainTile.Update(new StandardTileData
                {
                    Count = NewMentions.Count,
                    BackContent = BackText
                });
            }

            NotifyComplete();
        }
    }
}