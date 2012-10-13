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
using Hammock.Authentication.OAuth;
using Hammock;
using Ocell.Library;
using Hammock.Authentication;
using Hammock.Silverlight.Compat;
using System.Linq;
using Ocell.Library.Twitter;
using Ocell.Library.Notifications;
using DanielVaughan;

namespace Ocell.Pages.Settings
{
    public class TwitterOAuthModel : OAuthModel
    {
        string request_token = "";
        string request_token_secret = "";

        public TwitterOAuthModel()
        {
            callbackUrl = "http://www.google.es";
            APIAuthority = "https://api.twitter.com";
            Version = OAuthVersion.OAuthV1;
        }

        protected override IWebCredentials  GetAuthorizationTokenCredentials()
        {
            OAuthCredentials credentials = new OAuthCredentials()
            {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = SensitiveData.ConsumerToken,
                ConsumerSecret = SensitiveData.ConsumerSecret,
                CallbackUrl = callbackUrl,
                Version = "1.0a"
            };

            return credentials;
        }
        
        protected override RestRequest CreateAuthTokensRequest()
        {
            var request = new RestRequest
            {
                Path = "/oauth/request_token/"
            };

            return request;
        }

        protected override void PreProcessTokenResponse(NameValueCollection collection)
        {
            request_token = collection["oauth_token"];
            request_token_secret = collection["oauth_token_secret"];
        }

        protected override string GetAuthorizationUrl()
        {
            return string.Format("https://api.twitter.com/oauth/authorize?oauth_token={0}", request_token);
        }

        protected override bool VerifyCallbackParams(NameValueCollection parameters)
        {
            return parameters.AllKeys.Contains("oauth_token") && parameters.AllKeys.Contains("oauth_verifier");
        }

        protected override IWebCredentials GetCredentials(NameValueCollection parameters)
        {
            OAuthCredentials credentials = new OAuthCredentials()
            {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = SensitiveData.ConsumerToken,
                ConsumerSecret = SensitiveData.ConsumerSecret,
                Token = parameters["oauth_token"],
                TokenSecret = request_token_secret,
                Verifier = parameters["oauth_verifier"],
                CallbackUrl = callbackUrl,
                Version = "1.0a"
            };

            return credentials;
        }

        protected override RestRequest CreateTokensRequest(NameValueCollection parameters)
        {
            var request = new RestRequest
            {
                Path = "/oauth/access_token/",
                DecompressionMethods = Hammock.Silverlight.Compat.DecompressionMethods.None
            };

            return request;
        }

        protected override void PostProcess(NameValueCollection parameters)
        {
            if (!parameters.AllKeys.Contains("oauth_token") || !parameters.AllKeys.Contains("oauth_token_secret"))
            {
                MessageService.ShowError(Localization.Resources.ErrorClientTokens);
                GoBack();
                return;
            }

            UserToken Token = new UserToken
            {
                Key = parameters["oauth_token"],
                Secret = parameters["oauth_token_secret"],
                Preferences = new NotificationPreferences
                {
                    MentionsPreferences = NotificationType.Tile,
                    MessagesPreferences = NotificationType.Tile
                }
            };

            var filler = new UserTokenFiller(Token);
            filler.UserDataFilled += new UserTokenFiller.OnUserDataFilled(InsertTokenIntoAccounts);
            filler.FillUserData();
        }

        private void InsertTokenIntoAccounts(UserToken Token)
        {
            CheckIfExistsAndInsert(Token);
            CreateColumns(Token);
            GoBack();
            return;
        }

        private void CreateColumns(UserToken user)
        {
            TwitterResource Home = new TwitterResource { Type = ResourceType.Home, User = user };
            TwitterResource Mentions = new TwitterResource { Type = ResourceType.Mentions, User = user };
            TwitterResource Messages = new TwitterResource { Type = ResourceType.Messages, User = user };
            Deployment.Current.Dispatcher.InvokeIfRequired(() =>
            {
                if (!Config.Columns.Contains(Home))
                    Config.Columns.Add(Home);
                if (!Config.Columns.Contains(Mentions))
                    Config.Columns.Add(Mentions);
                if (!Config.Columns.Contains(Messages))
                    Config.Columns.Add(Messages);
                Config.SaveColumns();
            });
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
    }
}
