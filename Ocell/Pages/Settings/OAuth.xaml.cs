using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Web;
using System.Windows;
using Hammock;
using Hammock.Authentication.OAuth;
using Microsoft.Phone.Controls;
using System.Linq; 

namespace Ocell.Settings
{
    public partial class OAuth : PhoneApplicationPage
    {
        protected string consumerKey = SensitiveData.ConsumerToken;
        protected string consumerSecret = SensitiveData.ConsumerSecret;
        
        public OAuth()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(OAuth_Loaded);
        }

        void OAuth_Loaded(object sender, RoutedEventArgs e)
        {
            string callBackUrl = "http://www.google.es";

            // Use Hammock to set up our authentication credentials
            OAuthCredentials credentials = new OAuthCredentials()
            {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                CallbackUrl = callBackUrl,
                Version = "1.0a"
            };

            // Use Hammock to create a rest client
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                Credentials = credentials
            };

            // Use Hammock to create a request
            var request = new RestRequest
            {
                Path = "/oauth/request_token/"
            };

            // Get the response from the request
            client.BeginRequest(request, new RestCallback(GetRequestTokenResponse));
        }

        private void GetRequestTokenResponse(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() => { 
                    MessageBox.Show("Error while authenticating with Twitter. Please try again");
                    NavigationService.GoBack();
                });

            }
                        
            var collection = HttpUtility.ParseQueryString(response.Content);
            string request_token = collection["oauth_token"];
            string token_secret = collection["oauth_token_secret"];
            
            Tokens.request_token = request_token;
            Tokens.request_secret = token_secret;

            Dispatcher.BeginInvoke(() => { wb.Navigate(new Uri("http://api.twitter.com/oauth/authorize?oauth_token=" + request_token)); });
        }

        private string GetQueryString(string Query)
        {
            int index = Query.IndexOf("?");
            if (index > 0)
                Query = Query.Substring(index).Remove(0, 1);

            return Query;
        }

        private void wb_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.Host.Contains("google"))
            {
                // This is the Twitter callback, so cancel the call and manage the tokens.
                e.Cancel = true;
                string url = GetQueryString(e.Uri.Query);

                var collection = HttpUtility.ParseQueryString(url);

                string token = collection["oauth_token"];
                string verifier = collection["oauth_verifier"];

                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(verifier))
                {
                    Dispatcher.BeginInvoke(() => { 
                        MessageBox.Show("Authentication error.");
                        NavigationService.GoBack();
                    });
                    return;
                }

                GetFullTokens(token, verifier);
            }
        }

        private void GetFullTokens(string token, string verifier)
        {
            // Use Hammock to set up our authentication credentials
            OAuthCredentials credentials = new OAuthCredentials()
            {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = token,
                TokenSecret = Tokens.request_secret,
                Verifier = verifier,
                CallbackUrl = "http://google.es",
                Version = "1.0a"
            };

            // Use Hammock to create a rest client
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                Credentials = credentials
            };

            // Use Hammock to create a request
            var request = new RestRequest
            {
                Path = "/oauth/access_token/"
            };

            // Get the response from the request
            client.BeginRequest(request, new RestCallback(RequestCompleted));
        }

        public void RequestCompleted(RestRequest req, RestResponse response, object userstate)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() => { 
                    MessageBox.Show("Error while authenticating with Twitter");
                    NavigationService.GoBack();
                });
                return;
            }

            var collection = HttpUtility.ParseQueryString(response.Content);
            
            UserToken Token = new UserToken {
                Key = collection["oauth_token"],
                Secret = collection["oauth_token_secret"]
            };

            Token.UserDataFilled += new UserToken.OnUserDataFilled(InsertTokenIntoAccounts);
            Token.FillUserData();
            CreateColumns(Token);

           
        }

        private void CreateColumns(UserToken user)
        {
            TwitterResource Home = new TwitterResource { Type = ResourceType.Home, User = user };
            TwitterResource Mentions = new TwitterResource { Type = ResourceType.Mentions, User = user };
            Dispatcher.BeginInvoke(() =>
            {
                if(!Config.Columns.Contains(Home))
                    Config.Columns.Add(Home);
                if(!Config.Columns.Contains(Mentions))
                    Config.Columns.Add(Mentions);
                Config.SaveColumns();
            });
        }

        private void InsertTokenIntoAccounts(UserToken Token)
        {
            CheckIfExistsAndInsert(Token);

            Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); });
            return;
        }

        private static void CheckIfExistsAndInsert(UserToken Token)
        {
            foreach (var item in Config.Accounts)
            {
                if (item.Key == Token.Key && item.ScreenName == Token.ScreenName)
                {
                    if (item.Secret != Token.Secret)
                    {
                        Config.Accounts.Remove(item);
                        Config.Accounts.Add(Token);
                        Config.SaveAccounts();
                    }
                    return;
                }
            }
            Config.Accounts.Add(Token);
            Config.SaveAccounts();
        }

        private void wb_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if(e.Uri.Host.Contains("api.twitter.com"))
                Dispatcher.BeginInvoke(() =>
                {
                    txtAuth.Visibility = Visibility.Collapsed;
                    pBar.Visibility = Visibility.Collapsed;
                    wb.Visibility = Visibility.Visible;
                });
        }
    }
}