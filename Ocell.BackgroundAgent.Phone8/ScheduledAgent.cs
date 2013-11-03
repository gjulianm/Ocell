using Microsoft.Phone.Info;
using Microsoft.Phone.Scheduler;
using Ocell.BackgroundAgent.Library;
using Ocell.Compatibility;
using Ocell.Library;
using System;
using System.Diagnostics;
using System.Windows;

namespace Ocell.BackgroundAgent.Phone8
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            var agent = new BaseScheduledAgent();
            agent.ApplicationMemoryUsage = () => DeviceStatus.ApplicationCurrentMemoryUsage;
            agent.ApplicationMemoryLimit = () => DeviceStatus.ApplicationMemoryUsageLimit;
            agent.TileManager = new WP8TileManager();

            try
            {
                agent.Start();
            }
            catch (Exception e)
            {
                Logger.Trace("Exception " + e.GetType().Name + ": " + e.Message);
            }

            NotifyComplete();
        }
    }
}