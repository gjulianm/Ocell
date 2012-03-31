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
            if (value is UserFilter)
                return "user";
            else if (value is SourceFilter)
                return "source";
            else if (value is TextInfo)
                return "tweet text";
            else
                return "unknown";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
