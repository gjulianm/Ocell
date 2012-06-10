using System.Collections.Generic;
using TweetSharp;


namespace Ocell.Library.Twitter
{
    public static class ServiceDispatcher 
    {
        private static object _lockFlag = new object();
        private static Dictionary<string, ITwitterService> _list;

        public static bool TestSession { get; set; }

        public static ITwitterService GetService(UserToken account)
        {
            lock (_lockFlag)
            {
                if (_list == null)
                    _list = new Dictionary<string, ITwitterService>();
            }

            if (account == null || account.Key == null)
                return null;

            lock (_lockFlag)
            {
                if (_list.ContainsKey(account.Key))
                    return _list[account.Key];
            }

            ITwitterService srv;
            if (TestSession)
                srv = new MockTwitterService();
            else
                srv = new TwitterService();

            srv.AuthenticateWith(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, account.Key, account.Secret);

            lock(_lockFlag)
                _list.Add(account.Key, srv);
            return srv;
        }

        public static ITwitterService GetDefaultService()
        {
            if (Config.Accounts.Count > 0)
            {
                UserToken account = Config.Accounts[0];
                return GetService(account);
            }
            else
            {
                if (!TestSession)
                    return new TwitterService(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret);
                else
                    return new MockTwitterService();
            }
        }

        public static ITwitterService GetCurrentService()
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