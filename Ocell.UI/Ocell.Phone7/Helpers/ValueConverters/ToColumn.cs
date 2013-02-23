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
    public class ToColumn : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is TwitterResource))
                return Resources.Error;

            TwitterResource resource = (TwitterResource)value;

            string listName = "";
            int indexOfSlash = resource.Data.IndexOf('/');
            if (indexOfSlash != -1)
                listName = resource.Data.Substring(indexOfSlash + 1);

            switch (resource.Type)
            {
                case ResourceType.Search:
                    return Resources.Search_CapitalFirst +"/" + resource.Data;
                case ResourceType.Tweets:
                    return Resources.User + "/@" + resource.Data;
                case ResourceType.List:
                    return Resources.Lists + "/" + listName;
                case ResourceType.Favorites:
                    return Resources.Favorites;
                case ResourceType.Home:
                    return Resources.Home;
                case ResourceType.Mentions:
                    return Resources.Mentions;
                case ResourceType.Messages:
                    return Resources.Messages;
                default:
                    return Resources.Error;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
