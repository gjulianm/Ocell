using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ocell.Library;
using TweetSharp;
using System.Net;
using Ocell.Library.Filtering;

namespace Ocell
{
    public class IncludeExcludeConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (!(value is IncludeOrExclude))
                return "Unknown";

            IncludeOrExclude what = (IncludeOrExclude)value;
            if (what == IncludeOrExclude.Include)
                return "does not contain";
            else
                return "contains";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FilterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            ITweetableFilter filter = value as ITweetableFilter;
            if (filter != null)
            {
                switch (filter.Type)
                {
                    case FilterType.User:
                        return "user";
                    case FilterType.Text:
                        return "tweet text";
                    case FilterType.Source:
                        return "source";
                }
            }

            return "unknown";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FilterDateConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            
            if (value is DateTime)
            {
                DateTime date = (DateTime)value;
                if (date == DateTime.MaxValue)
                    return "forever.";
                else
                    return "until " + date.ToString("f") +".";
            }

            return "forever.";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
