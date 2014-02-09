using AncoraMVVM.Rest;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Ocell.Pages.Settings
{
    public enum OAuthVersion { OAuthV1, OAuthV2 };

    public abstract class OAuthModel : ExtendedViewModelBase
    {
        protected string callbackUrl = "";
        protected string APIAuthority = "";
        protected string AuthAutority = "";

        protected OAuthVersion Version = OAuthVersion.OAuthV1;

        #region UI Communication
        public bool BrowserVisible { get; set; }

        public bool IsLoading { get; set; }

        public event Navigator BrowserNavigate;
        private TokenResponse<RequestToken> tokenResponse;
        public void RaiseNavigate(Uri uri)
        {
            if (BrowserNavigate != null)
                BrowserNavigate(this, uri);
        }

        public virtual void BrowserNavigating(NavigatingEventArgs e)
        {
            if (e != null && e.Uri != null && !string.IsNullOrEmpty(e.Uri.Host)
                && e.Uri.AbsoluteUri.StartsWith(callbackUrl))
            {
                e.Cancel = true;
                BrowserVisible = false;
                Progress.IsLoading = true;
                ReturnedToCallback(e.Uri);
            }
        }

        public virtual void BrowserNavigated(NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.StartsWith(AuthAutority))
                BrowserVisible = true;

            Progress.IsLoading = !BrowserVisible;
        }

        public virtual void PageLoaded()
        {
            Progress.IsLoading = true;

            if (Version == OAuthVersion.OAuthV1)
                GetAuthorizationTokens();
            else
                GetAuthorizationFromUser();
        }

        #endregion UI Communication

        #region Url getters & verifiers
        /// <summary>
        /// Build the URL where the user will be redirected to authorize the application.
        /// </summary>
        /// <returns>Url string.</returns>
        protected abstract string GetAuthorizationUrl();

        /// <summary>
        /// Verifies that the parameters received after the user authorization are correct.
        /// </summary>
        /// <param name="parameters">Parameters.</param>
        /// <returns>True if correct, false if not.</returns>
        protected abstract bool VerifyCallbackParams(ParameterCollection parameters);

        /// <summary>
        /// Creates the request for getting the full tokens.
        /// </summary>
        /// <param name="parameters">Parameters from the authorization response.</param>
        /// <returns>RestRequest</returns>
        protected abstract HttpRequestMessage CreateTokensRequest(ParameterCollection parameters);

        /// <summary>
        /// Returns the path for getting the auth tokens. Should only be used on OAuth 1.
        /// As this function won't be used much, it's implemented with return null to avoid
        /// having a bunch of useless functions implemented on OAuth 2 models.
        /// </summary>
        /// <returns>string</returns>
        protected virtual string GetTokenRequestPath()
        {
            throw new NotImplementedException();
        }

        protected virtual string GetAccessTokenPath()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function is called when the used is authenticated and the OAuth flow is over.
        /// </summary>
        /// <param name="parameters">Collection of parameters from the tokens callback.</param>
        protected abstract void PostProcess(ParameterCollection parameters);

        /// <summary>
        /// Pre process the auth tokens parameters. Should only be used on OAuth 1.
        /// Implemented as an empty method to avoid cluttering on OAuth 2 models.
        /// </summary>
        /// <param name="collection">Collection of parameters.</param>
        protected virtual void PreProcessTokenResponse(TokenResponse<RequestToken> response)
        {
        }

        protected virtual OAuthAuthorizer GetOAuthorizer()
        {
            throw new NotImplementedException();
        }

        #endregion Url getters & verifiers

        #region Tools
        private string GetQueryString(string Query)
        {
            int index = Query.IndexOf("?");
            if (index > 0)
                Query = Query.Substring(index).Remove(0, 1);

            return Query;
        }

        private ParameterCollection GetQueryParameters(string query)
        {
            ParameterCollection collection = new ParameterCollection();

            query = query.TrimStart('?');

            var pars = query.Split('&').Select(x => x.Split('=')).Select(x => Tuple.Create(x[0], Uri.UnescapeDataString(x[1])));

            foreach (var pair in pars)
                collection.Add(pair.Item1, pair.Item2);

            return collection;
        }

        #endregion Tools

        #region OAuth flow
        public virtual async void GetAuthorizationTokens()
        {
            var authorizer = GetOAuthorizer();

            // Get the response from the request
            try
            {
                tokenResponse = await authorizer.GetRequestToken(APIAuthority + GetTokenRequestPath());
            }
            catch (HttpRequestException ex)
            {
                DebugError("Error getting request token: {0}", ex);
                Notificator.ShowError(Localization.Resources.ErrorAuthURL);
                Navigator.GoBack();
                return;
            }
            GetRequestTokenResponse(tokenResponse);
        }

        private void GetRequestTokenResponse(TokenResponse<RequestToken> response)
        {
            try
            {
                PreProcessTokenResponse(response);
            }
            catch (Exception e)
            {
                DebugError("Error processing token response: {0}", e);
                Notificator.ShowError(Localization.Resources.ErrorAuthURL);
                Navigator.GoBack();
                return;
            }

            GetAuthorizationFromUser();
        }

        public void GetAuthorizationFromUser()
        {
            Uri authUri;
            string authUrl;

            try
            {
                authUrl = GetAuthorizationUrl();
            }
            catch (Exception e)
            {
                DebugError("Error getting auth URL: {0}", e);
                Notificator.ShowError(Localization.Resources.ErrorAuthURL);
                return;
            }

            if (!Uri.TryCreate(authUrl, UriKind.Absolute, out authUri))
                return;

            RaiseNavigate(authUri);
        }

        public virtual async void ReturnedToCallback(Uri uri)
        {
            string url = GetQueryString(uri.Query);

            var paramCollection = GetQueryParameters(url);

            if (!VerifyCallbackParams(paramCollection))
            {
                DebugError("Callback parameters are not correct. Returned to {0}", uri);
                Notificator.ShowError(Localization.Resources.ErrorClientTokens);
                Navigator.GoBack();
                return;
            }

            await GetFullTokens(paramCollection);
        }

        public virtual async Task GetFullTokens(ParameterCollection parameters)
        {
            // Use Hammock to create a rest client
            var client = new HttpClient();
            client.BaseAddress = new Uri(APIAuthority);
            var returnedParameters = new ParameterCollection();

            if (Version == OAuthVersion.OAuthV1)
                await GetOAuth1AccessTokenParameters(parameters, returnedParameters);
            else
                await GetOAuth2AccessTokenParameters(parameters, returnedParameters);

            PostProcess(returnedParameters);
        }

        private async Task GetOAuth2AccessTokenParameters(ParameterCollection parameters, ParameterCollection returnedParameters)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(APIAuthority, UriKind.Absolute);

            var response = await client.SendAsync(CreateTokensRequest(parameters));

            if (!response.IsSuccessStatusCode)
            {
                DebugError("Error in the token request. Response code {0}, content {1}.", response.StatusCode, await response.Content.ReadAsStringAsync());
                Notificator.ShowError(Localization.Resources.ErrorClientTokens);
                Navigator.GoBack();
                return;
            }

            try
            {
                string contents = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(contents);

                foreach (var key in json)
                    returnedParameters.Add(key.Key, key.Value);
            }
            catch (Exception e)
            {
                DebugError("Error post-processing the token response: {0}", e);
                Notificator.ShowError(Localization.Resources.ErrorClientTokens);
                Navigator.GoBack();
            }
        }

        private async Task GetOAuth1AccessTokenParameters(ParameterCollection parameters, ParameterCollection returnedParameters)
        {
            try
            {
                var response = await GetOAuthorizer().GetAccessToken(APIAuthority + GetAccessTokenPath(), tokenResponse.Token, parameters.First(x => x.Key == "oauth_verifier").Value.ToString());

                returnedParameters.Add("oauth_token", response.Token.Key);
                returnedParameters.Add("oauth_token_secret", response.Token.Secret);
            }
            catch (Exception e)
            {
                DebugError("Error requesting access token parameters (OAuth 1): {0}", e);
                Notificator.ShowError(Localization.Resources.ErrorClientTokens);
                Navigator.GoBack();
                return;
            }
        }

        #endregion OAuth flow

        [Conditional("DEBUG")]
        private void DebugError(string message, params object[] args)
        {
            string msg = String.Format(message, args);
            Debug.WriteLine(msg);
            Notificator.ShowError(msg);
        }
    }

    public delegate void Navigator(object sender, Uri toNavigate);
}