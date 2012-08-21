using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ocell.Library;
using Ocell.Localization;

namespace Ocell
{
    public class RelativeDateTimeConverter : IValueConverter
    {
        private const int Minute = 60;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        private const int Year = Day * 365;

        private readonly static Dictionary<long, Func<TimeSpan, string>> thresholds = new Dictionary<long, Func<TimeSpan, string>>
        {
            {2, t => Resources.ASecondAgo},
            {Minute,  t => String.Format(Resources.XSecondsAgo, (int)t.TotalSeconds)},
            {Minute * 2,  t => Resources.AMinuteAgo},
            {Hour,  t => String.Format(Resources.XMinutesAgo, (int)t.TotalMinutes)},
            {Hour * 2,  t => Resources.AnHourAgo},
            {Day,  t => String.Format(Resources.XHoursAgo, (int)t.TotalHours)},
            {Day * 2,  t => Resources.Yesterday},
            {Day * 30,  t => String.Format(Resources.XDaysAgo, (int)t.TotalDays)},
            {Day * 60,  t => Resources.LastMonth},
            {Year,  t => String.Format(Resources.XMonthsAgo, (int)t.TotalDays / 30)},
            {Year * 2,  t => Resources.LastYear},
            {Int64.MaxValue,  t => String.Format(Resources.XYearsAgo, (int)t.TotalDays / 365)}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime;
            if (value.GetType() == "".GetType())
                dateTime = DateTime.Parse((string)value);
            else
                dateTime = (DateTime)value;

            if (dateTime == DateTime.MinValue)
                return "";

            var difference = DateTime.UtcNow - dateTime.ToUniversalTime();

            return thresholds.First(t => difference.TotalSeconds < t.Key).Value(difference);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
