using System.Collections.Generic;
using TweetSharp;
using BufferAPI;


namespace Ocell.Library.Twitter
{
    public static class ServiceDispatcher 
    {
        private static object _lockFlag = new object();
        private static Dictionary<string, ITwitterService> _services;

        public static bool TestSession { get; set; }

        public static ITwitterService GetService(UserToken account)
        {
            lock (_lockFlag)
            {
                if (_services == null)
                    _services = new Dictionary<string, ITwitterService>();
            }

            if (account == null || account.Key == null)
                return null;

            ITwitterService srv;
            
            lock (_lockFlag)
            {
                if (_services.ContainsKey(account.Key))
                    return _services[account.Key];


                
                if (TestSession)
                    srv = new MockTwitterService();
                else
                {
                    var tempSrv = new TwitterService();
                    tempSrv.AuthenticateWith(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, account.Key, account.Secret);
                    srv = tempSrv;
                }

                try
                {

                    _services.Add(account.Key, srv);
                }
                catch
                {
                    // Again, this sometimes gives some weird exceptions. Investigate!
                }
            }

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

        public static Sharplonger.TwitlongerService GetTwitlongerService(UserToken user)
        {
            return GetTwitlongerService(user.ScreenName);
        }

        public static Sharplonger.TwitlongerService GetTwitlongerService(string user)
        {
            return new Sharplonger.TwitlongerService(SensitiveData.TwitlongerAppName, SensitiveData.TwitlongerApiKey, user);
        }

        public static BufferService GetBufferService()
        {
            if (Config.BufferAccessToken != null)
                return new BufferService(Config.BufferAccessToken);
            else
                return null;
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
            _services.Clear();
        }
    }
}