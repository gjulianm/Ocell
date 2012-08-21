using System.Collections.Generic;

namespace Ocell.Library.Twitter
{
    public static class ProtectedAccounts
    {
        private static List<UserToken> _protectedAccounts;

        public static bool IsProtected(UserToken User)
        {
            InitializeIfNull();

            if (User == null)
                return false;
            return _protectedAccounts.Contains(User);
        }

        private static void InitializeIfNull()
        {
            if (_protectedAccounts == null)
                _protectedAccounts = Config.ProtectedAccounts;
        }

        public static void ProtectAccount(UserToken User)
        {
            InitializeIfNull();

            if (!_protectedAccounts.Contains(User))
                _protectedAccounts.Add(User);
        }

        public static void UnprotectAccount(UserToken User)
        {
            InitializeIfNull();

            if (_protectedAccounts.Contains(User))
                _protectedAccounts.Remove(User);
        }

        public static bool SwitchAccountState(UserToken User)
        {
            if (IsProtected(User))
            {
                UnprotectAccount(User);
                return false;
            }
            else
            {
                ProtectAccount(User);
                return true;
            }
        }
    }
}
