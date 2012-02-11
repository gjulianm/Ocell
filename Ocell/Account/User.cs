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
using System.Collections.Generic;

namespace Ocell
{
    public class Account : ITweeter
    {
        public string ProfileImageUrl { get; protected set; }
        public string ScreenName { get; protected set; }
        public string UserToken { get; protected set; }
        public string UserSecret { get; protected set; }
    }

    public class AccountEqualityComparer : IEqualityComparer<Account>
    {
        public bool Equals(Account a, Account b)
        {
            return a.UserToken == b.UserToken;
        }

        public int GetHashCode(Account a)
        {
            return a.UserToken.GetHashCode() ^ a.UserSecret.GetHashCode();
        }
    }
}
