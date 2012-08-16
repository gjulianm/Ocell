using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ocell.Library;


namespace Ocell
{
    public class ListConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string))
                return Localization.Resources.Error;

            string str = (value as string).ToLowerInvariant();
            int posSemicolon = str.IndexOf(';');
            int posPoints = str.IndexOf(':');
            int posSlash = str.IndexOf('/');

            int whereToCut = Math.Max(Math.Max(posPoints, posSlash), posSemicolon) + 1;

            return str.Substring(whereToCut);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
