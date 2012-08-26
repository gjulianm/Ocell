using System;
using System.Threading;

#if METRO
using Windows.Storage;
using System.Threading.Tasks;
#else
using System.IO.IsolatedStorage;
#endif

namespace Ocell.Library
{
    public static class DateSync
    {
        private const string FileName = "DateFile";
        private static Mutex _mutex = new Mutex(false, "OcellDateSync");

        private static void WriteDate(DateTime date, string file)
        {
            FileAbstractor.WriteContentsToFile(date.ToString("s"), file);
        }

#if !METRO
        private static DateTime GetDate(string file)
#else 
        private static async Task<DateTime> GetDate(string file)
#endif
        {
#if !METRO
            string dateStr = FileAbstractor.ReadContentsOfFile(file);
#else
            string dateStr = await FileAbstractor.ReadContentsOfFile(file);
#endif


            DateTime date;

            if (!DateTime.TryParse(dateStr, out date))
                date = DateTime.Now.ToUniversalTime();

            return date;
        }

        public static void WriteLastCheckDate(DateTime date)
        {
            WriteDate(date, "DateFile");
        }        

        public static void WriteLastToastNotificationDate(DateTime date)
        {
            WriteDate(date, "ToastFile");
        }

#if !METRO
        public static DateTime GetLastCheckDate()
        {
            return GetDate("DateFile");
        }

        public static DateTime GetLastToastNotificationDate()
        {
            return GetDate("ToastFile");
        }
#else
        public async static Task<DateTime> GetLastCheckDate()
        {
            return await GetDate("DateFile");
        }

        public async static Task<DateTime> GetLastToastNotificationDate()
        {
            return await GetDate("ToastFile");
        }
#endif
    }
}
