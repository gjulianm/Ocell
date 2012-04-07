using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TweetSharp;

using System.Reflection;
using System.Runtime.Serialization;

namespace Ocell.Library
{
    public enum IncludeOrExclude
    { Include, Exclude };

    public enum FilterType
    {
        User, Source, Text
    };

    public class ITweetableFilter
    {
        public string Filter { get; set; }
        public IncludeOrExclude Inclusion { get; set; }
        public FilterType Type { get; set; }
        private string getStringToCheck(ITweetable item)
        {
            if (item is LoadMoreTweetable)
                return "";

            switch (Type)
            {
                case FilterType.Source:
                    if (item is TwitterStatus)
                        return (item as TwitterStatus).Source.ToLowerInvariant();
                    else if (item is TwitterSearchStatus)
                        return (item as TwitterSearchStatus).Source.ToLowerInvariant();
                    else
                        return "";
                case FilterType.Text:
                    return item.Text.ToLowerInvariant();
                case FilterType.User:
                    return item.Author.ScreenName.ToLowerInvariant();
                default:
                    return "";
            }
        }
        public bool Evaluate(ITweetable item)
        {
            if (Filter == null)
                Filter = "";

            if (item == null)
                return false;

            string whatToCheck = getStringToCheck(item);
            
            bool result = whatToCheck.Contains(Filter.ToLowerInvariant());

            if (Inclusion == IncludeOrExclude.Exclude)
                return !result;
            else
                return result;
        }

        public override int GetHashCode()
        {
            return (int)Inclusion ^ (int)Type ^ Filter.GetHashCode();
        }
    }
}
