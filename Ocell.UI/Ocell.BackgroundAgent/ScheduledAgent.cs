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
using System.Linq;
using Ocell.BackgroundAgent.Library;
using Ocell.Compatibility;

namespace Ocell.BackgroundAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
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
        }

        ~ScheduledAgent()
        {
            Dispose();
        }

        public void Dispose()
        {
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
            var agent = new BaseScheduledAgent();
            agent.TileManager = new WP7TileManager();
            agent.ApplicationMemoryLimit = () => DeviceStatus.ApplicationMemoryUsageLimit;
            agent.ApplicationMemoryUsage = () => DeviceStatus.ApplicationCurrentMemoryUsage;

            try
            {
                agent.Start();
            }
            catch (Exception e)
            {
                Logger.Log("Exception " + e.GetType().Name + ": " + e.Message);
            }

            Logger.Save();

            NotifyComplete();
        }
    }
}