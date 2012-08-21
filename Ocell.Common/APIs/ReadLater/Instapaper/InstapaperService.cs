using System;
using System.Net;
using Hammock;
using Hammock.Authentication.Basic;

namespace Ocell.Library.ReadLater.Instapaper
{
    public class InstapaperService : IReadLaterService
    {
        protected RestClient _client;
        public string UserName { get; set; }
        public string Password { get; set; }

        protected ReadLaterResult GetResult(RestResponse response)
        {
            switch ((int)response.StatusCode)
            {
                case 200:
                case 201:
                    return ReadLaterResult.Accepted;
                case 403:
                    return ReadLaterResult.AuthenticationFail;
                case 400:
                    return ReadLaterResult.BadRequest;
                case 500:
                    return ReadLaterResult.InternalError;
                default:
                    return ReadLaterResult.Unknown;
            }
        }

        protected void InitRestClient()
        {
            _client = new RestClient();
            _client.Authority = "https://www.instapaper.com/api/";
        }

        protected BasicAuthCredentials GetCredentials()
        {
            if (UserName == null)
                UserName = "";
            if (Password == null)
                Password = "";

            return new BasicAuthCredentials
            {
                Username = UserName,
                Password = Password
            };
        }

        protected void CredentialsCallback(RestRequest request, RestResponse response, object userState)
        {
            Action<bool, ReadLaterResponse> action = userState as Action<bool, ReadLaterResponse>;
            if (action != null)
                action.Invoke(response.StatusCode == HttpStatusCode.OK, new ReadLaterResponse { ResponseBase = response, Result = GetResult(response) });
        }

        /// <summary>
        /// Checks if the credentials are valid.
        /// </summary>
        /// <param name="action">Action to invoke. First parameter tells if credentials are correct.</param>
        public void CheckCredentials(Action<bool, ReadLaterResponse> action)
        {
            if (_client == null)
                InitRestClient();

            RestRequest request = new RestRequest
            {
                Credentials = GetCredentials(),
                Path = "authenticate"
            };

            _client.BeginRequest(request, CredentialsCallback, action);
        }

        protected void AddUrlCallback(RestRequest request, RestResponse response, object userState)
        {
            Action<ReadLaterResponse> action = userState as Action<ReadLaterResponse>;

            if (action != null)
                action.Invoke(new ReadLaterResponse { ResponseBase = response, Result = GetResult(response) });
        }

        /// <summary>
        /// Adds an URL to read later.
        /// </summary>
        /// <param name="url">URL to add.</param>
        /// <param name="action">Callback.</param>
        public void AddUrl(string url, Action<ReadLaterResponse> action)
        {
            if (_client == null)
                InitRestClient();

            RestRequest request = new RestRequest
            {
                Credentials = GetCredentials(),
                Path = "add?url=" + url
            };

            _client.BeginRequest(request, AddUrlCallback, action);
        }

        /// <summary>
        /// Adds an URL to read later with selected text.
        /// </summary>
        /// <param name="url">URL to add.</param>
        /// <param name="tweetText">Tweet text.</param>
        /// <param name="action">Callback.</param>
        public void AddUrl(string url, string tweetText, Action<ReadLaterResponse> action)
        {
            if (_client == null)
                InitRestClient();

            RestRequest request = new RestRequest
            {
                Credentials = GetCredentials(),
                Path = "add?selection=" + tweetText + "&url=" + url
            };

            _client.BeginRequest(request, AddUrlCallback, action);
        }
    }
}
