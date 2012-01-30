using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Web;
using System.Windows;
using Hammock;
using Hammock.Authentication.OAuth;
using Microsoft.Phone.Controls;

namespace Ocell.Settings
{
    public partial class OAuth : PhoneApplicationPage
    {
        public OAuth()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(OAuth_Loaded);
        }

        void OAuth_Loaded(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings config;

            try
            {
                config = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.ToString());
                MessageBox.Show("Sorry, an error has happened while loading the configuration.");
                throw;
            }
            
            string consumerKey;
            string consumerSecret;

            try
            {
                consumerKey = Tokens.consumer_token;
                consumerSecret = Tokens.consumer_secret;
            }
            catch (Exception)
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Error loading app credentials."); });
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                return;
            }

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
            client.BeginRequest(request, new RestCallback(TwitterPostCompleted));
        }

        private void TwitterPostCompleted(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Error while authenticating with Twitter");
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
                return;
            }
                        
            var collection = HttpUtility.ParseQueryString(response.Content);
            string request_token = collection["oauth_token"];
            string token_secret = collection["oauth_token_secret"];
            
            Tokens.request_token = request_token;
            Tokens.request_secret = token_secret;

            Dispatcher.BeginInvoke(() => { wb.Navigate(new Uri("http://api.twitter.com/oauth/authorize?oauth_token=" + request_token)); });
        }



        private void wb_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri.Host.Contains("google"))
            {
                // This is the Twitter callback, so cancel the call and manage the tokens.
                e.Cancel = true;
                string url = e.Uri.Query;
                int index = url.IndexOf("?");
                if (index > 0)
                    url = url.Substring(index).Remove(0, 1);

                var collection = HttpUtility.ParseQueryString(url);

                string token = collection["oauth_token"];
                string verifier = collection["oauth_verifier"];

                string consumerKey;
                string consumerSecret;

                try
                {
                    consumerKey = Tokens.consumer_token;
                    consumerSecret = Tokens.consumer_secret;
                }
                catch (Exception)
                {
                    Dispatcher.BeginInvoke(() => { 
                        MessageBox.Show("Error loading app credentials");
                        NavigationService.Navigate(new Uri("/MainPage.xaml"));
                    });
                    return;
                }

                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(verifier))
                {
                    Dispatcher.BeginInvoke(() => { 
                        MessageBox.Show("Authentication error");
                        NavigationService.Navigate(new Uri("/MainPage.xaml"));
                    });
                    return;
                }

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
        }

        public void RequestCompleted(RestRequest req, RestResponse response, object userstate)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Error while authenticating with Twitter"); });
                return;
            }

            var collection = HttpUtility.ParseQueryString(response.Content);
            
            Tokens.user_token = collection["oauth_token"];
            Tokens.user_secret = collection["oauth_token_secret"];

            Clients.Service = new TweetSharp.TwitterService(Tokens.consumer_token, Tokens.consumer_secret, Tokens.user_token, Tokens.user_secret);
            Clients.isServiceInit = true;
            Clients.fillScreenName();

            Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); });
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