using Hammock;
using System;
using System.Net;

namespace Ocell.Library.ReadLater.Pocket
{
    public class PocketService : IReadLaterService
    {
        protected RestClient _client;
        public string UserName { get; set; }
        public string Password { get; set; }

        protected void InitRestClient()
        {
            _client = new RestClient();
            _client.Authority = "https://readitlaterlist.com/v2/";
        }

        protected string GetCredentials()
        {
            if (UserName == null)
                UserName = "";
            if (Password == null)
                Password = "";

            return "username=" + UserName + "&password=" + Password;
        }

        protected ReadLaterResult GetResult(RestResponse response)
        {
            switch ((int)response.StatusCode)
            {
                case 200:
                case 201:
                    return ReadLaterResult.Accepted;
                case 400:
                    return ReadLaterResult.BadRequest;
                case 401:
                    return ReadLaterResult.AuthenticationFail;
                case 403:
                    return ReadLaterResult.RateLimitExceeded;
                case 503:
                    return ReadLaterResult.InternalError;
                default:
                    return ReadLaterResult.Unknown;
            }
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
                Path = "auth?" + GetCredentials() + "&apikey=" + SensitiveData.PocketAPIKey
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
                Path = "add?" + GetCredentials() +"&url=" + url + "&apikey=" + SensitiveData.PocketAPIKey
            };

            _client.BeginRequest(request, AddUrlCallback, action);
        }

        /// <summary>
        /// Adds a URL to Pocker with reference to the tweet which generated it.
        /// </summary>
        /// <param name="url">URL to save.</param>
        /// <param name="tweetId">Tweet ID.</param>
        /// <param name="action">Callback.</param>
        public void AddUrl(string url, long tweetId, Action<ReadLaterResponse> action)
        {
            if (_client == null)
                InitRestClient();

            RestRequest request = new RestRequest
            {
                Path = "add?" + GetCredentials() + "&ref_id=" + tweetId.ToString() + "&url=" + url + "&apikey=" + SensitiveData.PocketAPIKey
            };

            _client.BeginRequest(request, AddUrlCallback, action);
        }
    }
}
