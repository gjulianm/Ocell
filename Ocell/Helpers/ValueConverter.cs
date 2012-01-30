using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;


namespace Ocell
{
    public class RelativeDateTimeConverter : IValueConverter
    {
        private const int Minute = 60;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        private const int Year = Day * 365;

        private readonly Dictionary<long, Func<TimeSpan, string>> thresholds = new Dictionary<long, Func<TimeSpan, string>>
        {
            {2, t => "a second ago"},
            {Minute,  t => String.Format("{0} seconds ago", (int)t.TotalSeconds)},
            {Minute * 2,  t => "a minute ago"},
            {Hour,  t => String.Format("{0} minutes ago", (int)t.TotalMinutes)},
            {Hour * 2,  t => "an hour ago"},
            {Day,  t => String.Format("{0} hours ago", (int)t.TotalHours)},
            {Day * 2,  t => "yesterday"},
            {Day * 30,  t => String.Format("{0} days ago", (int)t.TotalDays)},
            {Day * 60,  t => "last month"},
            {Year,  t => String.Format("{0} months ago", (int)t.TotalDays / 30)},
            {Year * 2,  t => "last year"},
            {Int64.MaxValue,  t => String.Format("{0} years ago", (int)t.TotalDays / 365)}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime;
            if (value.GetType() == "".GetType())
                dateTime = DateTime.Parse((string)value);
            else
                dateTime = (DateTime)value;
            var difference = DateTime.UtcNow - dateTime.ToUniversalTime();

            return thresholds.First(t => difference.TotalSeconds < t.Key).Value(difference);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ListConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if(value == null || !(value is string))
                return "error";

            string str = (value as string).ToLowerInvariant();
            int posPoints = str.IndexOf(':');
            int posSlash = str.IndexOf('/');
            int whereToCut = Math.Max(posPoints, posSlash) + 1 ;
            if (whereToCut == 0)
                return str;
            else
                return str.Substring(whereToCut);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FirstToUpper : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            string p = (string)value;
            p = char.ToUpper(p[0])+p.Substring(1);
            return p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class RemoveHTML : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            string what = value.GetType()=="".GetType()?(string)value:value.ToString();
            Regex exp = new Regex("<[^>]+>");
            return exp.Replace(what, (target) => { return ""; });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ResourceToString : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine(value.GetType().ToString());
            if (!(value is TwitterResource))
                return "";

            TwitterResource res = (TwitterResource)value;
            Debug.WriteLine(res);
            Debug.WriteLine(res.Type);
            switch (res.Type)
            {
                case ResourceType.Favorites:
                    return "Favorites";
                case ResourceType.Home:
                    return "Home";
                case ResourceType.List:
                    return "List:" + res.Data;
                case ResourceType.Mentions:
                    return "Mentions";
                case ResourceType.Messages:
                    return "Messages";
                case ResourceType.Search:
                    return "Search:" + res.Data;
                default:
                    return "Unknown value";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string))
                return new TwitterResource { Type = ResourceType.Home, Data = "" };

            TwitterResource Resource = new TwitterResource();
            Resource.Data = "";
            string what = (string) value;
            if(!what.Contains(':')) 
            {
                if(what == "Favorites")
                    Resource.Type = ResourceType.Favorites;
                else if (what == "Home")
                    Resource.Type = ResourceType.Home;
                else if (what == "Mentions")
                        Resource.Type = ResourceType.Mentions;
                else if (what == "Messages")
                        Resource.Type = ResourceType.Messages;
            }
            else if(what.Contains("List:"))
            {
                Resource.Type = ResourceType.List;
                Resource.Data = what.Substring(what.IndexOf(':')+1);
            }
            else if(what.Contains("Search:"))
            {
                Resource.Type = ResourceType.Search;
                Resource.Data = what.Substring(what.IndexOf(':')+1);
            }
            else
                Resource.Type = ResourceType.Home;

            return Resource;

        }
    }
}
