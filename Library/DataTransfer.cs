using Ocell.Library.Twitter;
using Ocell.Library.Filtering;
using System;

namespace Ocell.Library
{
    public static class DataTransfer
    {
        public static TweetSharp.TwitterStatus Status;
        public static string Text;
        public static long ReplyId;
        public static string Search;
        public static string User;
        public static TweetSharp.TwitterDirectMessage DM;
        public static bool ReplyingDM;
        public static UserToken CurrentAccount;
        public static long DMDestinationId;
        public static ITweetableFilter Filter;
        public static ColumnFilter cFilter;
        public static bool IsGlobalFilter;
        public static bool ShouldReloadColumns;
        public static bool ShouldReloadFilters = false;
    }
}
