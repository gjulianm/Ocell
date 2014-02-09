using Microsoft.Phone.Scheduler;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Ocell
{
    public static class SchedulerSync
    {
        private const string FileName = "DateFile";

        public static void WriteLastCheckDate(DateTime Date)
        {
            Library.DateSync.WriteLastCheckDate(Date);
        }

        public static async Task<DateTime> GetLastCheckDate()
        {
            return await Library.DateSync.GetLastCheckDate();
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
