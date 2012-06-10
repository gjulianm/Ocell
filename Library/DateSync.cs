using System;
using System.IO.IsolatedStorage;
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

            if (_mutex.WaitOne(1000))
            {
                try
                {
                    using (File = storage.OpenFile(FileName, System.IO.FileMode.Create))
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

        public static DateTime GetLastCheckDate()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            string dateStr = "";

            if (_mutex.WaitOne(1000))
            {
                try
                {
                    using (IsolatedStorageFileStream File = storage.OpenFile(FileName, System.IO.FileMode.OpenOrCreate))
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
    }
}
