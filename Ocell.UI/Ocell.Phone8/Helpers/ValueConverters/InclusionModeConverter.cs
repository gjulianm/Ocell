using Ocell.Library.Filtering;
using Ocell.Localization;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Ocell.Converters
{
    public class InclusionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (!(value is ExcludeMode))
                return "";

            var mode = (ExcludeMode)value;

            if (mode == ExcludeMode.ExcludeOnMatch)
                return Resources.Remove;
            else
                return Resources.ShowOnly;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FirstToLower : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            string p = (string)value;
            p = char.ToLower(p[0]) + p.Substring(1);
            return p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
