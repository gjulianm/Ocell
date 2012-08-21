using System;
using System.IO.IsolatedStorage;
using System.Threading;

namespace Ocell.Library
{
    public static class DateSync
    {
        private const string FileName = "DateFile";
        private static Mutex _mutex = new Mutex(false, "OcellDateSync");

        private static void WriteDate(DateTime date, string file)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream File;

            if (_mutex.WaitOne(1000))
            {
                try
                {
                    using (File = storage.OpenFile(file, System.IO.FileMode.Create))
                    {
                        File.WriteLine(date.ToString("s"));
                        File.Close();
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
        }

        private static DateTime GetDate(string file)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            string dateStr = "";

            if (_mutex.WaitOne(1000))
            {
                try
                {
                    using (IsolatedStorageFileStream File = storage.OpenFile(file, System.IO.FileMode.OpenOrCreate))
                    {
                        dateStr = File.ReadLine();
                    }

                }
                catch (Exception)
                {

                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }


            DateTime date;

            if (!DateTime.TryParse(dateStr, out date))
                date = DateTime.Now.ToUniversalTime();

            return date;
        }

        public static void WriteLastCheckDate(DateTime date)
        {
            WriteDate(date, "DateFile");
        }

        public static DateTime GetLastCheckDate()
        {
            return GetDate("DateFile");
        }

        public static void WriteLastToastNotificationDate(DateTime date)
        {
            WriteDate(date, "ToastFile");
        }

        public static DateTime GetLastToastNotificationDate()
        {
            return GetDate("ToastFile");
        }
    }
}
