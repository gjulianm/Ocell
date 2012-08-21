using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Text;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace Ocell
{
    public static class SchedulerSync
    {
        private const string FileName = "DateFile";

        public static void WriteLastCheckDate(DateTime Date)
        {
            Library.DateSync.WriteLastCheckDate(Date);
        }

        public static DateTime GetLastCheckDate()
        {
            return Library.DateSync.GetLastCheckDate();
        }

        public static void StartPeriodicAgent()
        {
            string periodicTaskName = "OcellPeriodicTask";
            PeriodicTask periodicTask = null;

            try
            {
                 periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
                 
            }
            catch (Exception)
            {
            }

            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);
            periodicTask.Description = "Updates live tile, sends scheduled tweets.";

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    ScheduledActionService.Add(periodicTask);
                    ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(30));
                }
                catch (Exception)
                {
                }
            });
        }

        private static void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }
    }
}
