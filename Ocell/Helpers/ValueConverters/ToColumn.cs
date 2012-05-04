using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ocell.Library;
using Ocell.Library.Twitter;


namespace Ocell
{
    public class ToColumn : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is TwitterResource))
                return "error";

            TwitterResource resource = (TwitterResource)value;

            string listName = "";
            int indexOfSlash = resource.Data.IndexOf('/');
            if (indexOfSlash != -1)
                listName = resource.Data.Substring(indexOfSlash + 1);

            switch (resource.Type)
            {
                case ResourceType.Search:
                    return "Search/" + resource.Data;
                case ResourceType.Tweets:
                    return "User/@" + resource.Data;
                case ResourceType.List:
                    return "List/" + listName;
                case ResourceType.Favorites:
                    return "Favorites";
                case ResourceType.Home:
                    return "Home";
                case ResourceType.Mentions:
                    return "Mentions";
                case ResourceType.Messages:
                    return "Direct messages";
                default:
                    return "Error";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
