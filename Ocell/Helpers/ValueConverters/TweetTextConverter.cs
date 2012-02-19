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
    public class TweetTextConverter : IValueConverter
    {
        public static string TrimUrl(string url)
        {
            url = url.Replace("http://", "");
            if (url.Length > 25)
            {
                int SlashIndex = url.IndexOf('/');
                url = url.Substring(0, SlashIndex + 1);
                url += "...";
            }

            return url;
        }

        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            if (!(value is ITweetable))
                return "";

            ITweetable Tweet = value as ITweetable;

            if (Tweet is TwitterStatus)
            {
                TwitterStatus Status = Tweet as TwitterStatus;
                if (Status.RetweetedStatus != null)
                    Tweet = Status.RetweetedStatus;
            }

            string TweetText = Tweet.Text;
            string ReturnText = "";
            string PreviousText;
            int i = 0;

            foreach (var Entity in Tweet.Entities)
            {
                if (Entity.StartIndex > i)
                {
                    PreviousText = TweetText.Substring(i, Entity.StartIndex - i);
                    ReturnText += HttpUtility.HtmlDecode(PreviousText);
                }

                i = Entity.EndIndex;

                switch (Entity.EntityType)
                {
                    case TwitterEntityType.HashTag:
                        ReturnText += "#" + ((TwitterHashTag)Entity).Text;
                        break;

                    case TwitterEntityType.Mention:
                        ReturnText += "@" + ((TwitterMention)Entity).ScreenName;
                        break;

                    case TwitterEntityType.Url:
                        ReturnText += TrimUrl(((TwitterUrl)Entity).ExpandedValue);
                        break;
                    case TwitterEntityType.Media:
                        ReturnText += ((TwitterMedia)Entity).DisplayUrl;
                        break;
                }
            }

            if (i < TweetText.Length)
                ReturnText += HttpUtility.HtmlDecode(TweetText.Substring(i));

            return ReturnText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
