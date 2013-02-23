using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Ocell
{
    public class ResourceTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is ResourceType))
                return "";

            var t = (ResourceType)value;

            switch (t)
            {
                case ResourceType.List:
                    return Localization.Resources.List + ": ";
                case ResourceType.Search:
                    return Localization.Resources.Search_CapitalFirst + ": ";
                default:
                    return "";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
