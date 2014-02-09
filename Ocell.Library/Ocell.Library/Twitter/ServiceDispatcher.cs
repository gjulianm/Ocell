using System.Collections.Generic;
using TweetSharp;
using BufferAPI;
using Sharplonger;


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

                var _srv = new TwitterService();
                _srv.AuthenticateWith(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, account.Key, account.Secret);

                srv = _srv;

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
            if (Config.Accounts.Value.Count > 0)
            {
                UserToken account = Config.Accounts.Value[0];
                return GetService(account);
            }
            else
            {
                return new TwitterService(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret);
            }
        }

        public static ITwitterService GetCurrentService()
        {
            return GetService(DataTransfer.CurrentAccount);
        }

        public static TwitlongerService GetTwitlongerService(UserToken user)
        {
            return GetTwitlongerService(user.ScreenName);
        }

        public static TwitlongerService GetTwitlongerService(string user)
        {
            return new TwitlongerService(SensitiveData.TwitlongerAppName, SensitiveData.TwitlongerApiKey, user);
        }

        public static BufferService GetBufferService()
        {
            if (Config.BufferAccessToken.Value != null)
                return new BufferService(Config.BufferAccessToken.Value);
            else
                return null;
        }

        public static bool CanGetServices
        {
            get
            {
                return Config.Accounts.Value.Count > 0;
            }
        }

        public static void Dispose()
        {
            _services.Clear();
        }
    }
}