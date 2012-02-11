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
using System.Collections;
using System.Linq;

namespace Ocell
{
    public static class ServiceDispatcher
    {
        private static Dictionary<string, TwitterService> _list;
        
        public static TwitterService GetService(UserToken account)
        {
            if(_list == null)
                _list = new Dictionary<string, TwitterService>();

            if (account == null || account.Key == null)
                return null;

            if(_list.ContainsKey(account.Key))
                return _list[account.Key];
            
            TwitterService srv = new TwitterService();
            srv.AuthenticateWith(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, account.Key, account.Secret);
            _list.Add(account.Key, srv);
            return srv;
        }

        public static TwitterService GetDefaultService()
        {
            if (Config.Accounts.Count > 0)
            {
                UserToken account = Config.Accounts[0];
                return GetService(account);
            }

            return null;
        }

        public static TwitterService GetCurrentService()
        {
            return GetService(DataTransfer.CurrentAccount);
        }
    }
}