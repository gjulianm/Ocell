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
using System.Collections.Generic;
namespace Ocell.Library
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

        public static void SwitchAccountState(UserToken User)
        {
            if (IsProtected(User))
                UnprotectAccount(User);
            else
                ProtectAccount(User);
        }
    }
}
