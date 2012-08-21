using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;

namespace Ocell
{
    public class ProtectedConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is UserToken))
                return null;

            if (ProtectedAccounts.IsProtected(value as UserToken))
                return Resources.Unprotect;
            else
                return Resources.Protect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
