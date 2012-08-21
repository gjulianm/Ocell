using Ocell.Library.Twitter;
using TweetSharp;
using System;

namespace Ocell.Library.Filtering
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
        public DateTime IsValidUntil { get; set; }
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

            if (DateTime.Now > IsValidUntil)
                return true;

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
