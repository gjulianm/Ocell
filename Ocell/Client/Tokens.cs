using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace Ocell
{
    public static class Tokens
    {
        // Store here the variables, so we won't need to call the settings everytime.
        private static string PV_CTK;
        private static string PV_CSC;
        private static string PV_UTK;
        private static string PV_USC;

        public static string consumer_token
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PV_CTK))
                    return PV_CTK;

                IsolatedStorageSettings config;

                try
                {
                    config = IsolatedStorageSettings.ApplicationSettings;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return string.Empty;
                }

                string consumerKey;

                consumerKey = SensitiveData.ConsumerToken;

                if (string.IsNullOrWhiteSpace(consumerKey))
                {
                    return string.Empty;
                }
                PV_CTK = consumerKey;
                return consumerKey;
            }
        }

        public static string consumer_secret
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PV_CSC))
                    return PV_CSC;

                IsolatedStorageSettings config;

                try
                {
                    config = IsolatedStorageSettings.ApplicationSettings;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return string.Empty;
                }

                string consumerKey;

                consumerKey = SensitiveData.ConsumerSecret;

                if (string.IsNullOrWhiteSpace(consumerKey))
                {
                    return string.Empty; ;
                }
                PV_CSC = consumerKey;
                return consumerKey;
            }
        }

        public static string user_token
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PV_UTK))
                    return PV_UTK;
                IsolatedStorageSettings config;

                try
                {
                    config = IsolatedStorageSettings.ApplicationSettings;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return string.Empty;
                }

                string consumerKey;

                config.TryGetValue<string>("USER_TOKEN", out consumerKey);

                PV_UTK = consumerKey;
                return consumerKey;
            }
            set
            {
                PV_UTK = value;

                IsolatedStorageSettings config;

                try
                {
                    config = IsolatedStorageSettings.ApplicationSettings;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return;
                }

                config.Add("USER_TOKEN", value);

                config.Save();
            }
        }

        public static string user_secret
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PV_USC))
                    return PV_USC;

                IsolatedStorageSettings config;

                try
                {
                    config = IsolatedStorageSettings.ApplicationSettings;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return string.Empty;
                }

                string consumerKey;

                config.TryGetValue<string>("USER_SECRET", out consumerKey);

                return consumerKey;
            }
            set
            {
                PV_USC = value;

                IsolatedStorageSettings config;

                try
                {
                    config = IsolatedStorageSettings.ApplicationSettings;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return;
                }

                config.Add("USER_SECRET", value);

                config.Save();
            }
        }

        public static string request_token { get; set; }
        public static string request_secret { get; set; }
    }

    
}
