using BufferAPI;
using Sharplonger;
using System.Collections.Generic;
using TweetSharp;


namespace Ocell.Library.Twitter
{
    public static class ServiceDispatcher
    {
        private static object lockFlag = new object();
        private static Dictionary<string, ITwitterService> services;

        public static string ApplicationKey { get; set; }
        public static string ApplicationSecret { get; set; }

        public static ITwitterService GetService(UserToken account)
        {
            lock (lockFlag)
            {
                if (services == null)
                    services = new Dictionary<string, ITwitterService>();
            }

            if (account == null || account.Key == null)
                return null;

            ITwitterService srv;

            if (ApplicationKey == null || ApplicationSecret == null)
                throw new ApplicationException("ApplicationKey/ApplicationSecret are null. We can't create Twitter Services.");

            lock (lockFlag)
            {
                if (services.ContainsKey(account.Key))
                    return services[account.Key];

                var _srv = new TwitterService();
                _srv.AuthenticateWith(ApplicationKey, ApplicationSecret, account.Key, account.Secret);

                srv = _srv;

                try
                {

                    services.Add(account.Key, srv);
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
            services.Clear();
        }
    }
}