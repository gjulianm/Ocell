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
    public class ToColumn : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string))
                return "error";

            string str = (value as string);
            int posSemicolon = str.IndexOf(';');
            string user = str.Substring(0, Math.Max(0, posSemicolon));
            int posPoints = str.IndexOf(':');
            int posSlash = str.IndexOf('/');
            int whereToCut = Math.Max(Math.Max(posPoints, posSlash), posSemicolon) + 1;
            str = user + ": " + str.Substring(whereToCut);
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
