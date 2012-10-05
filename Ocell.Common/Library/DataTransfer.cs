using Ocell.Library.Twitter;
using Ocell.Library.Filtering;
using System;
using System.Linq;
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

        // Workaround for this version. This class will be trashed in the near future.
        static UserToken _account;
        public static UserToken CurrentAccount
        {
            get
            {
                if (_account == null)
                    return Config.Accounts.FirstOrDefault();
                return _account;
            }
            set
            {
                _account = value;
            }
        }
        public static long DMDestinationId;
        public static ITweetableFilter Filter;
        public static ColumnFilter cFilter;
        public static bool IsGlobalFilter;
        public static bool ShouldReloadColumns;
        public static bool ShouldReloadFilters = false;
        public static TwitterDraft Draft;
        public static GroupedDM DMGroup;
    }
}
