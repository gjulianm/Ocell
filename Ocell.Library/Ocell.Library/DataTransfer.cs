using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using System.Linq;

namespace Ocell.Library
{
    public static class DataTransfer
    {
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
                    return Config.Accounts.Value.FirstOrDefault();
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
        public static bool ShouldReloadFilters = false;
    }
}
