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
using System.Windows;


namespace Ocell
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            double NormalSize = 64;
            if (value is TwitterStatus && ((TwitterStatus)value).RetweetedStatus != null)
                return 0.75 * NormalSize;
            else
                return NormalSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            SizeConverter converter = new SizeConverter();
            object returnvalue = converter.Convert(value, targeType, parameter, culture);
            double left = 8 + 64 - (double)returnvalue;

            Thickness Margin = new Thickness
            {
                Top = 14,
                Bottom = 0,
                Right = 8,
                Left = left
            };

            return Margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class AvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (!(value is ITweetable))
                return "";

            if (!(value is TwitterStatus))
            {
                if ((string)parameter == "false" && ((ITweetable)value).Author != null)
                    return ((ITweetable)value).Author.ProfileImageUrl;
                else
                    return "";
            }

            TwitterStatus Status = value as TwitterStatus;

            if ((string)parameter == "false")
            {
                if (Status.RetweetedStatus != null)
                    return Status.RetweetedStatus.Author.ProfileImageUrl;
                else if(Status.Author != null)
                    return Status.Author.ProfileImageUrl;
            }
            else
            {
                if (Status.RetweetedStatus != null && Status.RetweetedStatus.Author != null)
                    return Status.Author.ProfileImageUrl;
                else
                    return "";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class ScreenNameConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (!(value is ITweetable))
                return "";

            if (!(value is TwitterStatus))
                return ((ITweetable)value).Author.ScreenName;

            TwitterStatus Status = value as TwitterStatus;

            if (Status.RetweetedStatus != null)
                return Status.RetweetedStatus.Author.ScreenName;
            else
                return Status.Author.ScreenName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
