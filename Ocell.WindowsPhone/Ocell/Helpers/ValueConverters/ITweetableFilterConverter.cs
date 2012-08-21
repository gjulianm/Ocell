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
using Ocell.Localization;

namespace Ocell
{
    public class IncludeExcludeConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (!(value is IncludeOrExclude))
                return Resources.UnknownValue;

            IncludeOrExclude what = (IncludeOrExclude)value;
            if (what == IncludeOrExclude.Include)
                return Resources.DoesNotContain;
            else
                return Resources.Contains;

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
                        return Resources.User;
                    case FilterType.Text:
                        return Resources.TweetText;
                    case FilterType.Source:
                        return Resources.Source;
                }
            }

            return Resources.UnknownValue;

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
                    return Resources.Forever + ".";
                else
                    return String.Format(Resources.UntilDate, date.ToString("f"));
            }

            return Resources.Forever + ".";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
