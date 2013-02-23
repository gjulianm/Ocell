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
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Hammock.Silverlight.Compat;
using Hammock.Authentication.OAuth;
using Ocell.Library;
using Hammock;
using Hammock.Authentication;

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
        bool browserVisible;
        public bool BrowserVisible
        {
            get { return browserVisible; }
            set { Assign("BrowserVisible", ref browserVisible, value); }
        }

        public event Navigator BrowserNavigate;
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
                ReturnedToCallback(e.Uri);
            }
        }

        public virtual void BrowserNavigated(NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.StartsWith(AuthAutority))
                BrowserVisible = true;
        }

        public virtual void PageLoaded()
        {
            if (Version == OAuthVersion.OAuthV1)
                GetAuthorizationTokens();
            else
                GetAuthorizationFromUser();
        }
        #endregion

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
        protected abstract bool VerifyCallbackParams(NameValueCollection parameters);

        /// <summary>
        /// Builds the credentials for getting the full tokens.
        /// </summary>
        /// <param name="parameters">Parameters from the authorization reponse.</param>
        /// <returns>Credentials. By default, it returns null.</returns>
        protected virtual IWebCredentials GetCredentials(NameValueCollection parameters)
        {
            return null;
        }

        /// <summary>
        /// Builds the credentials for getting the auth tokens. Should only be used on OAuth 1.
        /// </summary>
        /// <returns>Credentials. By default, it returns null</returns>
        protected virtual IWebCredentials GetAuthorizationTokenCredentials()
        {
            return null;
        }

        /// <summary>
        /// Creates the request for getting the full tokens.
        /// </summary>
        /// <param name="parameters">Parameters from the authorization response.</param>
        /// <returns>RestRequest</returns>
        protected abstract RestRequest CreateTokensRequest(NameValueCollection parameters);

        /// <summary>
        /// Creates the request for getting the auth tokens. Should only be used on OAuth 1.
        /// As this function won't be used much, it's implemented with return null to avoid
        /// having a bunch of useless functions implemented on OAuth 2 models.
        /// </summary>
        /// <returns>RestRequest</returns>
        protected virtual RestRequest CreateAuthTokensRequest()
        {
            return null;
        }

        /// <summary>
        /// This function is called when the used is authenticated and the OAuth flow is over.
        /// </summary>
        /// <param name="parameters">Collection of parameters from the tokens callback.</param>
        protected abstract void PostProcess(string contents);
        
        /// <summary>
        /// Pre process the auth tokens parameters. Should only be used on OAuth 1.
        /// Implemented as an empty method to avoid cluttering on OAuth 2 models.
        /// </summary>
        /// <param name="collection">Collection of parameters.</param>
        protected virtual void PreProcessTokenResponse(NameValueCollection collection)
        {
        }
        #endregion

        #region Tools
        private string GetQueryString(string Query)
        {
            int index = Query.IndexOf("?");
            if (index > 0)
                Query = Query.Substring(index).Remove(0, 1);

            return Query;
        }
        #endregion

        #region OAuth flow
        public virtual void GetAuthorizationTokens()
        {
            var client = new RestClient
            {
                Authority = APIAuthority,
                Credentials = GetAuthorizationTokenCredentials()
            };

            // Get the response from the request
            client.BeginRequest(CreateAuthTokensRequest(), new RestCallback(GetRequestTokenResponse));
        }

        private void GetRequestTokenResponse(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageService.ShowError(Localization.Resources.ErrorAuthURL);
                GoBack();
                return;
            }

            try
            {
                var collection = System.Web.HttpUtility.ParseQueryString(response.Content);
                PreProcessTokenResponse(collection);
            }
            catch (Exception)
            {
                MessageService.ShowError(Localization.Resources.ErrorAuthURL);
                GoBack();
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
            catch (Exception)
            {
                MessageService.ShowError(Localization.Resources.ErrorAuthURL);
                return;
            }

            if (!Uri.TryCreate(authUrl, UriKind.Absolute, out authUri))
                return;

            RaiseNavigate(authUri);
        }

        public virtual void ReturnedToCallback(Uri uri)
        {
            string url = GetQueryString(uri.Query);

            var paramCollection = System.Web.HttpUtility.ParseQueryString(url);

            if (!VerifyCallbackParams(paramCollection))
            {
                MessageService.ShowError(Localization.Resources.ErrorClientTokens);
                GoBack();
                return;
            }

            GetFullTokens(paramCollection);
        }

        public virtual void GetFullTokens(NameValueCollection parameters)
        {
            // Use Hammock to create a rest client
            var client = new RestClient
            {
                Authority = APIAuthority,
                Credentials = GetCredentials(parameters)
            };

            // Get the response from the request
            client.BeginRequest(CreateTokensRequest(parameters), new RestCallback(TokensRequestCompleted));
        }

        public void TokensRequestCompleted(RestRequest req, RestResponse response, object userstate)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                MessageService.ShowError(Localization.Resources.ErrorClientTokens);
                GoBack();
                return;
            }

            try
            {
                PostProcess(response.Content);
            }
            catch (Exception)
            {
                MessageService.ShowError(Localization.Resources.ErrorClientTokens);
                GoBack();
            }
        }
        #endregion
    }

    public delegate void Navigator(object sender, Uri toNavigate);
}
