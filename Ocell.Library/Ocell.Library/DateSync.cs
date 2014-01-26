using AncoraMVVM.Base.Files;
using AncoraMVVM.Base.IoC;
using System;
using System.Threading.Tasks;



namespace Ocell.Library
{
    public static class DateSync
    {
        private const string FileName = "DateFile";

        private static void WriteDate(DateTime date, string file)
        {
            Dependency.Resolve<IFileManager>().WriteContents(date.ToString("s"), file);
        }

        private async static Task<DateTime> GetDate(string file)
        {
            string dateStr = await Dependency.Resolve<IFileManager>().ReadContents(file);

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

        public async static Task<DateTime> GetLastCheckDate()
        {
            return await GetDate("DateFile");
        }

        public static async Task<DateTime> GetLastToastNotificationDate()
        {
            return await GetDate("ToastFile");
        }
    }
}