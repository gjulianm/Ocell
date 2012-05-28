using System.Collections.Generic;
using TweetSharp;


namespace Ocell.Library.Twitter
{
    public static class ServiceDispatcher 
    {
        private static object _lockFlag = new object();
        private static Dictionary<string, TwitterService> _list;
        
        public static TwitterService GetService(UserToken account)
        {
            lock (_lockFlag)
            {
                if (_list == null)
                    _list = new Dictionary<string, TwitterService>();
            }

            if (account == null || account.Key == null)
                return null;

            lock (_lockFlag)
            {
                if (_list.ContainsKey(account.Key))
                    return _list[account.Key];
            }
            
            TwitterService srv = new TwitterService();
            srv.AuthenticateWith(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, account.Key, account.Secret);

            lock(_lockFlag)
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

        public static bool CanGetServices
        {
            get
            {
                return Config.Accounts.Count > 0;
            }
        }

        public static void Dispose()
        {
            _list.Clear();
        }
    }
}