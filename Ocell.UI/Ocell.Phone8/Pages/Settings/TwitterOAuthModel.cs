using AncoraMVVM.Rest;
using AsyncOAuth;

using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Twitter;
using System.Net.Http;
using System.Windows;

namespace Ocell.Pages.Settings
{
    public class TwitterOAuthModel : OAuthModel
    {
        private string request_token = "";
        private string request_token_secret = "";

        public TwitterOAuthModel()
        {
            callbackUrl = "http://www.google.es";
            APIAuthority = "https://api.twitter.com";
            AuthAutority = "https://api.twitter.com";
            Version = OAuthVersion.OAuthV1;
        }

        protected override OAuthAuthorizer GetOAuthorizer()
        {
            return new OAuthAuthorizer(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret);
        }

        protected override string GetTokenRequestPath()
        {
            return "/oauth/request_token/";
        }

        protected override void PreProcessTokenResponse(TokenResponse<RequestToken> response)
        {
            request_token = response.Token.Key;
            request_token_secret = response.Token.Secret;
        }

        protected override string GetAuthorizationUrl()
        {
            return string.Format("https://api.twitter.com/oauth/authorize?oauth_token={0}", request_token);
        }

        protected override string GetAccessTokenPath()
        {
            return "/oauth/access_token";
        }

        protected override bool VerifyCallbackParams(ParameterCollection parameters)
        {
            return parameters.ContainsKey("oauth_token") && parameters.ContainsKey("oauth_verifier");
        }

        protected override HttpRequestMessage CreateTokensRequest(ParameterCollection parameters)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/oauth/access_token/");

            return request;
        }

        protected override async void PostProcess(ParameterCollection parameters)
        {
            if (!parameters.ContainsKey("oauth_token") || !parameters.ContainsKey("oauth_token_secret"))
            {
                MessageService.ShowError(Localization.Resources.ErrorClientTokens);
                GoBack();
                return;
            }

            UserToken token = new UserToken
            {
                Key = parameters["oauth_token"].ToString(),
                Secret = parameters["oauth_token_secret"].ToString(),
                Preferences = new NotificationPreferences
                {
                    MentionsPreferences = NotificationType.TileAndToast,
                    MessagesPreferences = NotificationType.TileAndToast
                }
            };

            var filler = new UserTokenFiller();
            await filler.FillUserData(token);

            CheckIfExistsAndInsert(token);
            CreateColumns(token);
            GoBack();
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