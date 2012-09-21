using System.Collections.Generic;
using TweetSharp;


namespace Ocell.Library.Twitter
{
    public static class ServiceDispatcher 
    {
        private static object _lockFlag = new object();
        private static Dictionary<string, ITwitterService> _services;
        private static Dictionary<string, StreamingClient> _clients = new Dictionary<string, StreamingClient>();

        public static bool TestSession { get; set; }

        public static StreamingClient GetStreamingService(UserToken account)
        {
            lock (_lockFlag)
            {
                if (_clients == null)
                    _clients = new Dictionary<string, StreamingClient>();
            }

            if (account == null || account.Key == null)
                return null;

            lock (_lockFlag)
            {
                if (_clients.ContainsKey(account.Key))
                    return _clients[account.Key];
            }

            var srv = new StreamingClient(account);

            lock (_lockFlag)
                _clients.Add(account.Key, srv);

            return srv;
        }

        public static ITwitterService GetService(UserToken account)
        {
            lock (_lockFlag)
            {
                if (_services == null)
                    _services = new Dictionary<string, ITwitterService>();
            }

            if (account == null || account.Key == null)
                return null;

            lock (_lockFlag)
            {
                if (_services.ContainsKey(account.Key))
                    return _services[account.Key];
            }

            ITwitterService srv;
            if (TestSession)
                srv = new MockTwitterService();
            else
            {
                var tempSrv = new TwitterService();
                tempSrv.AuthenticateWith(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret, account.Key, account.Secret);
                srv = tempSrv;
            }            

            lock(_lockFlag)
                _services.Add(account.Key, srv);
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
            _services.Clear();
        }
    }
}