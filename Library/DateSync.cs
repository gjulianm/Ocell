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
using System.Threading;
namespace Ocell.Library
{
    public static class DateSync
    {
        private const string FileName = "DateFile";
        private static Mutex _mutex = new Mutex(false, "OcellDateSync");
        public static void WriteLastCheckDate(DateTime date)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File;
            try
            {
                _mutex.WaitOne();
                 File = storage.OpenFile(FileName, System.IO.FileMode.Create);
                 File.WriteLine(date.ToString("s"));
                 File.Close();
                 _mutex.ReleaseMutex();
            }
            catch (IsolatedStorageException)
            {
                return;
            }
        }

        public static DateTime GetLastCheckDate()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            string dateStr;

            try
            {
                _mutex.WaitOne();
                IsolatedStorageFileStream File = storage.OpenFile(FileName, System.IO.FileMode.OpenOrCreate);
                dateStr = File.ReadLine();
                File.Close();
                _mutex.ReleaseMutex();
            }
            catch (Exception)
            {
                return DateTime.Now.ToUniversalTime();
            }

            DateTime date;

            if (!DateTime.TryParse(dateStr, out date))
                date = DateTime.Now.ToUniversalTime();

            return date;
        }
    }
}
