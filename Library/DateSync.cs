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

namespace Ocell.Library
{
    public static class DateSync
    {
        private const string FileName = "DateFile";

        public static void WriteLastCheckDate(DateTime Date)
        {
            IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File;
            try
            {
                 File = Storage.OpenFile(FileName, System.IO.FileMode.Create);
                 File.WriteLine(Date.ToString("s"));
                 File.Close();
            }
            catch (IsolatedStorageException)
            {
                return;
            }
        }

        public static DateTime GetLastCheckDate()
        {
            IsolatedStorageFile Storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File;
            string DateStr;

            try
            {
                 File= Storage.OpenFile(FileName, System.IO.FileMode.OpenOrCreate);
                 DateStr = File.ReadLine();
            }
            catch (Exception)
            {
                return DateTime.Now.ToUniversalTime();
            }

            DateTime Date;

            if (!DateTime.TryParse(DateStr, out Date))
                Date = DateTime.Now.ToUniversalTime();

            return Date;
        }
    }
}
