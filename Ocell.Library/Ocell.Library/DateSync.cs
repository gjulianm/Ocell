﻿using System;



namespace Ocell.Library
{
    public static class DateSync
    {
        private const string FileName = "DateFile";

        private static void WriteDate(DateTime date, string file)
        {
            FileAbstractor.WriteContentsToFile(date.ToString("s"), file);
        }

        private static DateTime GetDate(string file)
        {
            string dateStr = FileAbstractor.ReadContentsOfFile(file);

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

        public static DateTime GetLastCheckDate()
        {
            return GetDate("DateFile");
        }

        public static DateTime GetLastToastNotificationDate()
        {
            return GetDate("ToastFile");
        }
    }
}